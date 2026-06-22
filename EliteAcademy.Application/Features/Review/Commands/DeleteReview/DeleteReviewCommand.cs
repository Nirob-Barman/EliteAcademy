using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Review.Commands.DeleteReview;

public record DeleteReviewCommand(int ReviewId) : IRequest<Result<bool>>;
