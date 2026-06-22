using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Review.Commands.DeleteReview;

public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public DeleteReviewHandler(
        IApplicationDbContext context,
        IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var review = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);
        if (review == null)
            return Result<bool>.Fail("Review not found.");
        if (review.StudentId != studentId)
            return Result<bool>.Fail("Not authorized.");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Review deleted.");
    }
}
