using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class QaAnswerCreateTests
{
    private static Class InstructorClass(string instructorId = "instructor-1") =>
        new() { Id = 1, InstructorId = instructorId };

    [Fact]
    public void Create_EmptyText_ReturnsFail()
    {
        var result = QaAnswer.Create("instructor-1", 1, "", InstructorClass());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Answer cannot be empty.");
    }

    [Fact]
    public void Create_NullClass_ReturnsFail()
    {
        var result = QaAnswer.Create("instructor-1", 1, "Good question!", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Not authorized to answer this question.");
    }

    [Fact]
    public void Create_WrongInstructor_ReturnsFail()
    {
        var result = QaAnswer.Create("instructor-2", 1, "Good question!", InstructorClass());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Not authorized to answer this question.");
    }

    [Fact]
    public void Create_ValidAnswer_ReturnsOk()
    {
        var result = QaAnswer.Create("instructor-1", 1, "  Here is the answer.  ", InstructorClass());

        result.IsSuccess.Should().BeTrue();
        result.Value!.QuestionId.Should().Be(1);
        result.Value!.InstructorId.Should().Be("instructor-1");
        result.Value!.AnswerText.Should().Be("Here is the answer.");
    }
}
