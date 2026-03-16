using Microsoft.AspNetCore.Http;

namespace AquaHub.MVC.Utilities;

public static class ImageStorageHelper
{
    public static async Task<string?> SaveImageAsync(
        IWebHostEnvironment environment,
        IFormFile? imageFile,
        params string[] relativeFolderSegments)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return null;
        }

        var uploadFolder = Path.Combine(new[] { environment.WebRootPath }.Concat(relativeFolderSegments).ToArray());
        Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        var relativePath = string.Join("/", relativeFolderSegments.Select(segment => segment.Trim('/')));
        return $"/{relativePath}/{uniqueFileName}";
    }

    public static void DeleteImageIfExists(IWebHostEnvironment environment, string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}