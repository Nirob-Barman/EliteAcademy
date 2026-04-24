using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using System.Text.Json;

namespace EliteAcademy.Application.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigEncryptor _encryptor;
        private readonly IAuditLogService _auditLogService;

        public PaymentGatewayService(
            IUnitOfWork unitOfWork,
            IConfigEncryptor encryptor,
            IAuditLogService auditLogService)
        {
            _unitOfWork      = unitOfWork;
            _encryptor       = encryptor;
            _auditLogService = auditLogService;
        }

        public async Task<Result<List<PaymentGatewayDto>>> GetAllAsync()
        {
            var all = (await _unitOfWork.Repository<PaymentGateway>().GetAllAsync()).ToList();
            var txCounts = (await _unitOfWork.Repository<PaymentTransaction>().GetAllAsync())
                .GroupBy(t => t.GatewayId)
                .ToDictionary(g => g.Key, g => g.Count());

            var dtos = all.Select(g => new PaymentGatewayDto
            {
                Id               = g.Id,
                Slug             = g.Slug,
                Name             = g.Name,
                IsActive         = g.IsActive,
                IsSandbox        = g.IsSandbox,
                CreatedAt        = g.CreatedAt,
                TransactionCount = txCounts.GetValueOrDefault(g.Id)
            }).ToList();

            return Result<List<PaymentGatewayDto>>.Ok(dtos);
        }

        public async Task<Result<PaymentGatewayDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<PaymentGatewayDto>.Fail("Gateway not found.");

            return Result<PaymentGatewayDto>.Ok(new PaymentGatewayDto
            {
                Id        = entity.Id,
                Slug      = entity.Slug,
                Name      = entity.Name,
                IsActive  = entity.IsActive,
                IsSandbox = entity.IsSandbox,
                CreatedAt = entity.CreatedAt
            });
        }

        public async Task<Result<string>> GetDecryptedConfigAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<string>.Fail("Gateway not found.");

            try
            {
                var json = _encryptor.Decrypt(entity.Config);

                // Inject sandbox flag so processors always know the mode
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                           ?? new Dictionary<string, string>();
                dict["_is_sandbox"] = entity.IsSandbox.ToString().ToLower();
                return Result<string>.Ok(JsonSerializer.Serialize(dict));
            }
            catch
            {
                // Config may be empty (e.g. mock gateway) — return minimal JSON
                return Result<string>.Ok(JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["_is_sandbox"] = entity.IsSandbox.ToString().ToLower()
                }));
            }
        }

        public async Task<Result<bool>> CreateAsync(PaymentGatewayFormDto dto)
        {
            var slugLower = dto.Slug.Trim().ToLower();
            if (await _unitOfWork.Repository<PaymentGateway>().AnyAsync(g => g.Slug == slugLower))
                return Result<bool>.Fail("A gateway with this slug already exists.");

            var configJson = string.IsNullOrWhiteSpace(dto.Config) ? "{}" : dto.Config;
            var entity = new PaymentGateway
            {
                Slug      = slugLower,
                Name      = dto.Name.Trim(),
                Config    = _encryptor.Encrypt(configJson),
                IsActive  = dto.IsActive,
                IsSandbox = dto.IsSandbox,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PaymentGateway>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PaymentGateway", "Create",
                details: $"Created gateway \"{entity.Name}\" (slug: {entity.Slug})");

            return Result<bool>.Ok(true, "Payment gateway created.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, PaymentGatewayFormDto dto)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Gateway not found.");

            var slugLower = dto.Slug.Trim().ToLower();
            if (await _unitOfWork.Repository<PaymentGateway>().AnyAsync(g => g.Slug == slugLower && g.Id != id))
                return Result<bool>.Fail("Another gateway already uses this slug.");

            // Merge incoming config on top of existing decrypted config
            // (blank secrets are preserved from the stored config)
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

            entity.Slug      = slugLower;
            entity.Name      = dto.Name.Trim();
            entity.Config    = _encryptor.Encrypt(mergedJson);
            entity.IsActive  = dto.IsActive;
            entity.IsSandbox = dto.IsSandbox;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<PaymentGateway>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PaymentGateway", "Update",
                details: $"Updated gateway \"{entity.Name}\" (ID: {id})");

            return Result<bool>.Ok(true, "Payment gateway updated.");
        }

        public async Task<Result<bool>> ToggleActiveAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Gateway not found.");

            entity.IsActive  = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<PaymentGateway>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, entity.IsActive ? "Gateway activated." : "Gateway deactivated.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Gateway not found.");

            var hasTx = await _unitOfWork.Repository<PaymentTransaction>().AnyAsync(t => t.GatewayId == id);
            if (hasTx)
                return Result<bool>.Fail("Cannot delete a gateway that has transactions.");

            _unitOfWork.Repository<PaymentGateway>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PaymentGateway", "Delete",
                details: $"Deleted gateway \"{entity.Name}\"");

            return Result<bool>.Ok(true, "Payment gateway deleted.");
        }
    }
}
