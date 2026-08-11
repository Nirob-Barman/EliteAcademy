using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainAnnouncement = EliteAcademy.Domain.Entities.Instructor.Announcement;

namespace EliteAcademy.Application.Features.Announcement.Commands.CreateAnnouncement;

public class CreateAnnouncementHandler : IRequestHandler<CreateAnnouncementCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public CreateAnnouncementHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;

        var cls = await _context.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Dto.ClassId, cancellationToken);

        var domainResult = DomainAnnouncement.Create(instructorId, cls, request.Dto.Title, request.Dto.Body);
        if (!domainResult.IsSuccess)
            return Result<bool>.Fail(domainResult.Error);

        var announcement = domainResult.Value!;
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Announcement posted.");
    }
}
