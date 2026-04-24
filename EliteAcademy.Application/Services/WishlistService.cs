using EliteAcademy.Application.DTOs.Wishlist;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public WishlistService(
            IUnitOfWork unitOfWork,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<WishlistDto>>> GetMyWishlistAsync()
        {
            var studentId = _userContextService.UserId!;
            var items = (await _unitOfWork.Repository<Wishlist>()
                .Where(w => w.StudentId == studentId))
                .ToList();

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var dtos = new List<WishlistDto>();
            foreach (var item in items)
            {
                var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(item.ClassId);
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
            var ids = (await _unitOfWork.Repository<Wishlist>()
                .Where(w => w.StudentId == studentId))
                .Select(w => w.ClassId)
                .ToHashSet();

            return Result<HashSet<int>>.Ok(ids);
        }

        public async Task<Result<bool>> AddAsync(int classId)
        {
            var studentId = _userContextService.UserId!;

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(classId);
            if (cls == null || cls.Status != ClassStatus.Approved)
                return Result<bool>.Fail("Class not available.");

            var alreadyWishlisted = await _unitOfWork.Repository<Wishlist>()
                .AnyAsync(w => w.StudentId == studentId && w.ClassId == classId);
            if (alreadyWishlisted)
                return Result<bool>.Fail("Already in wishlist.");

            var alreadyEnrolled = await _unitOfWork.Repository<Enrollment>()
                .AnyAsync(e => e.StudentId == studentId && e.ClassId == classId);
            if (alreadyEnrolled)
                return Result<bool>.Fail("You are already enrolled in this class.");

            await _unitOfWork.Repository<Wishlist>().AddAsync(new Wishlist
            {
                ClassId   = classId,
                StudentId = studentId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Added to wishlist.");
        }

        public async Task<Result<bool>> RemoveAsync(int wishlistId)
        {
            var studentId = _userContextService.UserId!;
            var item = await _unitOfWork.Repository<Wishlist>().GetByIdAsync(wishlistId);
            if (item == null)
                return Result<bool>.Fail("Wishlist item not found.");
            if (item.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");

            _unitOfWork.Repository<Wishlist>().Remove(item);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Removed from wishlist.");
        }
    }
}
