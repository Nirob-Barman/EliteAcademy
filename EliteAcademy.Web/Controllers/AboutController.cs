using EliteAcademy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [AllowAnonymous]
    public class AboutController : Controller
    {
        private readonly IAdminService _adminService;

        public AboutController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _adminService.GetPlatformStatsAsync();
            return View(result.Data);
        }
    }
}
