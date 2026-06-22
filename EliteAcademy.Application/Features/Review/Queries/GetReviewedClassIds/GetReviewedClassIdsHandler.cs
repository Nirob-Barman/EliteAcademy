using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Review.Queries.GetReviewedClassIds;

public class GetReviewedClassIdsHandler : IRequestHandler<GetReviewedClassIdsQuery, Result<HashSet<int>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetReviewedClassIdsHandler(
        IApplicationDbContext context,
        IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<HashSet<int>>> Handle(GetReviewedClassIdsQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var ids = (await _context.Reviews.AsNoTracking().Where(r => r.StudentId == studentId).ToListAsync(cancellationToken))
            .Select(r => r.ClassId)
            .ToHashSet();

        return Result<HashSet<int>>.Ok(ids);
    }
}
