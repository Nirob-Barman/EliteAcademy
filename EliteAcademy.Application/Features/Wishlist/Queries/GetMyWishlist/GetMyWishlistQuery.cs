using EliteAcademy.Application.DTOs.Wishlist;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlist;

public record GetMyWishlistQuery : IRequest<Result<List<WishlistDto>>>;
