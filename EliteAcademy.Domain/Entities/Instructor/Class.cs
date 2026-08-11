using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Domain.Events;

namespace EliteAcademy.Domain.Entities.Instructor
{
    public class Class : BaseEntity
    {
        public string? ClassName { get; private set; }
        public string? ClassImage { get; set; }
        public string? InstructorId { get; private set; }
        public int AvailableSeats { get; private set; }
        public decimal Price { get; private set; }
        public ClassStatus Status { get; private set; } = ClassStatus.Pending;
        public string? Feedback { get; private set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<PreEnrollment> PreEnrollments { get; set; } = new List<PreEnrollment>();

        public static DomainResult<Class> Create(string instructorId, string className, int availableSeats, decimal price)
        {
            var validationError = Validate(className, availableSeats, price);
            if (validationError != null)
                return DomainResult<Class>.Fail(validationError);

            return DomainResult<Class>.Ok(new Class
            {
                ClassName = className.Trim(),
                AvailableSeats = availableSeats,
                Price = price,
                InstructorId = instructorId,
                CreatedBy = instructorId,
                CreatedAt = DateTime.UtcNow
            });
        }

        public DomainResult<bool> UpdateDetails(string className, int availableSeats, decimal price)
        {
            var validationError = Validate(className, availableSeats, price);
            if (validationError != null)
                return DomainResult<bool>.Fail(validationError);

            ClassName = className.Trim();
            AvailableSeats = availableSeats;
            Price = price;
            UpdatedAt = DateTime.UtcNow;

            return DomainResult<bool>.Ok(true);
        }

        private static string? Validate(string className, int availableSeats, decimal price)
        {
            if (string.IsNullOrWhiteSpace(className))
                return "Class name is required.";
            if (availableSeats <= 0)
                return "Available seats must be greater than zero.";
            if (price < 0)
                return "Price cannot be negative.";

            return null;
        }

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
