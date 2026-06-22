using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Notification.Queries.GetMyNotifications;

public record GetMyNotificationsQuery : IRequest<Result<List<NotificationDto>>>;
