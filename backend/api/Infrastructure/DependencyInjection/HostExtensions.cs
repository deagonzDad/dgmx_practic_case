using Serilog;

namespace api.Infrastructure.DependencyInjection;

public static class HostExtensions
{
    public static void ConfigureLog(this IHostBuilder host)
    {
        host.UseSerilog(
            (context, loggedConf) => loggedConf.ReadFrom.Configuration(context.Configuration)
        );
    }
}
