using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Review.Queries.GetReviewSummary;

public record GetReviewSummaryQuery : IRequest<Result<Dictionary<int, (double Avg, int Count)>>>;
