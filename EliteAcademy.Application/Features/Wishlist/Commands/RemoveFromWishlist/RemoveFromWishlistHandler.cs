using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistHandler : IRequestHandler<RemoveFromWishlistCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public RemoveFromWishlistHandler(
        IApplicationDbContext context,
        IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var item = await _context.Wishlists.AsNoTracking().FirstOrDefaultAsync(w => w.Id == request.WishlistId, cancellationToken);
        if (item == null)
            return Result<bool>.Fail("Wishlist item not found.");
        if (item.StudentId != studentId)
            return Result<bool>.Fail("Not authorized.");

        _context.Wishlists.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Removed from wishlist.");
    }
}
