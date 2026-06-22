using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Review.Queries.GetClassReviews;

public class GetClassReviewsHandler : IRequestHandler<GetClassReviewsQuery, Result<List<ReviewDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetClassReviewsHandler(
        IApplicationDbContext context,
        IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetClassReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews.AsNoTracking().Where(r => r.ClassId == request.ClassId).ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        var dtos = reviews
            .Select(r => ReviewMapper.ToDto(r,
                userMap.GetValueOrDefault(r.StudentId ?? ""),
                cls?.ClassName))
            .ToList();

        return Result<List<ReviewDto>>.Ok(dtos);
    }
}
