using App.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace App.Modules.Media.Services;

/// <summary>
/// Dosyaları wwwroot/uploads altına kaydeden yerel disk implementasyonu.
/// Dosya adları çakışmayı önlemek için GUID ile öneklenir.
/// </summary>
public class LocalFileStorage(IWebHostEnvironment environment) : IFileStorage
{
    private const string UploadsFolder = "uploads";

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var directory = Path.Combine(environment.WebRootPath, UploadsFolder);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, safeName);
        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/{UploadsFolder}/{safeName}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        // "/uploads/x.png" -> wwwroot/uploads/x.png (path traversal'a karşı ad temizlenir)
        var name = Path.GetFileName(url);
        var fullPath = Path.Combine(environment.WebRootPath, UploadsFolder, name);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
