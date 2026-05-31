using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Wishlist;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public WishlistService(
            IApplicationDbContext context,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _context = context;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<WishlistDto>>> GetMyWishlistAsync()
        {
            var studentId = _userContextService.UserId!;
            var items = await _context.Wishlists.AsNoTracking().Where(w => w.StudentId == studentId).ToListAsync();

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var dtos = new List<WishlistDto>();
            foreach (var item in items)
            {
                var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.ClassId);
                var instructorName = cls?.InstructorId != null
                    ? userMap.GetValueOrDefault(cls.InstructorId, "")
                    : "";
                dtos.Add(WishlistMapper.ToDto(item, cls, instructorName));
            }

            return Result<List<WishlistDto>>.Ok(dtos);
        }

        public async Task<Result<HashSet<int>>> GetMyWishlistedClassIdsAsync()
        {
            var studentId = _userContextService.UserId!;
            var ids = (await _context.Wishlists.AsNoTracking().Where(w => w.StudentId == studentId).ToListAsync())
                .Select(w => w.ClassId)
                .ToHashSet();

            return Result<HashSet<int>>.Ok(ids);
        }

        public async Task<Result<bool>> AddAsync(int classId)
        {
            var studentId = _userContextService.UserId!;

            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId);

            var domainResult = Wishlist.Create(studentId, cls);
            if (!domainResult.IsSuccess)
                return Result<bool>.Fail(domainResult.Error);

            var alreadyWishlisted = await _context.Wishlists.AnyAsync(w => w.StudentId == studentId && w.ClassId == classId);
            if (alreadyWishlisted)
                return Result<bool>.Fail("Already in wishlist.");

            var alreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == classId);
            if (alreadyEnrolled)
                return Result<bool>.Fail("You are already enrolled in this class.");

            _context.Wishlists.Add(domainResult.Value!);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Added to wishlist.");
        }

        public async Task<Result<bool>> RemoveAsync(int wishlistId)
        {
            var studentId = _userContextService.UserId!;
            var item = await _context.Wishlists.AsNoTracking().FirstOrDefaultAsync(w => w.Id == wishlistId);
            if (item == null)
                return Result<bool>.Fail("Wishlist item not found.");
            if (item.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Removed from wishlist.");
        }
    }
}
