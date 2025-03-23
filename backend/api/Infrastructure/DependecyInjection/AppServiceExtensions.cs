using System;
using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.DependecyInjection;

public static class AppServiceExtensions
{
    public static async void ConfigureDatabaseScope(this IServiceProvider service)
    {
        using var scope = service.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await dbContext.SeedBasicUsers(dbContext);
    }
}
