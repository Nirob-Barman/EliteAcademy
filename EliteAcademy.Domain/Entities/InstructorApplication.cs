using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Domain.Events;

namespace EliteAcademy.Domain.Entities
{
    public class InstructorApplication : BaseEntity
    {
        public string? ApplicantId { get; private set; }   // FK → Identity user
        public string? FullName { get; private set; }
        public string? Email { get; private set; }
        public string? Bio { get; private set; }
        public string? Expertise { get; private set; }
        public string? Motivation { get; private set; }
        public InstructorApplicationStatus Status { get; private set; } = InstructorApplicationStatus.Pending;
        public string? AdminNotes { get; private set; }
        public DateTime? ReviewedAt { get; private set; }

        private InstructorApplication() { } // EF Core

        public static DomainResult<InstructorApplication> Create(
            string applicantId, string? fullName, string? email, string? bio, string? expertise, string? motivation)
        {
            if (string.IsNullOrWhiteSpace(applicantId))
                return DomainResult<InstructorApplication>.Fail("Applicant is required.");

            return DomainResult<InstructorApplication>.Ok(new InstructorApplication
            {
                ApplicantId = applicantId,
                FullName = fullName,
                Email = email,
                Bio = bio,
                Expertise = expertise,
                Motivation = motivation,
                Status = InstructorApplicationStatus.Pending,
                CreatedBy = applicantId,
                CreatedAt = DateTime.UtcNow
            });
        }

        public DomainResult<bool> Approve()
        {
            if (Status != InstructorApplicationStatus.Pending)
                return DomainResult<bool>.Fail("Only pending applications can be approved.");

            Status = InstructorApplicationStatus.Approved;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new InstructorApplicationApprovedEvent(ApplicantId!, FullName!, Email!));
            return DomainResult<bool>.Ok(true);
        }

        public DomainResult<bool> Reject(string adminNotes)
        {
            if (Status != InstructorApplicationStatus.Pending)
                return DomainResult<bool>.Fail("Only pending applications can be rejected.");

            Status = InstructorApplicationStatus.Rejected;
            AdminNotes = adminNotes;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new InstructorApplicationRejectedEvent(ApplicantId!, FullName!, Email!, adminNotes));
            return DomainResult<bool>.Ok(true);
        }
    }
}
