using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;

namespace EliteAcademy.Application.Mappers
{
    public static class EnrollmentMapper
    {
        public static PreEnrollmentDto ToPreEnrollmentDto(PreEnrollment entity, Class? cls, string? instructorName) => new()
        {
            Id             = entity.Id,
            ClassId        = entity.ClassId,
            ClassName      = cls?.ClassName,
            ClassImage     = cls?.ClassImage,
            InstructorName = instructorName,
            Price          = cls?.Price ?? 0,
            PaymentStatus  = entity.PaymentStatus,
            CouponCode     = entity.CouponCode,
            DiscountAmount = entity.DiscountAmount
        };

        public static EnrollmentDto ToEnrollmentDto(Enrollment entity, Class? cls, string? instructorName) => new()
        {
            Id             = entity.Id,
            ClassId        = entity.ClassId,
            ClassName      = cls?.ClassName,
            ClassImage     = cls?.ClassImage,
            InstructorName = instructorName,
            Price          = cls?.Price ?? 0,
            EnrolledAt     = entity.EnrolledAt
        };
    }
}
