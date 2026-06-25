using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record InstructorApplicationRejectedEvent(string ApplicantId, string FullName, string Email, string AdminNotes) : IDomainEvent;
