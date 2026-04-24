using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<Result<List<ReviewDto>>>                       GetClassReviewsAsync(int classId);
        Task<Result<Dictionary<int, (double Avg, int Count)>>> GetReviewSummaryAsync();
        Task<Result<HashSet<int>>>                          GetReviewedClassIdsAsync();
        Task<Result<bool>>                                  CreateAsync(ReviewFormDto dto);
        Task<Result<bool>>                                  DeleteAsync(int reviewId);
    }
}
