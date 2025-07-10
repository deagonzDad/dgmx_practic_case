using System.Text;
using api.Common;
using api.Data;
using api.DTO.SettingsDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Infrastructure.Data;
using api.Mappers;
using api.Repository;
using api.Repository.Interfaces;
using api.Services;
using api.Services.Interfaces;
using api.ValidationAttributes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Infrastructure.DependencyInjection;

public static class ServiceExtensions
{
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IUserService, UserService>();
    }

    public static void ConfigureHelpers(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRegexController, RegexController>();
        services.AddScoped<IErrorHandler, ErrorHandler>();
        services.AddScoped<IEncrypter, Encrypter>();
    }

    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<UsersProfile>();
            cfg.AddProfile<RoomsProfile>();
            cfg.AddProfile<ReservesProfile>();
            cfg.AddProfile<PaymentProfile>();
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
                JwtSettingsDTO jwtSettings =
                    config.GetSection("Jwt").Get<JwtSettingsDTO>()
                    ?? throw new InvalidOperationException("Jwt Section not found");
                byte[] EncodingJwtSecret = Encoding.UTF8.GetBytes(jwtSettings.Key);
                opt.SaveToken = true;

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(EncodingJwtSecret),
                };
            });
        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                "AdminOnly",
                options =>
                {
                    options.RequireAuthenticatedUser();
                    options.RequireRole(AppRoles.Admin);
                }
            )
            .AddPolicy(
                "UserOnly",
                options =>
                {
                    options.RequireAuthenticatedUser();
                    options.RequireRole(AppRoles.User);
                }
            )
            .AddPolicy(
                "AdminWithEmail",
                options =>
                {
                    options.RequireAuthenticatedUser();
                    options.RequireRole(AppRoles.Admin);
                    options.RequireClaim("email_verified", "true");
                }
            );
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(config.GetConnectionString("SqliteConnection"))
        );
    }
}
