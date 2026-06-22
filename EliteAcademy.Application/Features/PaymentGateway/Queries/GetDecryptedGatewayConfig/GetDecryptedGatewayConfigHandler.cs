using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EliteAcademy.Application.Features.PaymentGateway.Queries.GetDecryptedGatewayConfig;

public class GetDecryptedGatewayConfigHandler : IRequestHandler<GetDecryptedGatewayConfigQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfigEncryptor _encryptor;

    public GetDecryptedGatewayConfigHandler(IApplicationDbContext context, IConfigEncryptor encryptor)
    {
        _context = context;
        _encryptor = encryptor;
    }

    public async Task<Result<string>> Handle(GetDecryptedGatewayConfigQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentGateways
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (entity == null)
            return Result<string>.Fail("Gateway not found.");

        try
        {
            var json = _encryptor.Decrypt(entity.Config);

            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            dict["_is_sandbox"] = entity.IsSandbox.ToString().ToLower();
            return Result<string>.Ok(JsonSerializer.Serialize(dict));
        }
        catch
        {
            return Result<string>.Ok(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["_is_sandbox"] = entity.IsSandbox.ToString().ToLower()
            }));
        }
    }
}
