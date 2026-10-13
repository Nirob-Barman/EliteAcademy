using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentGatewayEntity = EliteAcademy.Domain.Entities.PaymentGateway;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.CreatePaymentGateway;

public class CreatePaymentGatewayHandler : IRequestHandler<CreatePaymentGatewayCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfigEncryptor _encryptor;
    private readonly IAuditLogService _auditLogService;

    public CreatePaymentGatewayHandler(
        IApplicationDbContext context,
        IConfigEncryptor encryptor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _encryptor = encryptor;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(CreatePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var slugLower = dto.Slug.Trim().ToLower();

        if (await _context.PaymentGateways.AnyAsync(g => g.Slug == slugLower, cancellationToken))
            return Result<bool>.Fail("A gateway with this slug already exists.");

        var configJson = string.IsNullOrWhiteSpace(dto.Config) ? "{}" : dto.Config;

        var encryptedConfig = _encryptor.Encrypt(configJson);
        var entity = PaymentGatewayEntity.Create(
            slug: slugLower,
            name: dto.Name,
            config: encryptedConfig,
            isActive: dto.IsActive,
            isSandbox: dto.IsSandbox);

        _context.PaymentGateways.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("PaymentGateway", "Create",
            details: $"Created gateway \"{entity.Name}\" (slug: {entity.Slug})");

        return Result<bool>.Ok(true, "Payment gateway created.");
    }
}
