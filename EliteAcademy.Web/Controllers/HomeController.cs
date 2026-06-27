using EliteAcademy.Application.Features.Admin.Queries.GetPlatformStats;
using EliteAcademy.Application.Features.Class.Queries.GetApprovedClasses;
using EliteAcademy.Application.Features.Coupon.Queries.GetAllCoupons;
using EliteAcademy.Application.Features.Instructor.Queries.GetPublicInstructorList;
using EliteAcademy.Web.Models;
using EliteAcademy.Web.ViewModels.Home;
using EliteAcademy.Web.ViewModels.Student;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
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

        [OutputCache(Duration = 300)]
        public async Task<IActionResult> Index()
        {
            var classesResult = await _mediator.Send(new GetApprovedClassesQuery());
            var statsResult = await _mediator.Send(new GetPlatformStatsQuery());
            var instructorsResult = await _mediator.Send(new GetPublicInstructorListQuery());
            var couponsResult = await _mediator.Send(new GetAllCouponsQuery());

            var classItems = (classesResult.Data ?? new()).Select(c => new ClassIndexItemViewModel
            {
                Class = c,
                AverageRating = c.AverageRating,
                ReviewCount = c.ReviewCount
            }).ToList();

            var activeCoupons = (couponsResult.Data ?? new())
                .Where(c => c.IsActive && !c.IsExpired && !c.IsFull)
                .ToList();

            return View(new HomeIndexViewModel
            {
                Classes = classItems,
                Stats = statsResult.Data ?? new(),
                FeaturedInstructors = (instructorsResult.Data ?? new()).Take(4).ToList(),
                ActiveCoupons = activeCoupons
            });
        }

        public new IActionResult NotFound() => View();

        [AllowAnonymous]
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
