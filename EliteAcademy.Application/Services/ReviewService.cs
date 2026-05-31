using EliteAcademy.Application.Common;
using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public ReviewService(
            IApplicationDbContext context,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _context = context;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<ReviewDto>>> GetClassReviewsAsync(int classId)
        {
            var reviews = await _context.Reviews.AsNoTracking().Where(r => r.ClassId == classId).ToListAsync();

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId);
            var dtos = reviews
                .Select(r => ReviewMapper.ToDto(r,
                    userMap.GetValueOrDefault(r.StudentId ?? ""),
                    cls?.ClassName))
                .ToList();

            return Result<List<ReviewDto>>.Ok(dtos);
        }

        public async Task<Result<Dictionary<int, (double Avg, int Count)>>> GetReviewSummaryAsync()
        {
            var all = await _context.Reviews.AsNoTracking().ToListAsync();
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
            var ids = (await _context.Reviews.AsNoTracking().Where(r => r.StudentId == studentId).ToListAsync())
                .Select(r => r.ClassId)
                .ToHashSet();

            return Result<HashSet<int>>.Ok(ids);
        }

        public async Task<Result<bool>> CreateAsync(ReviewFormDto dto)
        {
            var studentId = _userContextService.UserId!;

            var domainResult = Review.Create(studentId, dto.ClassId, dto.Rating, dto.Comment);
            if (!domainResult.IsSuccess)
                return Result<bool>.FailField("Rating", domainResult.Error);

            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == dto.ClassId);
            if (!isEnrolled)
                return Result<bool>.Fail("You must be enrolled to leave a review.");

            var alreadyReviewed = await _context.Reviews.AnyAsync(r => r.StudentId == studentId && r.ClassId == dto.ClassId);
            if (alreadyReviewed)
                return Result<bool>.Fail("You have already reviewed this class.");

            _context.Reviews.Add(domainResult.Value!);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Review submitted.");
        }

        public async Task<Result<bool>> DeleteAsync(int reviewId)
        {
            var studentId = _userContextService.UserId!;
            var review = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
                return Result<bool>.Fail("Review not found.");
            if (review.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Review deleted.");
        }
    }
}
