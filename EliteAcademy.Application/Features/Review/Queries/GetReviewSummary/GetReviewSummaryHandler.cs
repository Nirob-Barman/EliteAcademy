using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Review.Queries.GetReviewSummary;

public class GetReviewSummaryHandler : IRequestHandler<GetReviewSummaryQuery, Result<Dictionary<int, (double Avg, int Count)>>>
{
    private readonly IApplicationDbContext _context;

    public GetReviewSummaryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Dictionary<int, (double Avg, int Count)>>> Handle(GetReviewSummaryQuery request, CancellationToken cancellationToken)
    {
        var all = await _context.Reviews.AsNoTracking().ToListAsync(cancellationToken);
        var summary = all
            .GroupBy(r => r.ClassId)
            .ToDictionary(
                g => g.Key,
                g => (Avg: g.Average(r => (double)r.Rating), Count: g.Count()));

        return Result<Dictionary<int, (double, int)>>.Ok(summary);
    }
}
