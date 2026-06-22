using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.NotificationPreference.Commands.UpdateNotificationPreferences;

public record UpdateNotificationPreferencesCommand(NotificationPreferenceDto Dto) : IRequest<Result<bool>>;
