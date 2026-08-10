namespace TuCita.Application.Abstractions;

/// <summary>
/// Datos que el navegador necesita para subir un archivo directo a
/// Cloudinary (signed upload), sin que el binario pase por nuestra API.
/// </summary>
public sealed record UploadSignatureResult(
    string Signature,
    long Timestamp,
    string ApiKey,
    string CloudName,
    string Folder,
    string Transformation);
