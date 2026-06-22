using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Notification.Commands.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand : IRequest<Result<bool>>;
