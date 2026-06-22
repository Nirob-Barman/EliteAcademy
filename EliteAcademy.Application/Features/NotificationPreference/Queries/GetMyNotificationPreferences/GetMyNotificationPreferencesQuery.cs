using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.NotificationPreference.Queries.GetMyNotificationPreferences;

public record GetMyNotificationPreferencesQuery : IRequest<Result<NotificationPreferenceDto>>;
