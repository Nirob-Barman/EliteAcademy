using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainWishlist = EliteAcademy.Domain.Entities.Student.Wishlist;

namespace EliteAcademy.Application.Features.Wishlist.Commands.AddToWishlist;

public class AddToWishlistHandler : IRequestHandler<AddToWishlistCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public AddToWishlistHandler(
        IApplicationDbContext context,
        IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

        var domainResult = DomainWishlist.Create(studentId, cls);
        if (!domainResult.IsSuccess)
            return Result<bool>.Fail(domainResult.Error);

        var alreadyWishlisted = await _context.Wishlists.AnyAsync(w => w.StudentId == studentId && w.ClassId == request.ClassId, cancellationToken);
        if (alreadyWishlisted)
            return Result<bool>.Fail("Already in wishlist.");

        var alreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == request.ClassId, cancellationToken);
        if (alreadyEnrolled)
            return Result<bool>.Fail("You are already enrolled in this class.");

        _context.Wishlists.Add(domainResult.Value!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Added to wishlist.");
    }
}
