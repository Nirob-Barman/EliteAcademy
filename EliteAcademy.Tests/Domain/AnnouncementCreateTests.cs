using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Enums;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class AnnouncementCreateTests
{
    private static Class ApprovedClass(string instructorId = "instructor-1")
    {
        var cls = new Class { Id = 1, InstructorId = instructorId };
        cls.Approve();
        return cls;
    }

    [Fact]
    public void Create_NullClass_ReturnsFail()
    {
        var result = Announcement.Create("instructor-1", null, "Title", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not found.");
    }

    [Fact]
    public void Create_WrongInstructor_ReturnsFail()
    {
        var result = Announcement.Create("instructor-2", ApprovedClass(), "Title", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not found.");
    }

    [Fact]
    public void Create_ClassNotApproved_ReturnsFail()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1" };

        var result = Announcement.Create("instructor-1", cls, "Title", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You can only post announcements for approved classes.");
    }

    [Fact]
    public void Create_EmptyTitle_ReturnsFail()
    {
        var result = Announcement.Create("instructor-1", ApprovedClass(), "   ", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Title is required.");
    }

    [Fact]
    public void Create_ValidInput_ReturnsOk()
    {
        var result = Announcement.Create("instructor-1", ApprovedClass(), "  New update  ", "  Details here  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.Title.Should().Be("New update");
        result.Value!.Body.Should().Be("Details here");
        result.Value!.CreatedBy.Should().Be("instructor-1");
    }
}
