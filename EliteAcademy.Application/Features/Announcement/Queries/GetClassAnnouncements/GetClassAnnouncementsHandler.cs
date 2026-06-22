using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Announcement.Queries.GetClassAnnouncements;

public class GetClassAnnouncementsHandler : IRequestHandler<GetClassAnnouncementsQuery, Result<List<AnnouncementDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetClassAnnouncementsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AnnouncementDto>>> Handle(GetClassAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Announcements.AsNoTracking()
            .Where(a => a.ClassId == request.ClassId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementDto
            {
                Id        = a.Id,
                ClassId   = a.ClassId,
                Title     = a.Title,
                Body      = a.Body,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<AnnouncementDto>>.Ok(items);
    }
}
