using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EliteAcademy.Application.Features.Payment.Commands.InitiatePayment;

public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentProcessorFactory _processorFactory;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly IUserContextService _userContextService;

    public InitiatePaymentHandler(
        IApplicationDbContext context,
        IPaymentProcessorFactory processorFactory,
        IPaymentGatewayService gatewayService,
        IUserContextService userContextService)
    {
        _context = context;
        _processorFactory = processorFactory;
        _gatewayService = gatewayService;
        _userContextService = userContextService;
    }

    public async Task<Result<string>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var preEnrollment = await _context.PreEnrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PreEnrollmentId, cancellationToken);

        if (preEnrollment == null || preEnrollment.StudentId != studentId)
            return Result<string>.Fail("Selection not found.");
        if (preEnrollment.PaymentStatus != PaymentStatus.Pending)
            return Result<string>.Fail("This selection has already been paid.");

        var cls = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == preEnrollment.ClassId, cancellationToken);
        if (cls == null || cls.AvailableSeats <= 0)
            return Result<string>.Fail("No available seats remaining.");

        var gateway = await _context.PaymentGateways
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == request.GatewaySlug && g.IsActive, cancellationToken);
        if (gateway == null)
            return Result<string>.Fail("Payment gateway not available.");

        var processor = _processorFactory.GetProcessor(request.GatewaySlug);
        if (processor == null)
            return Result<string>.Fail("Payment processor not found.");

        var amount = cls.Price - preEnrollment.DiscountAmount;
        var successUrl = $"{request.BaseUrl}/Payment/Success?txId=0&gateway={request.GatewaySlug}";
        var cancelUrl = $"{request.BaseUrl}/Payment/Cancel?txId=0";

        await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            var tx = new PaymentTransaction
            {
                PreEnrollmentId = request.PreEnrollmentId,
                GatewayId = gateway.Id,
                Amount = amount,
                Status = PaymentTransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            };
            _context.PaymentTransactions.Add(tx);
            await _context.SaveChangesAsync(cancellationToken);

            successUrl = $"{request.BaseUrl}/Payment/Success?txId={tx.Id}&gateway={request.GatewaySlug}";
            cancelUrl = $"{request.BaseUrl}/Payment/Cancel?txId={tx.Id}";

            var configResult = await _gatewayService.GetDecryptedConfigAsync(gateway.Id);
            var config = configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data) ?? new()
                : new Dictionary<string, string>();

            var initiateResult = await processor.InitiateAsync(config, amount, tx.Id, successUrl, cancelUrl);
            if (!initiateResult.Success)
            {
                tx.Status = PaymentTransactionStatus.Failed;
                tx.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                await _context.CommitTransactionAsync(cancellationToken);
                return Result<string>.Fail(initiateResult.ErrorMessage ?? "Payment initiation failed.");
            }

            await _context.CommitTransactionAsync(cancellationToken);
            return Result<string>.Ok(initiateResult.RedirectUrl!, "Payment initiated.");
        }
        catch
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            return Result<string>.Fail("Payment initiation failed.");
        }
    }
}
