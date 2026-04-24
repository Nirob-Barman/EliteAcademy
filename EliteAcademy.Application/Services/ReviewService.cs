using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;

namespace EliteAcademy.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public ReviewService(
            IUnitOfWork unitOfWork,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<ReviewDto>>> GetClassReviewsAsync(int classId)
        {
            var reviews = (await _unitOfWork.Repository<Review>()
                .Where(r => r.ClassId == classId))
                .ToList();

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(classId);
            var dtos = reviews
                .Select(r => ReviewMapper.ToDto(r,
                    userMap.GetValueOrDefault(r.StudentId ?? ""),
                    cls?.ClassName))
                .ToList();

            return Result<List<ReviewDto>>.Ok(dtos);
        }

        public async Task<Result<Dictionary<int, (double Avg, int Count)>>> GetReviewSummaryAsync()
        {
            var all = (await _unitOfWork.Repository<Review>().GetAllAsync()).ToList();
            var summary = all
                .GroupBy(r => r.ClassId)
                .ToDictionary(
                    g => g.Key,
                    g => (Avg: g.Average(r => (double)r.Rating), Count: g.Count()));

            return Result<Dictionary<int, (double, int)>>.Ok(summary);
        }

        public async Task<Result<HashSet<int>>> GetReviewedClassIdsAsync()
        {
            var studentId = _userContextService.UserId!;
            var ids = (await _unitOfWork.Repository<Review>()
                .Where(r => r.StudentId == studentId))
                .Select(r => r.ClassId)
                .ToHashSet();

            return Result<HashSet<int>>.Ok(ids);
        }

        public async Task<Result<bool>> CreateAsync(ReviewFormDto dto)
        {
            var studentId = _userContextService.UserId!;

            if (dto.Rating < 1 || dto.Rating > 5)
                return Result<bool>.FailField("Rating", "Rating must be between 1 and 5.");

            var isEnrolled = await _unitOfWork.Repository<Enrollment>()
                .AnyAsync(e => e.StudentId == studentId && e.ClassId == dto.ClassId);
            if (!isEnrolled)
                return Result<bool>.Fail("You must be enrolled to leave a review.");

            var alreadyReviewed = await _unitOfWork.Repository<Review>()
                .AnyAsync(r => r.StudentId == studentId && r.ClassId == dto.ClassId);
            if (alreadyReviewed)
                return Result<bool>.Fail("You have already reviewed this class.");

            await _unitOfWork.Repository<Review>().AddAsync(new Review
            {
                ClassId   = dto.ClassId,
                StudentId = studentId,
                Rating    = dto.Rating,
                Comment   = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Review submitted.");
        }

        public async Task<Result<bool>> DeleteAsync(int reviewId)
        {
            var studentId = _userContextService.UserId!;
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
            if (review == null)
                return Result<bool>.Fail("Review not found.");
            if (review.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");

            _unitOfWork.Repository<Review>().Remove(review);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Review deleted.");
        }
    }
}
