FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/TuCita.Api/TuCita.Api.csproj src/TuCita.Api/
COPY src/TuCita.Application/TuCita.Application.csproj src/TuCita.Application/
COPY src/TuCita.Domain/TuCita.Domain.csproj src/TuCita.Domain/
COPY src/TuCita.Infrastructure/TuCita.Infrastructure.csproj src/TuCita.Infrastructure/
RUN dotnet restore src/TuCita.Api/TuCita.Api.csproj

COPY src/ src/
RUN dotnet publish src/TuCita.Api/TuCita.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# MailKit necesita libgssapi-krb5-2 para negociar auth SASL con el SMTP;
# la imagen base de Debian slim no la trae por defecto.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .
ENTRYPOINT ["dotnet", "TuCita.Api.dll"]
