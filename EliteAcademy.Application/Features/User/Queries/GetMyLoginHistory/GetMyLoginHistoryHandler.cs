using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.User.Queries.GetMyLoginHistory;

public class GetMyLoginHistoryHandler : IRequestHandler<GetMyLoginHistoryQuery, Result<List<LoginHistoryItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyLoginHistoryHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<List<LoginHistoryItemDto>>> Handle(GetMyLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result<List<LoginHistoryItemDto>>.Fail("User not authenticated.");

        var records = await _context.LoginAudits.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LoginTime)
            .Take(50)
            .Select(x => new LoginHistoryItemDto
            {
                LoginTime = x.LoginTime,
                IPAddress = x.IPAddress,
                UserAgent = x.UserAgent,
                IsSuccessful = x.IsSuccessful,
                ErrorMessage = x.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        return Result<List<LoginHistoryItemDto>>.Ok(records);
    }
}
