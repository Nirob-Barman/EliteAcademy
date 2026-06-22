using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Features.Instructor.Queries.GetPublicInstructorList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [AllowAnonymous]
    public class InstructorsController : Controller
    {
        private readonly IMediator _mediator;

        public InstructorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetPublicInstructorListQuery());
            return View(result.Data ?? new List<InstructorProfileDto>());
        }
    }
}
