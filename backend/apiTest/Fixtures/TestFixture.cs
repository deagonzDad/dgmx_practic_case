using System;
using System.Data.Common;
using api.Data;
using api.DTO.SettingsDTO;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Repository;
using api.Repository.Interfaces;
using api.Services;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace apiTest.Fixtures;

public class TestFixture : Fixture, IDisposable
{
    private static readonly Random _random = new();

    private readonly DbConnection _connection;
    public AppDbContext DbAppContext { get; }

    // public readonly Mock<IUserRepository> userRepoMock;
    // public readonly Mock<IRoleRepository> roleRepoMock;
    public readonly IUserRepository userRepo;
    public readonly IRoleRepository roleRepo;
    public readonly Mock<IHasher> hasherMock;
    public readonly Mock<IOptions<JwtSettingsDTO>> optionsMock;
    public readonly IJwtTokenGenerator jwtTokenGenerator;
    public readonly Mock<IMapper> mapperMock;
    public readonly Mock<ILogger<AuthService>> loggerMock;
    public readonly ErrorHandler errorHandler;

    public TestFixture()
    {
        Customize(new AutoMoqCustomization());
        Behaviors.Remove(new ThrowingRecursionBehavior());
        Behaviors.Add(new OmitOnRecursionBehavior());

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        DbAppContext = new AppDbContext(options);
        DbAppContext.Database.EnsureCreated();

        optionsMock = this.Freeze<Mock<IOptions<JwtSettingsDTO>>>();
        SetIOptions();

        userRepo = new UserRepository(DbAppContext);
        roleRepo = new RoleRepository(DbAppContext);
        hasherMock = this.Freeze<Mock<IHasher>>();
        mapperMock = this.Freeze<Mock<IMapper>>();
        jwtTokenGenerator = this.Create<IJwtTokenGenerator>();

        loggerMock = this.Freeze<Mock<ILogger<AuthService>>>();
        errorHandler = new ErrorHandler();

        LimitDecimalMaxRange();
        SeedDatabase();
    }

    /// <summary>
    ///   Represents a custom AutoFixture fixture for test setup.
    ///   This class includes common customizations, such as limiting the range
    ///   of generated decimal values to prevent `OverflowException`s
    ///   when dealing with properties constrained by large `double.MaxValue` ranges.
    /// </summary>
    private void LimitDecimalMaxRange()
    {
        Customize<int>(composer => composer.FromFactory(() => _random.Next(1, 101)));
    }

    private void SeedDatabase()
    {
        List<Role> roles = [new() { Name = "Admin" }, new() { Name = "User" }];
        DbAppContext.Roles.AddRange(roles);
        DbAppContext.SaveChanges();

        var adminRole = DbAppContext.Roles.First(r => r.Name == "Admin");

        var users = Build<User>()
            .Without(u => u.Reservations)
            .Without(u => u.Roles)
            .Without(u => u.UserRoles)
            .Without(u => u.Username)
            .Without(u => u.Email)
            .With(u => u.Id, 0)
            .CreateMany(10)
            .ToList();
        int userId = 0;
        foreach (var user in users)
        {
            user.Email = $"user{userId + 1}@examples.com";
            user.Username = $"user{userId + 1}";
            user.UserRoles = [new UserRole { Role = adminRole, User = user }];
            userId++;
        }
        List<Room> rooms = [];
        for (int i = 0; i < 1; i++)
        {
            var newRoom = Build<Room>()
                .Without(r => r.Reservations)
                .With(r => r.RoomNumber, 1 + i)
                .With(r => r.Id, 0)
                .Create();
            rooms.Add(newRoom);
        }

        DbAppContext.Users.AddRange(users);
        DbAppContext.Rooms.AddRange(rooms);
        DbAppContext.SaveChanges();

        var reservations = Build<Reservation>()
            .With(r => r.User, users.First())
            .With(r => r.Room, rooms.First())
            .Without(r => r.Payment)
            .With(r => r.Id, 0)
            .CreateMany(10)
            .ToList();

        DbAppContext.Reservations.AddRange(reservations);
        DbAppContext.SaveChanges();
    }

    private void SetIOptions()
    {
        var jwtSettings = this.Build<JwtSettingsDTO>()
            .With(s => s.Key, this.Create<string>())
            .With(s => s.ExpirationMinutes, this.Create<int>())
            .Create();

        optionsMock.Setup(o => o.Value).Returns(jwtSettings);
        this.Register(() => optionsMock.Object);

        this.Register<IJwtTokenGenerator>(() => this.Create<JwtTokenGenerator>());
    }

    public void Dispose()
    {
        _connection.Dispose();
        DbAppContext.Dispose();
    }
}
