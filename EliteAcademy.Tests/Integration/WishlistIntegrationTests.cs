using EliteAcademy.Application.Features.Wishlist.Commands.AddToWishlist;
using EliteAcademy.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlistedClassIds;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Infrastructure.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EliteAcademy.Tests.Integration;

public class WishlistIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IUserManager> _userMgr = new();
    private readonly Mock<IUserContextService> _userCtx = new();
    private const string StudentId = "student-1";

    public WishlistIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var mediator = new Mock<IMediator>();
        _db = new ApplicationDbContext(options, mediator.Object);
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _userMgr.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(new List<AppUser>());
    }

    public void Dispose() => _db.Dispose();

    private void Seed(Action<ApplicationDbContext> action)
    {
        action(_db);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
    }

    private void SeedClass(int id, ClassStatus status = ClassStatus.Approved, int seats = 10) =>
        Seed(db =>
        {
            var cls = new Class { Id = id, ClassName = $"Class {id}", AvailableSeats = seats };
            if (status == ClassStatus.Approved)
                cls.Approve();
            else if (status == ClassStatus.Rejected)
                cls.Reject("Rejected for test setup");
            db.Classes.Add(cls);
        });

    // ── AddToWishlistHandler ──────────────────────────────────────────────

    [Fact]
    public async Task AddToWishlist_ValidClass_PersistsToDatabase()
    {
        SeedClass(1);
        var handler = new AddToWishlistHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeTrue();
        _db.Wishlists.Count().Should().Be(1);
        _db.Wishlists.First().StudentId.Should().Be(StudentId);
        _db.Wishlists.First().ClassId.Should().Be(1);
    }

    [Fact]
    public async Task AddToWishlist_ClassNotFound_DoesNotPersist()
    {
        var handler = new AddToWishlistHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new AddToWishlistCommand(99), default);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(0);
    }

    [Fact]
    public async Task AddToWishlist_AlreadyWishlisted_ReturnsFail()
    {
        SeedClass(1);
        Seed(db => db.Wishlists.Add(new Wishlist { ClassId = 1, StudentId = StudentId }));
        var handler = new AddToWishlistHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(1);
    }

    [Fact]
    public async Task AddToWishlist_AlreadyEnrolled_ReturnsFail()
    {
        SeedClass(1);
        Seed(db => db.Enrollments.Add(new Enrollment { ClassId = 1, StudentId = StudentId }));
        var handler = new AddToWishlistHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(0);
    }

    // ── RemoveFromWishlistHandler ─────────────────────────────────────────

    [Fact]
    public async Task RemoveFromWishlist_ValidItem_RemovesFromDatabase()
    {
        Seed(db => db.Wishlists.Add(new Wishlist { Id = 1, ClassId = 1, StudentId = StudentId }));
        var handler = new RemoveFromWishlistHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new RemoveFromWishlistCommand(1), default);

        result.Success.Should().BeTrue();
        _db.Wishlists.Count().Should().Be(0);
    }

    [Fact]
    public async Task RemoveFromWishlist_WrongStudent_DoesNotRemove()
    {
        Seed(db => db.Wishlists.Add(new Wishlist { Id = 1, ClassId = 1, StudentId = "other-student" }));
        var handler = new RemoveFromWishlistHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new RemoveFromWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        _db.Wishlists.Count().Should().Be(1);
    }

    // ── GetMyWishlistedClassIdsHandler ────────────────────────────────────

    [Fact]
    public async Task GetMyWishlistedClassIds_ReturnsOnlyCurrentStudentIds()
    {
        Seed(db => db.Wishlists.AddRange(
            new Wishlist { ClassId = 1, StudentId = StudentId },
            new Wishlist { ClassId = 2, StudentId = StudentId },
            new Wishlist { ClassId = 3, StudentId = "other-student" }));
        var handler = new GetMyWishlistedClassIdsHandler(_db, _userCtx.Object);

        var result = await handler.Handle(new GetMyWishlistedClassIdsQuery(), default);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new HashSet<int> { 1, 2 });
    }
}
