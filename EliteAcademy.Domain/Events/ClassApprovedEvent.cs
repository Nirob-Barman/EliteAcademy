using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record ClassApprovedEvent(int ClassId, string InstructorId, string ClassName) : IDomainEvent;
