using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Announcement.Queries.GetClassAnnouncements;

public record GetClassAnnouncementsQuery(int ClassId) : IRequest<Result<List<AnnouncementDto>>>;
