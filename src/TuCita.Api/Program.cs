using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TuCita.Api.Endpoints;
using TuCita.Api.Middleware;
using TuCita.Application.Common;
using TuCita.Application.DependencyInjection;
using TuCita.Infrastructure.DependencyInjection;
using TuCita.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

// Render (y la mayoría de PaaS) asignan el puerto en runtime vía $PORT y
// terminan TLS en su propio proxy, reenviando tráfico plano al contenedor.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // El proxy de Render no tiene una IP fija conocida de antemano.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (isDevelopment)
        {
            // En local el puerto del frontend cambia según cuál esté libre
            // (Next.js salta al siguiente si 3000 está ocupado): en vez de
            // mantener a mano una lista de puertos permitidos, aceptamos
            // cualquier origen localhost/127.0.0.1. En producción seguimos
            // usando la lista explícita de abajo.
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && uri.Host is "localhost" or "127.0.0.1");
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("La sección 'Jwt' no está configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProfessionalOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, AccountTypeClaims.Professional));
    options.AddPolicy("BusinessOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, AccountTypeClaims.Business));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, AccountTypeClaims.Admin));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapProfessionalEndpoints();
app.MapAppointmentEndpoints();
app.MapBusinessEndpoints();
app.MapAdminEndpoints();
app.MapWhatsAppWebhookEndpoints();

app.Run();
