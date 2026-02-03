using System;

namespace AquaHub.MVC.Models;

public class Heater : Equipment
{
    public decimal MinTemperature { get; set; }
    public decimal MaxTemperature { get; set; }
}
