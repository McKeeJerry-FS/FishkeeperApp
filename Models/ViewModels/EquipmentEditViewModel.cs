using System;
using System.ComponentModel.DataAnnotations;

namespace AquaHub.MVC.Models.ViewModels
{
    public class EquipmentEditViewModel
    {
        public int Id { get; set; }
        public int TankId { get; set; }
        [Required]
        public string Brand { get; set; } = string.Empty;
        [Required]
        public string Model { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        public DateTime InstalledOn { get; set; }
        public string EquipmentType { get; set; } = string.Empty;
        // Add additional fields for each equipment type as needed
        // Example for Filter:
        public string? FilterType { get; set; }
        public double? FlowRate { get; set; }
        public string? Media { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        // Example for Heater:
        public double? MinTemperature { get; set; }
        public double? MaxTemperature { get; set; }
        // Add more fields for other types as needed

        // Image upload support
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }
    }
}
