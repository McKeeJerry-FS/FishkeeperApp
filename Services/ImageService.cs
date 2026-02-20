using System;
using AquaHub.MVC.Services.Interfaces;
using AquaHub.MVC.Models.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AquaHub.MVC.Services;

public class ImageService : IImageService
{
    private readonly string _defaultProfileImage = "/img/profile.jpg";
    private readonly string _defaultPlantImage = "/img/plant_image.jpg";
    private readonly string _defaultEquipmentImage = "/img/aqua_equipment.jpg";
    private readonly string _defaultTankImage = "/img/journal.jpg";
    private readonly string _defaultNutrientImage = "/img/fertilizer.jpg";
    private readonly string _defaultMaintenanceImage = "/img/maintenance_default.jpg";
    private readonly string _defaultWaterTestImage = "/img/water_test.jpg";
    private readonly ILogger<ImageService> _logger;

    public ImageService(ILogger<ImageService> logger)
    {
        _logger = logger;
    }

    public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
    {
        try
        {
            if (fileData == null || fileData.Length == 0)
            {
                // show default - return null to let the view handle fallback display
                return null;
            }

            // All images are already processed to JPEG in ConvertFileToByteArrayAsync
            string contentType = "image/jpeg";

            string? imageBase64Data = Convert.ToBase64String(fileData!);
            imageBase64Data = $"data:{contentType};base64,{imageBase64Data}";
            return imageBase64Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to base64");
            return null;
        }
    }

    public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return null!;
            }

            _logger.LogInformation("Processing image upload: {FileName}, ContentType: {ContentType}, Size: {Size} bytes",
                file.FileName, file.ContentType, file.Length);

            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            // Log original image dimensions and format
            _logger.LogInformation("Original image: {Width}x{Height}, Format: {Format}",
                image.Width, image.Height, image.Metadata.DecodedImageFormat?.Name ?? "Unknown");

            // Auto-orient the image based on EXIF data (fixes iOS rotation issues)
            image.Mutate(x => x.AutoOrient());

            // Resize if image is too large (max 2000px on longest side)
            const int maxSize = 2000;
            if (image.Width > maxSize || image.Height > maxSize)
            {
                var ratio = Math.Min((double)maxSize / image.Width, (double)maxSize / image.Height);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(newWidth, newHeight));
                _logger.LogInformation("Resized image from original to {Width}x{Height}", newWidth, newHeight);
            }

            // Convert to JPEG format (ensures browser compatibility, handles HEIC/HEIF)
            using var outputStream = new MemoryStream();
            var encoder = new JpegEncoder
            {
                Quality = 85 // Good balance between quality and file size
            };

            await image.SaveAsJpegAsync(outputStream, encoder);
            byte[] imageBytes = outputStream.ToArray();

            _logger.LogInformation("Image processed successfully. Output size: {Size} bytes (JPEG)", imageBytes.Length);

            return imageBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image file: {FileName}", file?.FileName ?? "unknown");
            throw new InvalidOperationException($"Failed to process image: {ex.Message}", ex);
        }
    }
}

