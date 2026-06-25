using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record InstructorApplicationApprovedEvent(string ApplicantId, string FullName, string Email) : IDomainEvent;
