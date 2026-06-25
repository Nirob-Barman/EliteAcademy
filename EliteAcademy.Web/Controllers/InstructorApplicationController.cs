using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Features.InstructorApplication.Commands.ApplyForInstructor;
using EliteAcademy.Application.Features.InstructorApplication.Queries.GetMyInstructorApplication;
using EliteAcademy.Web.ViewModels.InstructorApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class InstructorApplicationController : Controller
    {
        private readonly IMediator _mediator;

        public InstructorApplicationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /InstructorApplication/Apply
        public async Task<IActionResult> Apply()
        {
            // If the student already has an application, redirect to status page
            var existing = await _mediator.Send(new GetMyInstructorApplicationQuery());
            if (existing.Data != null)
                return RedirectToAction(nameof(MyApplication));

            return View(new InstructorApplicationFormViewModel());
        }

        // POST /InstructorApplication/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(InstructorApplicationFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _mediator.Send(new ApplyForInstructorCommand(new InstructorApplicationFormDto
            {
                Bio = vm.Bio,
                Expertise = vm.Expertise,
                Motivation = vm.Motivation
            }));

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Failed to submit application.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(MyApplication));
        }

        // GET /InstructorApplication/MyApplication
        public async Task<IActionResult> MyApplication()
        {
            var result = await _mediator.Send(new GetMyInstructorApplicationQuery());
            return View(result.Data);   // nullable — view handles null (no application yet)
        }
    }
}
