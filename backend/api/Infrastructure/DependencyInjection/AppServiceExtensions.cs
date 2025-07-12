using api.Infrastructure.Data;

namespace api.Infrastructure.DependencyInjection;

public static class AppServiceExtensions
{
    public static async Task ConfigureDatabaseScopeAsync(this IServiceProvider service)
    {
        using var scope = service.CreateAsyncScope();
        var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await databaseSeeder.SeedBasicUsers();
    }
}
