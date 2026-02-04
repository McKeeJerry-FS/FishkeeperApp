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
                // show default
                switch (defaultImage)
                {
                    case DefaultImage.ProfileImage:
                        return _defaultProfileImage;
                    case DefaultImage.PlantImage:
                        return _defaultPlantImage;
                    case DefaultImage.EquipmentImage:
                        return _defaultEquipmentImage;
                    case DefaultImage.TankImage:
                        return _defaultTankImage;
                    case DefaultImage.NutrientImage:
                        return _defaultNutrientImage;
                }
            }
            string? imageBase64Data = Convert.ToBase64String(fileData!);
            imageBase64Data = string.Format($"data:{extension};base64, {imageBase64Data}");
            return imageBase64Data;
        }
        catch (Exception)
        {

            throw;
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

