using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Services;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace EliteAcademy.Tests.Services;

public class WishlistServiceTests
{
    private readonly Mock<IApplicationDbContext> _ctx      = new();
    private readonly Mock<IAsyncQueryExecutor>   _exec     = new();
    private readonly Mock<IUserManager>          _userMgr  = new();
    private readonly Mock<IUserContextService>   _userCtx  = new();

    private const string StudentId = "student-1";

    private WishlistService CreateSut() =>
        new(_ctx.Object, _exec.Object, _userMgr.Object, _userCtx.Object);

    // ── AddAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ClassNotFound_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Classes).Returns(MockQueryable.Of<Class>());
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Class>>(), true, default))
             .ReturnsAsync((Class?)null);

        var result = await CreateSut().AddAsync(99);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Class not available.");
    }

    [Fact]
    public async Task AddAsync_ClassNotApproved_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Classes).Returns(MockQueryable.Of(
            new Class { Id = 1, Status = ClassStatus.Pending }));
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Class>>(), true, default))
             .ReturnsAsync(new Class { Id = 1, Status = ClassStatus.Pending });

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Class not available.");
    }

    [Fact]
    public async Task AddAsync_AlreadyWishlisted_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Classes).Returns(MockQueryable.Of(
            new Class { Id = 1, Status = ClassStatus.Approved }));
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Class>>(), true, default))
             .ReturnsAsync(new Class { Id = 1, Status = ClassStatus.Approved });
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of<Wishlist>());
        _exec.Setup(x => x.AnyAsync(
                It.IsAny<IQueryable<Wishlist>>(), default))
             .ReturnsAsync(true);

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Already in wishlist.");
    }

    [Fact]
    public async Task AddAsync_AlreadyEnrolled_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Classes).Returns(MockQueryable.Of(
            new Class { Id = 1, Status = ClassStatus.Approved }));
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Class>>(), true, default))
             .ReturnsAsync(new Class { Id = 1, Status = ClassStatus.Approved });
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of<Wishlist>());
        _ctx.Setup(x => x.Enrollments).Returns(MockQueryable.Of<Enrollment>());
        _exec.SetupSequence(x => x.AnyAsync(It.IsAny<IQueryable<Wishlist>>(), default))
             .ReturnsAsync(false);
        _exec.Setup(x => x.AnyAsync(It.IsAny<IQueryable<Enrollment>>(), default))
             .ReturnsAsync(true);

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("You are already enrolled in this class.");
    }

    [Fact]
    public async Task AddAsync_ValidClass_AddsAndReturnsOk()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Classes).Returns(MockQueryable.Of(
            new Class { Id = 1, Status = ClassStatus.Approved }));
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Class>>(), true, default))
             .ReturnsAsync(new Class { Id = 1, Status = ClassStatus.Approved });
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of<Wishlist>());
        _ctx.Setup(x => x.Enrollments).Returns(MockQueryable.Of<Enrollment>());
        _exec.Setup(x => x.AnyAsync(It.IsAny<IQueryable<Wishlist>>(), default))
             .ReturnsAsync(false);
        _exec.Setup(x => x.AnyAsync(It.IsAny<IQueryable<Enrollment>>(), default))
             .ReturnsAsync(false);
        _ctx.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateSut().AddAsync(1);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Added to wishlist.");
        _ctx.Verify(x => x.Add(It.Is<Wishlist>(w =>
            w.ClassId == 1 && w.StudentId == StudentId)), Times.Once);
        _ctx.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    // ── RemoveAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveAsync_ItemNotFound_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of<Wishlist>());
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Wishlist>>(), true, default))
             .ReturnsAsync((Wishlist?)null);

        var result = await CreateSut().RemoveAsync(99);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Wishlist item not found.");
    }

    [Fact]
    public async Task RemoveAsync_NotOwner_ReturnsFail()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of<Wishlist>());
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Wishlist>>(), true, default))
             .ReturnsAsync(new Wishlist { Id = 1, StudentId = "other-student" });

        var result = await CreateSut().RemoveAsync(1);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Not authorized.");
    }

    [Fact]
    public async Task RemoveAsync_ValidOwner_RemovesAndReturnsOk()
    {
        var item = new Wishlist { Id = 1, StudentId = StudentId, ClassId = 5 };
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of(item));
        _exec.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<IQueryable<Wishlist>>(), true, default))
             .ReturnsAsync(item);
        _ctx.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateSut().RemoveAsync(1);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Removed from wishlist.");
        _ctx.Verify(x => x.Remove(item), Times.Once);
        _ctx.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    // ── GetMyWishlistedClassIdsAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetMyWishlistedClassIdsAsync_ReturnsCorrectIds()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of(
            new Wishlist { StudentId = StudentId, ClassId = 1 },
            new Wishlist { StudentId = StudentId, ClassId = 3 }));
        _exec.Setup(x => x.ToListAsync(
                It.IsAny<IQueryable<Wishlist>>(), true, default))
             .ReturnsAsync(new List<Wishlist>
             {
                 new() { StudentId = StudentId, ClassId = 1 },
                 new() { StudentId = StudentId, ClassId = 3 }
             });

        var result = await CreateSut().GetMyWishlistedClassIdsAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(new HashSet<int> { 1, 3 });
    }

    // ── GetMyWishlistAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetMyWishlistAsync_EmptyWishlist_ReturnsEmptyList()
    {
        _userCtx.Setup(x => x.UserId).Returns(StudentId);
        _ctx.Setup(x => x.Wishlists).Returns(MockQueryable.Of<Wishlist>());
        _exec.Setup(x => x.ToListAsync(
                It.IsAny<IQueryable<Wishlist>>(), true, default))
             .ReturnsAsync(new List<Wishlist>());
        _userMgr.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(new List<AppUser>());

        var result = await CreateSut().GetMyWishlistAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
