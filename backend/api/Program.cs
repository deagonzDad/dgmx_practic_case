using System.Text;
using api.Data;
using api.DTO.SetttingsDTO;
using api.Helpers;
using api.Infrastructure;
using api.Infrastructure.DependecyInjection;
using api.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

// Log.Logger = new LoggerConfiguration()
//     .MinimumLevel.Debug()
//     .WriteTo.Console()
//     .WriteTo.Logger(lc =>
//         lc.Filter.ByIncludingOnly(le =>
//                 le.Level >= LogEventLevel.Warning || le.Properties.ContainsKey("CDebug")
//             )
//             .WriteTo.File(
//                 "Logs/log-dev-.txt",
//                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
//             )
//     )
//     .CreateLogger();
try
{
    Log.Information("Starting WebApp");
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    builder.Host.ConfigureLog();
    builder.Services.AddControllers();
    //add custom extensions
    builder.Services.ConfigureRepositories();
    builder.Services.ConfigureServices();
    builder.Services.ConfigureHelpers();
    builder.Services.ConfigureJWTToken(builder.Configuration);
    builder.Services.ConfigureAutoMapper();
    builder.Services.ConfigureDatabase(builder.Configuration);

    builder.Services.AddHttpClient();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.Services.ConfigureDatabaseScope();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.MapControllers();
    app.UseMiddleware<RequestLogContextMiddleware>();
    //add support to logging request with Serilog
    app.UseSerilogRequestLogging();
    app.Run();
}
catch (HostAbortedException)
{
    Log.Information("Host terminated expectedly by EF Core CLI");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("shutdown complete");
    Log.CloseAndFlush();
}
