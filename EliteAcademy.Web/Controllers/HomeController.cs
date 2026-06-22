using EliteAcademy.Application.Features.Admin.Queries.GetPlatformStats;
using EliteAcademy.Application.Features.Class.Queries.GetApprovedClasses;
using EliteAcademy.Application.Features.Coupon.Queries.GetAllCoupons;
using EliteAcademy.Application.Features.Instructor.Queries.GetPublicInstructorList;
using EliteAcademy.Application.Features.Review.Queries.GetReviewSummary;
using EliteAcademy.Web.Models;
using EliteAcademy.Web.ViewModels.Home;
using EliteAcademy.Web.ViewModels.Student;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EliteAcademy.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var classesResult     = await _mediator.Send(new GetApprovedClassesQuery());
            var statsResult       = await _mediator.Send(new GetPlatformStatsQuery());
            var instructorsResult = await _mediator.Send(new GetPublicInstructorListQuery());
            var summaryResult     = await _mediator.Send(new GetReviewSummaryQuery());
            var couponsResult     = await _mediator.Send(new GetAllCouponsQuery());

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
