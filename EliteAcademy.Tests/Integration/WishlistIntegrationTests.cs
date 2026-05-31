using EliteAcademy.Application.Services;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Infrastructure.Persistence;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EliteAcademy.Tests.Integration;

public class WishlistIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IUserManager> _userMgr  = new();
    private readonly Mock<IUserContextService> _userCtx  = new();
    private const string StudentId = "student-1";

    public WishlistIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _userMgr.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(new List<AppUser>());
    }

    public void Dispose() => _db.Dispose();

    private WishlistService CreateSut() => new(_db, _userMgr.Object, _userCtx.Object);

    private void Seed(Action<ApplicationDbContext> action)
    {
        action(_db);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
    }

    private void SeedClass(int id, ClassStatus status = ClassStatus.Approved, int seats = 10) =>
        Seed(db => db.Classes.Add(new Class { Id = id, ClassName = $"Class {id}", Status = status, AvailableSeats = seats }));

    // ── AddAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidClass_PersistsToDatabase()
    {
        SeedClass(1);

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeTrue();
        _db.Wishlists.Count().Should().Be(1);
        _db.Wishlists.First().StudentId.Should().Be(StudentId);
        _db.Wishlists.First().ClassId.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_ClassNotFound_DoesNotPersist()
    {
        var result = await CreateSut().AddAsync(99);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_AlreadyWishlisted_ReturnsFail()
    {
        SeedClass(1);
        Seed(db => db.Wishlists.Add(new Wishlist { ClassId = 1, StudentId = StudentId }));

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_AlreadyEnrolled_ReturnsFail()
    {
        SeedClass(1);
        Seed(db => db.Enrollments.Add(new Enrollment { ClassId = 1, StudentId = StudentId }));

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(0);
    }

    // ── RemoveAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveAsync_ValidItem_RemovesFromDatabase()
    {
        Seed(db => db.Wishlists.Add(new Wishlist { Id = 1, ClassId = 1, StudentId = StudentId }));

        var result = await CreateSut().RemoveAsync(1);

        result.Success.Should().BeTrue();
        _db.Wishlists.Count().Should().Be(0);
    }

    [Fact]
    public async Task RemoveAsync_WrongStudent_DoesNotRemove()
    {
        Seed(db => db.Wishlists.Add(new Wishlist { Id = 1, ClassId = 1, StudentId = "other-student" }));

        var result = await CreateSut().RemoveAsync(1);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(1);
    }

    // ── GetMyWishlistedClassIdsAsync ──────────────────────────────────────

    [Fact]
    public async Task GetMyWishlistedClassIdsAsync_ReturnsOnlyCurrentStudentIds()
    {
        Seed(db => db.Wishlists.AddRange(
            new Wishlist { ClassId = 1, StudentId = StudentId },
            new Wishlist { ClassId = 2, StudentId = StudentId },
            new Wishlist { ClassId = 3, StudentId = "other-student" }));

        var result = await CreateSut().GetMyWishlistedClassIdsAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new HashSet<int> { 1, 2 });
    }
}
