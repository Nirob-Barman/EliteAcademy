using EliteAcademy.Application.Features.Admin.Queries.GetPlatformStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EliteAcademy.Web.Controllers
{
    [AllowAnonymous]
    public class AboutController : Controller
    {
        private readonly IMediator _mediator;

        public AboutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [OutputCache(Duration = 1800)]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetPlatformStatsQuery());
            return View(result.Data);
        }
    }
}
