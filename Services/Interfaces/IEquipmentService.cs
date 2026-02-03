using System;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;

namespace AquaHub.MVC.Services.Interfaces;

public interface IEquipmentService
{
    Task<List<Equipment>> GetAllEquipmentAsync(string userId);
    Task<List<Equipment>> GetEquipmentByTankAsync(int tankId, string userId);
    Task<Equipment?> GetEquipmentByIdAsync(int id, string userId);
    Task<Equipment> CreateEquipmentAsync(Equipment equipment, int tankId, string userId);
    Task<Equipment> UpdateEquipmentAsync(Equipment equipment, string userId);
    Task<bool> DeleteEquipmentAsync(int id, string userId);
    Task<EquipmentDashboardViewModel> GetEquipmentDashboardAsync(int equipmentId, string userId);
}
