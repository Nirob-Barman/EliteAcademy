using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [AllowAnonymous]
    public class InstructorsController : Controller
    {
        private readonly IInstructorService _instructorService;

        public InstructorsController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _instructorService.GetPublicInstructorListAsync();
            return View(result.Data ?? new List<InstructorProfileDto>());
        }
    }
}
