using App.SharedKernel.Domain;

namespace App.Modules.Media.Domain;

/// <summary>
/// Yüklenen dosyanın meta verisi. GraphQL'de `mediaFiles` sorgusu otomatik oluşur.
/// </summary>
public class MediaFile : BaseEntity, IAggregateRoot
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? UploadedBy { get; set; }
}
