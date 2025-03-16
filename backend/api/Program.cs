using System.Text;
using api.Data;
using api.DTO.Users.Setttings;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.Logger(lc =>
        lc.Filter.ByIncludingOnly(le =>
                le.Level >= LogEventLevel.Warning || le.Properties.ContainsKey("CDebug")
            )
            .WriteTo.File(
                "Logs/log-.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
    )
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddHttpClient();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

    builder.Services.AddScoped<JwtTokenGenerator>();

    builder
        .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            var jwtSettings =
                builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Jwt Section not found");
            ;
            var jwtSecret =
                jwtSettings.Key
                ?? throw new InvalidOperationException("Jwt:Key configuration is missing");
            var EncodignjwtSecret = Encoding.UTF8.GetBytes(jwtSecret);
            opt.SaveToken = true;

            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(EncodignjwtSecret),
            };
        });

    builder
        .Services.AddAuthorizationBuilder()
        .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
        .AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));

    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"))
    );
    var app = builder.Build();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    dbContext.SeedBasicUsers(dbContext);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.MapControllers();
    //add support to logging request with Serilog
    app.UseSerilogRequestLogging();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
