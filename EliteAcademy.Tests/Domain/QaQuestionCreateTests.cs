using EliteAcademy.Domain.Entities.Student;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class QaQuestionCreateTests
{
    [Fact]
    public void Create_EmptyText_ReturnsFail()
    {
        var result = QaQuestion.Create("student-1", 1, "");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Question cannot be empty.");
    }

    [Fact]
    public void Create_WhitespaceText_ReturnsFail()
    {
        var result = QaQuestion.Create("student-1", 1, "   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Question cannot be empty.");
    }

    [Fact]
    public void Create_ValidText_ReturnsOk()
    {
        var result = QaQuestion.Create("student-1", 1, "  What is this class about?  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
        result.Value!.QuestionText.Should().Be("What is this class about?");
    }
}
