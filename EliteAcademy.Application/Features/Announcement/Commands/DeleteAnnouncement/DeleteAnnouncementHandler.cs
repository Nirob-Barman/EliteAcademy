using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Announcement.Commands.DeleteAnnouncement;

public class DeleteAnnouncementHandler : IRequestHandler<DeleteAnnouncementCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public DeleteAnnouncementHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;

        var entity = await _context.Announcements.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null)
            return Result<bool>.Fail("Announcement not found.");

        var cls = await _context.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == entity.ClassId, cancellationToken);

        if (cls?.InstructorId != instructorId)
            return Result<bool>.Fail("Not authorized.");

        _context.Announcements.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Announcement deleted.");
    }
}
