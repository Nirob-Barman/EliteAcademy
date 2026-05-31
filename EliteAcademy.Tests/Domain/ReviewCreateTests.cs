using EliteAcademy.Domain.Entities.Student;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class ReviewCreateTests
{
    [Fact]
    public void Create_RatingZero_ReturnsFail()
    {
        var result = Review.Create("student-1", 1, 0, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Rating must be between 1 and 5.");
    }

    [Fact]
    public void Create_RatingSix_ReturnsFail()
    {
        var result = Review.Create("student-1", 1, 6, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Rating must be between 1 and 5.");
    }

    [Fact]
    public void Create_ValidRating_ReturnsOk()
    {
        var result = Review.Create("student-1", 1, 4, "Great class!");

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
        result.Value!.Rating.Should().Be(4);
        result.Value!.Comment.Should().Be("Great class!");
    }
}
