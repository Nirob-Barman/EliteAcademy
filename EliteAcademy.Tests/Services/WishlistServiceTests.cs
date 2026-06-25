using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Features.Wishlist.Commands.AddToWishlist;
using EliteAcademy.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlist;
using EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlistedClassIds;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EliteAcademy.Tests.Services;

public class WishlistHandlerTests
{
    private readonly Mock<IApplicationDbContext> _ctx = new();
    private readonly Mock<IUserManager> _userMgr = new();
    private readonly Mock<IUserContextService> _userCtx = new();

    private const string StudentId = "student-1";

    private void SetupDbSet<T>(System.Linq.Expressions.Expression<Func<IApplicationDbContext, DbSet<T>>> prop,
        List<T> data) where T : class
    {
        var mock = MockDbSet.Create(data);
        _ctx.Setup(prop).Returns(mock.Object);
    }

    // ── AddToWishlistHandler ─────────────────────────────────────────────

    [Fact]
    public async Task AddToWishlist_ClassNotFound_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Classes, new List<Class>());

        var handler = new AddToWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new AddToWishlistCommand(99), default);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Class not available.");
    }

    [Fact]
    public async Task AddToWishlist_ClassNotApproved_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Classes, new List<Class> { new() { Id = 1, Status = ClassStatus.Pending } });

        var handler = new AddToWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Class not available.");
    }

    [Fact]
    public async Task AddToWishlist_AlreadyWishlisted_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Classes, new List<Class> { new() { Id = 1, Status = ClassStatus.Approved } });
        SetupDbSet(x => x.Wishlists, new List<Wishlist> { new() { StudentId = StudentId, ClassId = 1 } });

        var handler = new AddToWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Already in wishlist.");
    }

    [Fact]
    public async Task AddToWishlist_AlreadyEnrolled_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Classes, new List<Class> { new() { Id = 1, Status = ClassStatus.Approved } });
        SetupDbSet(x => x.Wishlists, new List<Wishlist>());
        SetupDbSet(x => x.Enrollments, new List<Enrollment> { new() { StudentId = StudentId, ClassId = 1 } });

        var handler = new AddToWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("You are already enrolled in this class.");
    }

    [Fact]
    public async Task AddToWishlist_ValidClass_AddsAndReturnsOk()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        var mockWishlists = MockDbSet.Create(new List<Wishlist>());
        SetupDbSet(x => x.Classes, new List<Class> { new() { Id = 1, Status = ClassStatus.Approved } });
        _ctx.Setup(x => x.Wishlists).Returns(mockWishlists.Object);
        SetupDbSet(x => x.Enrollments, new List<Enrollment>());
        _ctx.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = new AddToWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new AddToWishlistCommand(1), default);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Added to wishlist.");
        mockWishlists.Verify(x => x.Add(It.Is<Wishlist>(w =>
            w.ClassId == 1 && w.StudentId == StudentId)), Times.Once);
    }

    // ── RemoveFromWishlistHandler ────────────────────────────────────────

    [Fact]
    public async Task RemoveFromWishlist_ItemNotFound_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Wishlists, new List<Wishlist>());

        var handler = new RemoveFromWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new RemoveFromWishlistCommand(99), default);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Wishlist item not found.");
    }

    [Fact]
    public async Task RemoveFromWishlist_NotOwner_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Wishlists, new List<Wishlist> { new() { Id = 1, StudentId = "other-student" } });

        var handler = new RemoveFromWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new RemoveFromWishlistCommand(1), default);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Not authorized.");
    }

    [Fact]
    public async Task RemoveFromWishlist_ValidOwner_RemovesAndReturnsOk()
    {
        var item = new Wishlist { Id = 1, StudentId = StudentId, ClassId = 5 };
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        var mockWishlists = MockDbSet.Create(new List<Wishlist> { item });
        _ctx.Setup(x => x.Wishlists).Returns(mockWishlists.Object);
        _ctx.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = new RemoveFromWishlistHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new RemoveFromWishlistCommand(1), default);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Removed from wishlist.");
        mockWishlists.Verify(x => x.Remove(item), Times.Once);
    }

    // ── GetMyWishlistedClassIdsHandler ───────────────────────────────────

    [Fact]
    public async Task GetMyWishlistedClassIds_ReturnsCorrectIds()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Wishlists, new List<Wishlist>
        {
            new() { StudentId = StudentId, ClassId = 1 },
            new() { StudentId = StudentId, ClassId = 3 }
        });

        var handler = new GetMyWishlistedClassIdsHandler(_ctx.Object, _userCtx.Object);
        var result = await handler.Handle(new GetMyWishlistedClassIdsQuery(), default);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new HashSet<int> { 1, 3 });
    }

    // ── GetMyWishlistHandler ─────────────────────────────────────────────

    [Fact]
    public async Task GetMyWishlist_EmptyWishlist_ReturnsEmptyList()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        SetupDbSet(x => x.Wishlists, new List<Wishlist>());
        _userMgr.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(new List<AppUser>());

        var handler = new GetMyWishlistHandler(_ctx.Object, _userMgr.Object, _userCtx.Object);
        var result = await handler.Handle(new GetMyWishlistQuery(), default);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
