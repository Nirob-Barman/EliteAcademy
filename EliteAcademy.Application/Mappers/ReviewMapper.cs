using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Domain.Entities.Student;

namespace EliteAcademy.Application.Mappers
{
    public static class ReviewMapper
    {
        public static ReviewDto ToDto(Review entity, string? studentName = null, string? className = null) => new()
        {
            Id          = entity.Id,
            ClassId     = entity.ClassId,
            ClassName   = className,
            StudentId   = entity.StudentId,
            StudentName = studentName,
            Rating      = entity.Rating,
            Comment     = entity.Comment,
            CreatedAt   = entity.CreatedAt
        };
    }
}
