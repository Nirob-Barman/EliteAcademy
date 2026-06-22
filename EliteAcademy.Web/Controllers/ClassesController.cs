using EliteAcademy.Application.Features.Class.Queries.GetApprovedClasses;
using EliteAcademy.Application.Features.Review.Queries.GetReviewSummary;
using EliteAcademy.Application.Features.Student.Queries.GetEnrollmentStatus;
using EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlistedClassIds;
using EliteAcademy.Web.ViewModels.Student;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [AllowAnonymous]
    public class ClassesController : Controller
    {
        private readonly IMediator _mediator;

        public ClassesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var classResult   = await _mediator.Send(new GetApprovedClassesQuery());
            var classes       = classResult.Data ?? new List<Application.DTOs.Class.ClassDto>();
            var summaryResult = await _mediator.Send(new GetReviewSummaryQuery());
            var summary       = summaryResult.Data ?? new();

            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
            {
                var statusResult    = await _mediator.Send(new GetEnrollmentStatusQuery());
                var selected        = statusResult.Success ? statusResult.Data.Selected : new HashSet<int>();
                var enrolled        = statusResult.Success ? statusResult.Data.Enrolled : new HashSet<int>();
                var wishlistResult  = await _mediator.Send(new GetMyWishlistedClassIdsQuery());
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
