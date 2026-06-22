using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Wishlist.Commands.AddToWishlist;

public record AddToWishlistCommand(int ClassId) : IRequest<Result<bool>>;
