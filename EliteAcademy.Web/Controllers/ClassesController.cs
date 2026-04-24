using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [AllowAnonymous]
    public class ClassesController : Controller
    {
        private readonly IClassService _classService;
        private readonly IStudentService _studentService;
        private readonly IReviewService _reviewService;
        private readonly IWishlistService _wishlistService;

        public ClassesController(
            IClassService classService,
            IStudentService studentService,
            IReviewService reviewService,
            IWishlistService wishlistService)
        {
            _classService    = classService;
            _studentService  = studentService;
            _reviewService   = reviewService;
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Index()
        {
            var classResult   = await _classService.GetApprovedAsync();
            var classes       = classResult.Data ?? new List<ClassDto>();
            var summaryResult = await _reviewService.GetReviewSummaryAsync();
            var summary       = summaryResult.Data ?? new();

            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
            {
                var statusResult    = await _studentService.GetEnrollmentStatusAsync();
                var selected        = statusResult.Success ? statusResult.Data.Selected : new HashSet<int>();
                var enrolled        = statusResult.Success ? statusResult.Data.Enrolled : new HashSet<int>();
                var wishlistResult  = await _wishlistService.GetMyWishlistedClassIdsAsync();
                var wishlisted      = wishlistResult.Success ? wishlistResult.Data : new HashSet<int>();

                return View(classes.Select(c => new ClassIndexItemViewModel
                {
                    Class         = c,
                    IsSelected    = selected.Contains(c.Id),
                    IsEnrolled    = enrolled.Contains(c.Id),
                    IsWishlisted  = wishlisted!.Contains(c.Id),
                    AverageRating = summary.TryGetValue(c.Id, out var r) ? r.Avg   : 0,
                    ReviewCount   = summary.TryGetValue(c.Id, out var r2) ? r2.Count : 0
                }).ToList());
            }

            return View(classes.Select(c => new ClassIndexItemViewModel
            {
                Class         = c,
                AverageRating = summary.TryGetValue(c.Id, out var r)  ? r.Avg    : 0,
                ReviewCount   = summary.TryGetValue(c.Id, out var r2) ? r2.Count : 0
            }).ToList());
        }
    }
}
