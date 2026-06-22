using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainReview = EliteAcademy.Domain.Entities.Student.Review;

namespace EliteAcademy.Application.Features.Review.Commands.CreateReview;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public CreateReviewHandler(
        IApplicationDbContext context,
        IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var dto = request.Dto;

        var domainResult = DomainReview.Create(studentId, dto.ClassId, dto.Rating, dto.Comment);
        if (!domainResult.IsSuccess)
            return Result<bool>.FailField("Rating", domainResult.Error);

        var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == dto.ClassId, cancellationToken);
        if (!isEnrolled)
            return Result<bool>.Fail("You must be enrolled to leave a review.");

        var alreadyReviewed = await _context.Reviews.AnyAsync(r => r.StudentId == studentId && r.ClassId == dto.ClassId, cancellationToken);
        if (alreadyReviewed)
            return Result<bool>.Fail("You have already reviewed this class.");

        _context.Reviews.Add(domainResult.Value!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Review submitted.");
    }
}
