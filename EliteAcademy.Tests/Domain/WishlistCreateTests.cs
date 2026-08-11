using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class WishlistCreateTests
{
    [Fact]
    public void Create_NullClass_ReturnsFail()
    {
        var result = Wishlist.Create("student-1", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not available.");
    }

    [Fact]
    public void Create_ClassNotApproved_ReturnsFail()
    {
        var cls = new Class { Id = 1 };

        var result = Wishlist.Create("student-1", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not available.");
    }

    [Fact]
    public void Create_ValidClass_ReturnsOk()
    {
        var cls = new Class { Id = 1 };
        cls.Approve();

        var result = Wishlist.Create("student-1", cls);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
    }
}
