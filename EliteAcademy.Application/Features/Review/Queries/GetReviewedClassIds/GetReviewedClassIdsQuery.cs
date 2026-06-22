using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Review.Queries.GetReviewedClassIds;

public record GetReviewedClassIdsQuery : IRequest<Result<HashSet<int>>>;
