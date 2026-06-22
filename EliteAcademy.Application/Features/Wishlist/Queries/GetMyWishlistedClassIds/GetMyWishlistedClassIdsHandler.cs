using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlistedClassIds;

public class GetMyWishlistedClassIdsHandler : IRequestHandler<GetMyWishlistedClassIdsQuery, Result<HashSet<int>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyWishlistedClassIdsHandler(
        IApplicationDbContext context,
        IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<HashSet<int>>> Handle(GetMyWishlistedClassIdsQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var ids = (await _context.Wishlists.AsNoTracking().Where(w => w.StudentId == studentId).ToListAsync(cancellationToken))
            .Select(w => w.ClassId)
            .ToHashSet();

        return Result<HashSet<int>>.Ok(ids);
    }
}
