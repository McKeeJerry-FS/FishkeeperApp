using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Services.Interfaces;

public interface IImageService
{
    public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file);
    public string? ConvertByteArrayToFile(byte[]? FileData, string? extension, DefaultImage defaultImage);
}
