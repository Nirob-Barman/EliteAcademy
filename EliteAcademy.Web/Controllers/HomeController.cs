using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.Models;
using EliteAcademy.Web.ViewModels.Home;
using EliteAcademy.Web.ViewModels.Student;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EliteAcademy.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInstructorService _instructorService;
        private readonly IClassService _classService;
        private readonly IAdminService _adminService;
        private readonly IReviewService _reviewService;
        private readonly ICouponService _couponService;

        public HomeController(
            IInstructorService instructorService,
            IClassService classService,
            IAdminService adminService,
            IReviewService reviewService,
            ICouponService couponService)
        {
            _instructorService = instructorService;
            _classService      = classService;
            _adminService      = adminService;
            _reviewService     = reviewService;
            _couponService     = couponService;
        }

        public async Task<IActionResult> Index()
        {
            var classesResult     = await _classService.GetApprovedAsync();
            var statsResult       = await _adminService.GetPlatformStatsAsync();
            var instructorsResult = await _instructorService.GetPublicInstructorListAsync();
            var summaryResult     = await _reviewService.GetReviewSummaryAsync();
            var couponsResult     = await _couponService.GetAllAsync();

            var classes = classesResult.Data ?? new();
            var summary = summaryResult.Data ?? new();

            var classItems = classes.Select(c => new ClassIndexItemViewModel
            {
                Class         = c,
                AverageRating = summary.TryGetValue(c.Id, out var r)  ? r.Avg   : 0,
                ReviewCount   = summary.TryGetValue(c.Id, out var r2) ? r2.Count : 0
            }).ToList();

            var activeCoupons = (couponsResult.Data ?? new())
                .Where(c => c.IsActive && !c.IsExpired && !c.IsFull)
                .ToList();

            return View(new HomeIndexViewModel
            {
                Classes             = classItems,
                Stats               = statsResult.Data ?? new(),
                FeaturedInstructors = (instructorsResult.Data ?? new()).Take(4).ToList(),
                ActiveCoupons       = activeCoupons
            });
        }

        public new IActionResult NotFound() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
