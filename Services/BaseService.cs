using System;
using AquaHub.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class BaseService
{
    protected readonly IDbContextFactory<ApplicationDbContext> ContextFactory;

    protected BaseService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        ContextFactory = contextFactory;
    }

    protected async Task<ApplicationDbContext> CreateContextAsync()
    {
        return await ContextFactory.CreateDbContextAsync();
    }
}
