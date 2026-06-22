using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.UpdatePaymentGateway;

public class UpdatePaymentGatewayHandler : IRequestHandler<UpdatePaymentGatewayCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfigEncryptor _encryptor;
    private readonly IAuditLogService _auditLogService;

    public UpdatePaymentGatewayHandler(
        IApplicationDbContext context,
        IConfigEncryptor encryptor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _encryptor = encryptor;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(UpdatePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var entity = await _context.PaymentGateways
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Gateway not found.");

        var slugLower = dto.Slug.Trim().ToLower();
        if (await _context.PaymentGateways.AnyAsync(g => g.Slug == slugLower && g.Id != request.Id, cancellationToken))
            return Result<bool>.Fail("Another gateway already uses this slug.");

        string mergedJson;
        try
        {
            var existingDecrypted = _encryptor.Decrypt(entity.Config);
            var existingDict = JsonSerializer.Deserialize<Dictionary<string, string>>(existingDecrypted)
                               ?? new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(dto.Config) && dto.Config != "{}")
            {
                var incomingDict = JsonSerializer.Deserialize<Dictionary<string, string>>(dto.Config)
                                   ?? new Dictionary<string, string>();
                foreach (var kv in incomingDict.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)))
                    existingDict[kv.Key] = kv.Value;
            }

            mergedJson = JsonSerializer.Serialize(existingDict);
        }
        catch
        {
            mergedJson = string.IsNullOrWhiteSpace(dto.Config) ? "{}" : dto.Config;
        }

        entity.Slug = slugLower;
        entity.Name = dto.Name.Trim();
        entity.Config = _encryptor.Encrypt(mergedJson);
        entity.IsActive = dto.IsActive;
        entity.IsSandbox = dto.IsSandbox;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("PaymentGateway", "Update",
            details: $"Updated gateway \"{entity.Name}\" (ID: {request.Id})");

        return Result<bool>.Ok(true, "Payment gateway updated.");
    }
}
