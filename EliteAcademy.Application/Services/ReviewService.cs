using EliteAcademy.Application.Common.Interfaces;
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
        private readonly IApplicationDbContext _context;
        private readonly IAsyncQueryExecutor _executor;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public ReviewService(
            IApplicationDbContext context,
            IAsyncQueryExecutor executor,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _context = context;
            _executor = executor;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<ReviewDto>>> GetClassReviewsAsync(int classId)
        {
            var reviews = await _executor.ToListAsync(_context.Reviews.Where(r => r.ClassId == classId), noTracking: true);

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var cls = await _executor.FirstOrDefaultAsync(_context.Classes.Where(c => c.Id == classId), noTracking: true);
            var dtos = reviews
                .Select(r => ReviewMapper.ToDto(r,
                    userMap.GetValueOrDefault(r.StudentId ?? ""),
                    cls?.ClassName))
                .ToList();

            return Result<List<ReviewDto>>.Ok(dtos);
        }

        public async Task<Result<Dictionary<int, (double Avg, int Count)>>> GetReviewSummaryAsync()
        {
            var all = await _executor.ToListAsync(_context.Reviews, noTracking: true);
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
            var ids = (await _executor.ToListAsync(_context.Reviews.Where(r => r.StudentId == studentId), noTracking: true))
                .Select(r => r.ClassId)
                .ToHashSet();

            return Result<HashSet<int>>.Ok(ids);
        }

        public async Task<Result<bool>> CreateAsync(ReviewFormDto dto)
        {
            var studentId = _userContextService.UserId!;

            if (dto.Rating < 1 || dto.Rating > 5)
                return Result<bool>.FailField("Rating", "Rating must be between 1 and 5.");

            var isEnrolled = await _executor.AnyAsync(_context.Enrollments.Where(e => e.StudentId == studentId && e.ClassId == dto.ClassId));
            if (!isEnrolled)
                return Result<bool>.Fail("You must be enrolled to leave a review.");

            var alreadyReviewed = await _executor.AnyAsync(_context.Reviews.Where(r => r.StudentId == studentId && r.ClassId == dto.ClassId));
            if (alreadyReviewed)
                return Result<bool>.Fail("You have already reviewed this class.");

            _context.Add(new Review
            {
                ClassId = dto.ClassId,
                StudentId = studentId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Review submitted.");
        }

        public async Task<Result<bool>> DeleteAsync(int reviewId)
        {
            var studentId = _userContextService.UserId!;
            var review = await _executor.FirstOrDefaultAsync(_context.Reviews.Where(r => r.Id == reviewId), noTracking: true);
            if (review == null)
                return Result<bool>.Fail("Review not found.");
            if (review.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");

            _context.Remove(review);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Review deleted.");
        }
    }
}
