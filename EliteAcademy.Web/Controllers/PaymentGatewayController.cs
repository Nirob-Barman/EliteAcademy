using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Features.PaymentGateway.Commands.CreatePaymentGateway;
using EliteAcademy.Application.Features.PaymentGateway.Commands.DeletePaymentGateway;
using EliteAcademy.Application.Features.PaymentGateway.Commands.TogglePaymentGateway;
using EliteAcademy.Application.Features.PaymentGateway.Commands.UpdatePaymentGateway;
using EliteAcademy.Application.Features.PaymentGateway.Queries.GetAllPaymentGateways;
using EliteAcademy.Application.Features.PaymentGateway.Queries.GetDecryptedGatewayConfig;
using EliteAcademy.Application.Features.PaymentGateway.Queries.GetPaymentGatewayById;
using EliteAcademy.Web.ViewModels.Mappers;
using EliteAcademy.Web.ViewModels.PaymentGateway;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentGatewayController : Controller
    {
        private readonly IMediator _mediator;

        public PaymentGatewayController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllPaymentGatewaysQuery());
            return View(result.Data ?? new List<PaymentGatewayDto>());
        }

        [HttpGet]
        public IActionResult Create() => View(new PaymentGatewayFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mediator.Send(new CreatePaymentGatewayCommand(PaymentGatewayViewModelMapper.ToDto(vm)));
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
            var result = await _mediator.Send(new GetPaymentGatewayByIdQuery(id));
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

            var configResult = await _mediator.Send(new GetDecryptedGatewayConfigQuery(dto.Id));
            if (configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data))
                PaymentGatewayViewModelMapper.PopulateFields(vm, configResult.Data);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mediator.Send(new UpdatePaymentGatewayCommand(id, PaymentGatewayViewModelMapper.ToDto(vm)));
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
            var result = await _mediator.Send(new TogglePaymentGatewayCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeletePaymentGatewayCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
