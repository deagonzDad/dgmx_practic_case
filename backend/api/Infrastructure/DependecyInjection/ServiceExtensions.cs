using System.Text;
using api.Data;
using api.DTO.SetttingsDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Infrastructure.Data;
using api.Mappers;
using api.Repository;
using api.Repository.Interfaces;
using api.Services;
using api.Services.Interfaces;
using api.ValidationAttributes;
using api.ValidationAttributes.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Infrastructure.DependecyInjection;

public static class ServiceExtensions
{
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        // services.AddScoped<IReservationRepository, ReservationRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
    }

    public static void ConfigureHelpers(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRegexController, RegexController>();
        services.AddScoped<IErrorHandler, ErrorHandler>();
    }

    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<UsersProfile>();
            cfg.AddProfile<RoomsProfile>();
            cfg.AddProfile<ReservesProfile>();
        });
    }

    public static void ConfigureSections(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettingsDTO>(config.GetSection("Jwt"));
        services.Configure<EncryptKeysDTO>(config.GetSection("Encryption"));
    }

    public static void ConfigureJWTToken(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<JwtTokenGenerator>();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                var jwtSettings =
                    config.GetSection("Jwt").Get<JwtSettingsDTO>()
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
        services
            .AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(config.GetConnectionString("SqliteConnection"))
        );
    }
}
