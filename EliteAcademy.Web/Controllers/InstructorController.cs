using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Features.Instructor.Queries.GetInstructorDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly IMediator _mediator;

        public InstructorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Dashboard()
        {
            var result = await _mediator.Send(new GetInstructorDashboardQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new InstructorDashboardDto());
            }
            return View(result.Data!);
        }

    }
}
