using System;
using AquaHub.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class BaseService
{
    protected readonly ApplicationDbContext Context;

    protected BaseService(ApplicationDbContext context)
    {
        Context = context;
    }
}
