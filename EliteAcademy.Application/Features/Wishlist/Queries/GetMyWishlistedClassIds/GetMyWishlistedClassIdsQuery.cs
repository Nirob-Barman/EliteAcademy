using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlistedClassIds;

public record GetMyWishlistedClassIdsQuery : IRequest<Result<HashSet<int>>>;
