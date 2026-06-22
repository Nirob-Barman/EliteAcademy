using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Announcement.Commands.DeleteAnnouncement;

public record DeleteAnnouncementCommand(int Id) : IRequest<Result<bool>>;
