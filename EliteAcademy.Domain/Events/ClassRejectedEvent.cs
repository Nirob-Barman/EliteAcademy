using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record ClassRejectedEvent(int ClassId, string InstructorId, string ClassName, string Feedback) : IDomainEvent;
