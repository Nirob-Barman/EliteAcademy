using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities.Instructor
{
    public class Announcement : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public static DomainResult<Announcement> Create(string instructorId, Class? cls, string title, string body)
        {
            if (cls == null || cls.InstructorId != instructorId)
                return DomainResult<Announcement>.Fail("Class not found.");
            if (cls.Status != ClassStatus.Approved)
                return DomainResult<Announcement>.Fail("You can only post announcements for approved classes.");
            if (string.IsNullOrWhiteSpace(title))
                return DomainResult<Announcement>.Fail("Title is required.");

            return DomainResult<Announcement>.Ok(new Announcement
            {
                ClassId = cls.Id,
                Title = title.Trim(),
                Body = body.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = instructorId
            });
        }
    }
}
