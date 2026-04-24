using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.ViewModels.Instructor;
using EliteAcademy.Web.ViewModels.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly IInstructorService _instructorService;

        public InstructorController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var result = await _instructorService.GetDashboardAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new InstructorDashboardDto());
            }
            return View(result.Data!);
        }

    }
}
