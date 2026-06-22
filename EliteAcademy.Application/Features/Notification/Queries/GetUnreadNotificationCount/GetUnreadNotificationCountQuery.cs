using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Notification.Queries.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery : IRequest<Result<int>>;
