using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Payment.Commands.HandlePaymentCancel;

public class HandlePaymentCancelHandler : IRequestHandler<HandlePaymentCancelCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public HandlePaymentCancelHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(HandlePaymentCancelCommand request, CancellationToken cancellationToken)
    {
        var tx = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.Id == request.TxId, cancellationToken);
        if (tx == null) return Result<bool>.Fail("Transaction not found.");

        if (tx.Status == PaymentTransactionStatus.Pending)
        {
            tx.Status = PaymentTransactionStatus.Cancelled;
            tx.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Ok(true, "Payment cancelled.");
    }
}
