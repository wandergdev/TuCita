using TuCita.Domain.Identifiers;

namespace TuCita.Application.Admin.BusinessPhotos.RejectBusinessPhoto;

public sealed record RejectBusinessPhotoCommand(BusinessPhotoId BusinessPhotoId);
