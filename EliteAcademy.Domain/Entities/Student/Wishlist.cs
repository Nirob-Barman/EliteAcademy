using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities.Student
{
    public class Wishlist : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string? StudentId { get; set; }

        public static DomainResult<Wishlist> Create(string studentId, Class? cls)
        {
            if (cls == null || cls.Status != ClassStatus.Approved)
                return DomainResult<Wishlist>.Fail("Class not available.");

            return DomainResult<Wishlist>.Ok(new Wishlist
            {
                ClassId = cls.Id,
                StudentId = studentId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
        }
    }
}
