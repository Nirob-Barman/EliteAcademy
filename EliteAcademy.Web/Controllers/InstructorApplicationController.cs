using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.ViewModels.InstructorApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class InstructorApplicationController : Controller
    {
        private readonly IInstructorApplicationService _service;

        public InstructorApplicationController(IInstructorApplicationService service)
        {
            _service = service;
        }

        // GET /InstructorApplication/Apply
        public async Task<IActionResult> Apply()
        {
            // If the student already has an application, redirect to status page
            var existing = await _service.GetMyApplicationAsync();
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

            var result = await _service.ApplyAsync(new InstructorApplicationFormDto
            {
                Bio        = vm.Bio,
                Expertise  = vm.Expertise,
                Motivation = vm.Motivation
            });

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
            var result = await _service.GetMyApplicationAsync();
            return View(result.Data);   // nullable — view handles null (no application yet)
        }
    }
}
