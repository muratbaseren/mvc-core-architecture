using App.Application.Abstractions;
using App.Modules.Media.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace App.Modules.Media.Controllers;

[Authorize]
public class MediaController(
    IRepository<MediaFile> repository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var files = repository.Query()
            .OrderByDescending(f => f.CreatedAt)
            .ToList();
        return View(files);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        var maxMb = configuration.GetValue("Media:MaxFileSizeMb", 10);

        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Lütfen bir dosya seçin.";
            return RedirectToAction(nameof(Index));
        }

        if (file.Length > maxMb * 1024L * 1024L)
        {
            TempData["Error"] = $"Dosya boyutu en fazla {maxMb} MB olabilir.";
            return RedirectToAction(nameof(Index));
        }

        await using var stream = file.OpenReadStream();
        var url = await fileStorage.SaveAsync(stream, file.FileName, cancellationToken);

        await repository.AddAsync(new MediaFile
        {
            FileName = file.FileName,
            Url = url,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedBy = User.Identity?.Name
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        TempData["Success"] = $"\"{file.FileName}\" yüklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var mediaFile = await repository.GetByIdAsync(id, cancellationToken);
        if (mediaFile is null)
            return NotFound();

        await fileStorage.DeleteAsync(mediaFile.Url, cancellationToken);
        repository.Remove(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        TempData["Success"] = $"\"{mediaFile.FileName}\" silindi.";
        return RedirectToAction(nameof(Index));
    }
}
