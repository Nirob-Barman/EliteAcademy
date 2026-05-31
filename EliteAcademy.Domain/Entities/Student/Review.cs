using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Instructor;

namespace EliteAcademy.Domain.Entities.Student
{
    public class Review : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string? StudentId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }

        public static DomainResult<Review> Create(string studentId, int classId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                return DomainResult<Review>.Fail("Rating must be between 1 and 5.");

            return DomainResult<Review>.Ok(new Review
            {
                ClassId = classId,
                StudentId = studentId,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
        }
    }
}
