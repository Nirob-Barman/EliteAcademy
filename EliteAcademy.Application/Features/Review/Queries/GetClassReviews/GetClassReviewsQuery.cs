using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Review.Queries.GetClassReviews;

public record GetClassReviewsQuery(int ClassId) : IRequest<Result<List<ReviewDto>>>;
