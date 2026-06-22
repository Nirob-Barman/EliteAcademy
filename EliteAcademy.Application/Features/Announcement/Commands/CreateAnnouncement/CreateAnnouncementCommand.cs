using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Announcement.Commands.CreateAnnouncement;

public record CreateAnnouncementCommand(AnnouncementFormDto Dto) : IRequest<Result<bool>>;
