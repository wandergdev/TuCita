using Microsoft.Extensions.Options;
using TuCita.Infrastructure.Options;

namespace TuCita.Api.Endpoints;

/// <summary>
/// Webhook que Meta llama para verificar el callback URL y para avisar
/// eventos (mensajes entrantes, estados de entrega). TuCita solo envía
/// mensajes por ahora, así que el POST únicamente confirma recepción:
/// no procesamos el contenido, pero hay que responder 200 rápido o Meta
/// reintenta y puede terminar deshabilitando el webhook.
/// </summary>
public static class WhatsAppWebhookEndpoints
{
    public static void MapWhatsAppWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/whatsapp/webhook", (
            HttpRequest request,
            IOptions<MetaWhatsAppOptions> options) =>
        {
            var mode = request.Query["hub.mode"].ToString();
            var token = request.Query["hub.verify_token"].ToString();
            var challenge = request.Query["hub.challenge"].ToString();

            var verifyToken = options.Value.VerifyToken;
            if (mode != "subscribe" || string.IsNullOrEmpty(verifyToken) || token != verifyToken)
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            return Results.Text(challenge);
        }).WithTags("WhatsApp");

        app.MapPost("/api/whatsapp/webhook", () => Results.Ok())
            .WithTags("WhatsApp");
    }
}
