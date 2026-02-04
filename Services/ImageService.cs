using System;
using AquaHub.MVC.Services.Interfaces;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Services;

public class ImageService : IImageService
{
    private readonly string _defaultProfileImage = "/img/profile.jpg";
    private readonly string _defaultPlantImage = "/img/plant_Image";
    private readonly string _defaultEquipmentImage = "/img/equipment.jpg";
    private readonly string _defaultTankImage = "/img/journal.jpg";
    private readonly string _defaultNutrientImage = "/img/fertilizer.jpg";



    public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                // show default - return null to let the view handle fallback display
                return null;
            }

            // Normalize content type for iOS compatibility
            string contentType = extension?.ToLower().Trim() ?? "image/jpeg";

            // Handle various iOS formats and ensure browser compatibility
            if (contentType.Contains("heic") || contentType.Contains("heif"))
            {
                // HEIC/HEIF not supported in browsers, but we'll try to display it
                // In production, you'd want to convert these to JPEG server-side
                contentType = "image/jpeg";
            }
            else if (!contentType.StartsWith("image/"))
            {
                // Ensure proper mime type format
                contentType = "image/jpeg";
            }

            string? imageBase64Data = Convert.ToBase64String(fileData!);
            // Fixed: removed space after comma in data URL
            imageBase64Data = $"data:{contentType};base64,{imageBase64Data}";
            return imageBase64Data;
        }
        catch (Exception ex)
        {
            // Log the error and return null so the view can show a fallback
            Console.WriteLine($"Error converting image: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
    {
        try
        {
            if (file != null)
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            return null!;
        }
        catch (Exception)
        {

            throw;
        }
    }
}

