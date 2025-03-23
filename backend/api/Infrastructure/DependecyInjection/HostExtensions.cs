using System;
using Serilog;

namespace api.Infrastructure.DependecyInjection;

public static class HostExtensions
{
    public static void ConfigureLog(this IHostBuilder host)
    {
        host.UseSerilog(
            (context, loggedConf) => loggedConf.ReadFrom.Configuration(context.Configuration)
        );
    }
}
