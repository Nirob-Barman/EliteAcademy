using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using FluentAssertions;

namespace EliteAcademy.Tests.Domain;

public class EntityFactoryTests
{
    // ── Wishlist ─────────────────────────────────────────────────────────

    [Fact]
    public void Wishlist_Create_NullClass_ReturnsFail()
    {
        var result = Wishlist.Create("student-1", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not available.");
    }

    [Fact]
    public void Wishlist_Create_ClassNotApproved_ReturnsFail()
    {
        var cls = new Class { Id = 1, Status = ClassStatus.Pending };

        var result = Wishlist.Create("student-1", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not available.");
    }

    [Fact]
    public void Wishlist_Create_ValidClass_ReturnsOk()
    {
        var cls = new Class { Id = 1, Status = ClassStatus.Approved };

        var result = Wishlist.Create("student-1", cls);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
    }

    // ── PreEnrollment ────────────────────────────────────────────────────

    [Fact]
    public void PreEnrollment_Create_NullClass_ReturnsFail()
    {
        var result = PreEnrollment.Create("student-1", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class is not available.");
    }

    [Fact]
    public void PreEnrollment_Create_ClassNotApproved_ReturnsFail()
    {
        var cls = new Class { Id = 1, Status = ClassStatus.Rejected, AvailableSeats = 5 };

        var result = PreEnrollment.Create("student-1", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class is not available.");
    }

    [Fact]
    public void PreEnrollment_Create_NoSeats_ReturnsFail()
    {
        var cls = new Class { Id = 1, Status = ClassStatus.Approved, AvailableSeats = 0 };

        var result = PreEnrollment.Create("student-1", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No available seats.");
    }

    [Fact]
    public void PreEnrollment_Create_ValidClass_ReturnsOk()
    {
        var cls = new Class { Id = 1, Status = ClassStatus.Approved, AvailableSeats = 5 };

        var result = PreEnrollment.Create("student-1", cls);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
        result.Value!.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    // ── Review ───────────────────────────────────────────────────────────

    [Fact]
    public void Review_Create_RatingZero_ReturnsFail()
    {
        var result = Review.Create("student-1", 1, 0, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Rating must be between 1 and 5.");
    }

    [Fact]
    public void Review_Create_RatingSix_ReturnsFail()
    {
        var result = Review.Create("student-1", 1, 6, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Rating must be between 1 and 5.");
    }

    [Fact]
    public void Review_Create_ValidRating_ReturnsOk()
    {
        var result = Review.Create("student-1", 1, 4, "Great class!");

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
        result.Value!.Rating.Should().Be(4);
        result.Value!.Comment.Should().Be("Great class!");
    }

    // ── QaQuestion ───────────────────────────────────────────────────────

    [Fact]
    public void QaQuestion_Create_EmptyText_ReturnsFail()
    {
        var result = QaQuestion.Create("student-1", 1, "");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Question cannot be empty.");
    }

    [Fact]
    public void QaQuestion_Create_WhitespaceText_ReturnsFail()
    {
        var result = QaQuestion.Create("student-1", 1, "   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Question cannot be empty.");
    }

    [Fact]
    public void QaQuestion_Create_ValidText_ReturnsOk()
    {
        var result = QaQuestion.Create("student-1", 1, "  What is this class about?  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.StudentId.Should().Be("student-1");
        result.Value!.QuestionText.Should().Be("What is this class about?");
    }

    // ── QaAnswer ─────────────────────────────────────────────────────────

    [Fact]
    public void QaAnswer_Create_EmptyText_ReturnsFail()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1" };

        var result = QaAnswer.Create("instructor-1", 1, "", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Answer cannot be empty.");
    }

    [Fact]
    public void QaAnswer_Create_NullClass_ReturnsFail()
    {
        var result = QaAnswer.Create("instructor-1", 1, "Good question!", null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Not authorized to answer this question.");
    }

    [Fact]
    public void QaAnswer_Create_WrongInstructor_ReturnsFail()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1" };

        var result = QaAnswer.Create("instructor-2", 1, "Good question!", cls);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Not authorized to answer this question.");
    }

    [Fact]
    public void QaAnswer_Create_ValidAnswer_ReturnsOk()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1" };

        var result = QaAnswer.Create("instructor-1", 1, "  Here is the answer.  ", cls);

        result.IsSuccess.Should().BeTrue();
        result.Value!.QuestionId.Should().Be(1);
        result.Value!.InstructorId.Should().Be("instructor-1");
        result.Value!.AnswerText.Should().Be("Here is the answer.");
    }

    // ── Announcement ─────────────────────────────────────────────────────

    [Fact]
    public void Announcement_Create_NullClass_ReturnsFail()
    {
        var result = Announcement.Create("instructor-1", null, "Title", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not found.");
    }

    [Fact]
    public void Announcement_Create_WrongInstructor_ReturnsFail()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1", Status = ClassStatus.Approved };

        var result = Announcement.Create("instructor-2", cls, "Title", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Class not found.");
    }

    [Fact]
    public void Announcement_Create_ClassNotApproved_ReturnsFail()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1", Status = ClassStatus.Pending };

        var result = Announcement.Create("instructor-1", cls, "Title", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You can only post announcements for approved classes.");
    }

    [Fact]
    public void Announcement_Create_EmptyTitle_ReturnsFail()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1", Status = ClassStatus.Approved };

        var result = Announcement.Create("instructor-1", cls, "   ", "Body");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Title is required.");
    }

    [Fact]
    public void Announcement_Create_ValidInput_ReturnsOk()
    {
        var cls = new Class { Id = 1, InstructorId = "instructor-1", Status = ClassStatus.Approved };

        var result = Announcement.Create("instructor-1", cls, "  New update  ", "  Details here  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.ClassId.Should().Be(1);
        result.Value!.Title.Should().Be("New update");
        result.Value!.Body.Should().Be("Details here");
        result.Value!.CreatedBy.Should().Be("instructor-1");
    }
}
