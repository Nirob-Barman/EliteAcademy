using EliteAcademy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            await _notificationService.MarkAllReadAsync();
            var result = await _notificationService.GetMyAsync();
            return View(result.Data ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _notificationService.GetUnreadCountAsync();
            return Json(new { count = result.Data });
        }
    }
}
