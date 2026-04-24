using EliteAcademy.Application.DTOs.AuditLog;
using EliteAcademy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index(string? entity = null, string? action = null, int page = 1)
        {
            var result = await _auditLogService.GetAllAsync(entity, action, page, 30);
            ViewBag.EntityFilter = entity;
            ViewBag.ActionFilter = action;
            ViewBag.CurrentPage = page;
            return View(result.Data ?? new List<AuditLogDto>());
        }
    }
}
