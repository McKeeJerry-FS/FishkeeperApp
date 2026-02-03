using System;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;

namespace AquaHub.MVC.Services.Interfaces;

public interface ITankService
{
    Task<List<Tank>> GetAllTanksAsync(string userId);
    Task<Tank?> GetTankByIdAsync(int id, string userId);
    Task<Tank> CreateTankAsync(Tank tank, string userId);
    Task<Tank> UpdateTankAsync(Tank tank, string userId);
    Task<bool> DeleteTankAsync(int id, string userId);
    Task<TankDashboardViewModel> GetTankDashboardAsync(int tankId, string userId, int month, int year);
}
