using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadFolder = "uploads/images";

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveImageAsync(Stream fileStream, string fileName)
    {
        try
        {
            // Create upload directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath, _uploadFolder);
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save the file
            using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOutput);

            // Return relative path for storage in database
            return $"/{_uploadFolder}/{uniqueFileName}";
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteImageAsync(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return false;

        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
