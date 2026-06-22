using EliteAcademy.Application.Features.Coupon.Commands.CreateCoupon;
using EliteAcademy.Application.Features.Coupon.Commands.DeleteCoupon;
using EliteAcademy.Application.Features.Coupon.Commands.ToggleCoupon;
using EliteAcademy.Application.Features.Coupon.Commands.UpdateCoupon;
using EliteAcademy.Application.Features.Coupon.Queries.GetAllCoupons;
using EliteAcademy.Application.Features.Coupon.Queries.GetCouponById;
using EliteAcademy.Web.ViewModels.Coupon;
using EliteAcademy.Web.ViewModels.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly IMediator _mediator;

        public CouponController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllCouponsQuery());
            return View(result.Data ?? new());
        }

        [HttpGet]
        public IActionResult Create() => View(new CouponFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mediator.Send(new CreateCouponCommand(CouponViewModelMapper.ToDto(vm)));
            if (!result.Success)
            {
                if (result.FieldErrors?.Any() == true)
                    foreach (var (field, msg) in result.FieldErrors)
                        ModelState.AddModelError(field, msg);
                else
                    ModelState.AddModelError("", result.Errors?.FirstOrDefault() ?? result.Message ?? "Failed to create coupon.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetCouponByIdQuery(id));
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Coupon not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(CouponViewModelMapper.ToVm(result.Data));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CouponFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mediator.Send(new UpdateCouponCommand(id, CouponViewModelMapper.ToDto(vm)));
            if (!result.Success)
            {
                if (result.FieldErrors?.Any() == true)
                    foreach (var (field, msg) in result.FieldErrors)
                        ModelState.AddModelError(field, msg);
                else
                    ModelState.AddModelError("", result.Errors?.FirstOrDefault() ?? result.Message ?? "Failed to update coupon.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteCouponCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _mediator.Send(new ToggleCouponCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
