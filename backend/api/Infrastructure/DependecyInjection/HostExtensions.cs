using Serilog;

namespace api.Infrastructure.DependecyInjection;

public static class HostExtensions
{
    public static void ConfigureLog(this IHostBuilder host)
    {
        try
        {
            host.UseSerilog(
                (context, loggedConf) => loggedConf.ReadFrom.Configuration(context.Configuration)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error here");
            Console.WriteLine(ex);
            throw;
        }
    }
}
