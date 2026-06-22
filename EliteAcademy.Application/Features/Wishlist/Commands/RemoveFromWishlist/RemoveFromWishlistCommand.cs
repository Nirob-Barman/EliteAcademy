using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(int WishlistId) : IRequest<Result<bool>>;
