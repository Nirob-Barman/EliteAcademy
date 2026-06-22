using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Features.Payment.Commands.HandlePaymentCancel;
using EliteAcademy.Application.Features.Payment.Commands.HandlePaymentSuccess;
using EliteAcademy.Application.Features.Payment.Commands.InitiatePayment;
using EliteAcademy.Application.Features.PaymentGateway.Queries.GetAllPaymentGateways;
using EliteAcademy.Application.Features.Student.Queries.GetSelectedClasses;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Web.ViewModels.Student;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class PaymentController : Controller
    {
        private readonly IUserContextService _userContextService;
        private readonly IMediator _mediator;

        public PaymentController(
            IUserContextService userContextService,
            IMediator mediator)
        {
            _userContextService = userContextService;
            _mediator           = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var selectionsResult = await _mediator.Send(new GetSelectedClassesQuery());
            var preEnrollment = selectionsResult.Data?.FirstOrDefault(p => p.Id == id);
            if (preEnrollment == null)
            {
                TempData["Error"] = "Selection not found.";
                return RedirectToAction("Cart", "Student");
            }

            var gatewaysResult = await _mediator.Send(new GetAllPaymentGatewaysQuery());
            var activeGateways = gatewaysResult.Data?
                .Where(g => g.IsActive)
                .ToList() ?? new List<PaymentGatewayDto>();

            if (!activeGateways.Any())
            {
                TempData["Error"] = "No payment gateways are currently available. Please contact support.";
                return RedirectToAction("Cart", "Student");
            }

            var vm = new CheckoutViewModel
            {
                PreEnrollmentId = preEnrollment.Id,
                ClassName       = preEnrollment.ClassName ?? string.Empty,
                Price           = preEnrollment.Price,
                DiscountAmount  = preEnrollment.DiscountAmount,
                CouponCode      = preEnrollment.CouponCode,
                Gateways        = activeGateways
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(CheckoutViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Reload gateways for re-render
                var gatewaysResult = await _mediator.Send(new GetAllPaymentGatewaysQuery());
                vm.Gateways = gatewaysResult.Data?.Where(g => g.IsActive).ToList()
                              ?? new List<PaymentGatewayDto>();
                return View("Checkout", vm);
            }

            var baseUrl = _userContextService.GetBaseUrl();
            var result  = await _mediator.Send(new InitiatePaymentCommand(
                vm.PreEnrollmentId, vm.SelectedGatewaySlug!, baseUrl));

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Checkout), new { id = vm.PreEnrollmentId });
            }

            return Redirect(result.Data!);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Success(int txId, string gateway)
        {
            var callbackParams = Request.Query
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            var result = await _mediator.Send(new HandlePaymentSuccessCommand(txId, gateway, callbackParams));

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View("Cancel");
            }

            TempData["Success"] = result.Message;
            return View("Success");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Cancel(int txId)
        {
            await _mediator.Send(new HandlePaymentCancelCommand(txId));
            TempData["Error"] = "Payment was cancelled.";
            return View();
        }
    }
}
