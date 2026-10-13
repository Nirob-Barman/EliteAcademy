using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.TogglePaymentGateway;

public class TogglePaymentGatewayHandler : IRequestHandler<TogglePaymentGatewayCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public TogglePaymentGatewayHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(TogglePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentGateways
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Gateway not found.");

        entity.ToggleStatus();
        entity.SetUpdatedBy(_userContextService.UserId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, entity.IsActive ? "Gateway activated." : "Gateway deactivated.");
    }
}
