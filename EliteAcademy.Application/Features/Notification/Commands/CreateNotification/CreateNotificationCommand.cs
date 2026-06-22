using MediatR;

namespace EliteAcademy.Application.Features.Notification.Commands.CreateNotification;

public record CreateNotificationCommand(string UserId, string Title, string Message, string? Link)
    : IRequest<Unit>;
