using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class PreEnrollmentCreateTests
{
    [Fact]
    public void Create_NullClass_ReturnsFail()
    {
        var result = PreEnrollment.Create("student-1", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class is not available.");
    }

    [Fact]
    public void Create_ClassNotApproved_ReturnsFail()
    {
        var cls = new Class { Id = 1, AvailableSeats = 5 };
        cls.Reject("Not eligible");

        var result = PreEnrollment.Create("student-1", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class is not available.");
    }

    [Fact]
    public void Create_NoSeats_ReturnsFail()
    {
        var cls = new Class { Id = 1, AvailableSeats = 0 };
        cls.Approve();

        var result = PreEnrollment.Create("student-1", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No available seats.");
    }

    [Fact]
    public void Create_ValidClass_ReturnsOk()
    {
        var cls = new Class { Id = 1, AvailableSeats = 5 };
        cls.Approve();

        var result = PreEnrollment.Create("student-1", cls);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
        result.Value!.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }
}
