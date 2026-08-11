using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Domain.Events;

namespace EliteAcademy.Domain.Entities.Instructor
{
    public class Class : BaseEntity
    {
        public string? ClassName { get; set; }
        public string? ClassImage { get; set; }
        public string? InstructorId { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
        public ClassStatus Status { get; private set; } = ClassStatus.Pending;
        public string? Feedback { get; private set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<PreEnrollment> PreEnrollments { get; set; } = new List<PreEnrollment>();

        public DomainResult<bool> Approve()
        {
            if (Status != ClassStatus.Pending)
                return DomainResult<bool>.Fail("Only pending classes can be approved.");

            Status = ClassStatus.Approved;
            Feedback = null;
            UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(InstructorId))
                AddDomainEvent(new ClassApprovedEvent(Id, InstructorId, ClassName!));

            return DomainResult<bool>.Ok(true);
        }

        public DomainResult<bool> Reject(string feedback)
        {
            if (Status != ClassStatus.Pending)
                return DomainResult<bool>.Fail("Only pending classes can be rejected.");

            Status = ClassStatus.Rejected;
            Feedback = feedback;
            UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(InstructorId))
                AddDomainEvent(new ClassRejectedEvent(Id, InstructorId, ClassName!, feedback));

            return DomainResult<bool>.Ok(true);
        }

        public DomainResult<bool> DecrementSeat()
        {
            if (AvailableSeats <= 0)
                return DomainResult<bool>.Fail("No available seats remaining.");

            AvailableSeats--;
            UpdatedAt = DateTime.UtcNow;

            return DomainResult<bool>.Ok(true);
        }
    }
}
