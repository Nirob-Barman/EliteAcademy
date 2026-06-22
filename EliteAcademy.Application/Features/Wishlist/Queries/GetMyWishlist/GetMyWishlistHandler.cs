using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Wishlist;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlist;

public class GetMyWishlistHandler : IRequestHandler<GetMyWishlistQuery, Result<List<WishlistDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetMyWishlistHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService)
    {
        _context = context;
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<List<WishlistDto>>> Handle(GetMyWishlistQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var items = await _context.Wishlists.AsNoTracking().Where(w => w.StudentId == studentId).ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

        var dtos = new List<WishlistDto>();
        foreach (var item in items)
        {
            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.ClassId, cancellationToken);
            var instructorName = cls?.InstructorId != null
                ? userMap.GetValueOrDefault(cls.InstructorId, "")
                : "";
            dtos.Add(WishlistMapper.ToDto(item, cls, instructorName));
        }

        return Result<List<WishlistDto>>.Ok(dtos);
    }
}
