using System;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;

namespace AquaHub.MVC.Services.Interfaces;

public interface ILivestockService
{
    Task<List<Livestock>> GetAllLivestockAsync(string userId);
    Task<List<Livestock>> GetLivestockByTankAsync(int tankId, string userId);
    Task<Livestock?> GetLivestockByIdAsync(int id, string userId);
    Task<Livestock> CreateLivestockAsync(Livestock livestock, int tankId, string userId);
    Task<Livestock> UpdateLivestockAsync(Livestock livestock, string userId);
    Task<bool> DeleteLivestockAsync(int id, string userId);
    Task<LivestockDashboardViewModel> GetLivestockDashboardAsync(int livestockId, string userId);
}
