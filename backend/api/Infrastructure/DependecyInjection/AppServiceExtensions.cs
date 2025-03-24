using api.Infrastructure.Data;

namespace api.Infrastructure.DependecyInjection;

public static class AppServiceExtensions
{
    public static async void ConfigureDatabaseScope(this IServiceProvider service)
    {
        using var scope = service.CreateAsyncScope();
        var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await databaseSeeder.SeedBasicUsers();
    }
}
