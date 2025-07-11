using api.Infrastructure;
using api.Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

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
    builder.Services.ConfigureSections(builder.Configuration);
    builder.Services.ConfigureAutoMapper();
    builder.Services.ConfigureDatabase(builder.Configuration);

    builder.Services.AddHttpClient();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "DGX Hotels", Version = "v1" });
        OpenApiSecurityScheme securityScheme = new()
        {
            Name = "JWT Authentication",
            Description = "Enter your JWT Token here",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        };
        opt.AddSecurityDefinition("Bearer", securityScheme);
        OpenApiSecurityRequirement securityRequirement = new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        };
        opt.AddSecurityRequirement(securityRequirement);
    });

    var app = builder.Build();

    await app.Services.ConfigureDatabaseScopeAsync();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseMiddleware<RequestLogContextMiddleware>();
    //add support to logging request with Serilog
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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
