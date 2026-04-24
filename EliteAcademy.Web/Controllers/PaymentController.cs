using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentGatewayService _gatewayService;
        private readonly IStudentService _studentService;
        private readonly IUserContextService _userContextService;

        public PaymentController(
            IPaymentService paymentService,
            IPaymentGatewayService gatewayService,
            IStudentService studentService,
            IUserContextService userContextService)
        {
            _paymentService      = paymentService;
            _gatewayService      = gatewayService;
            _studentService      = studentService;
            _userContextService  = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var selectionsResult = await _studentService.GetSelectedClassesAsync();
            var preEnrollment = selectionsResult.Data?.FirstOrDefault(p => p.Id == id);
            if (preEnrollment == null)
            {
                TempData["Error"] = "Selection not found.";
                return RedirectToAction("Cart", "Student");
            }

            var gatewaysResult = await _gatewayService.GetAllAsync();
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
                var gatewaysResult = await _gatewayService.GetAllAsync();
                vm.Gateways = gatewaysResult.Data?.Where(g => g.IsActive).ToList()
                              ?? new List<PaymentGatewayDto>();
                return View("Checkout", vm);
            }

            var baseUrl = _userContextService.GetBaseUrl();
            var result  = await _paymentService.InitiateAsync(
                vm.PreEnrollmentId, vm.SelectedGatewaySlug!, baseUrl);

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

            var result = await _paymentService.HandleSuccessAsync(txId, gateway, callbackParams);

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
            await _paymentService.HandleCancelAsync(txId);
            TempData["Error"] = "Payment was cancelled.";
            return View();
        }
    }
}
