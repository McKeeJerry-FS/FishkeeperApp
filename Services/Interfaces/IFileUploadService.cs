using System;

namespace AquaHub.MVC.Services.Interfaces;

public interface IFileUploadService
{
    Task<string?> SaveImageAsync(Stream fileStream, string fileName);
    Task<bool> DeleteImageAsync(string? imagePath);
}
