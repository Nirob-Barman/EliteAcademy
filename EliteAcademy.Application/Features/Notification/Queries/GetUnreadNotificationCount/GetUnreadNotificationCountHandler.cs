using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Notification.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountHandler : IRequestHandler<GetUnreadNotificationCountQuery, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetUnreadNotificationCountHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<int>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (userId == null) return Result<int>.Ok(0);

        var count = await _context.AppNotifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
        return Result<int>.Ok(count);
    }
}
