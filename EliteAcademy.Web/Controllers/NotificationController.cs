using EliteAcademy.Application.Features.Notification.Commands.MarkAllNotificationsRead;
using EliteAcademy.Application.Features.Notification.Queries.GetMyNotifications;
using EliteAcademy.Application.Features.Notification.Queries.GetUnreadNotificationCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            await _mediator.Send(new MarkAllNotificationsReadCommand());
            var result = await _mediator.Send(new GetMyNotificationsQuery());
            return View(result.Data ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _mediator.Send(new GetUnreadNotificationCountQuery());
            return Json(new { count = result.Data });
        }
    }
}
