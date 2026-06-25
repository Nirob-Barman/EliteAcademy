using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record AnnouncementPostedEvent(int ClassId, string ClassName, string Title) : IDomainEvent;
