using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.ViewModels.Mappers;
using EliteAcademy.Web.ViewModels.PaymentGateway;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentGatewayController : Controller
    {
        private readonly IPaymentGatewayService _gatewayService;

        public PaymentGatewayController(IPaymentGatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _gatewayService.GetAllAsync();
            return View(result.Data ?? new List<PaymentGatewayDto>());
        }

        [HttpGet]
        public IActionResult Create() => View(new PaymentGatewayFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _gatewayService.CreateAsync(PaymentGatewayViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Errors?.FirstOrDefault() ?? result.Message ?? "Failed.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _gatewayService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = "Gateway not found.";
                return RedirectToAction(nameof(Index));
            }

            var dto = result.Data!;
            var vm = new PaymentGatewayFormViewModel
            {
                Id = dto.Id,
                Slug = dto.Slug,
                Name = dto.Name,
                IsActive = dto.IsActive,
                IsSandbox = dto.IsSandbox
            };

            var configResult = await _gatewayService.GetDecryptedConfigAsync(dto.Id);
            if (configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data))
                PaymentGatewayViewModelMapper.PopulateFields(vm, configResult.Data);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _gatewayService.UpdateAsync(id, PaymentGatewayViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Errors?.FirstOrDefault() ?? result.Message ?? "Failed.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _gatewayService.ToggleActiveAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _gatewayService.DeleteAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
