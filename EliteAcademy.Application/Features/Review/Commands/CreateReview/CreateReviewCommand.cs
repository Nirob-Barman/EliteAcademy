using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Review.Commands.CreateReview;

public record CreateReviewCommand(ReviewFormDto Dto) : IRequest<Result<bool>>;
