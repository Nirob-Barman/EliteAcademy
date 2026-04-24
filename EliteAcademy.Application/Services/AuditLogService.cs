using EliteAcademy.Application.DTOs.AuditLog;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public AuditLogService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task LogAsync(string entityName, string action, string? details = null,
            string? oldValues = null, string? newValues = null)
        {
            await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
            {
                EntityName = entityName,
                Action     = action,
                UserId     = _userContextService.UserId,
                UserName   = _userContextService.Email,
                Details    = details,
                OldValues  = oldValues,
                NewValues  = newValues,
                CreatedAt  = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Result<List<AuditLogDto>>> GetAllAsync(
            string? entityFilter = null, string? actionFilter = null,
            int page = 1, int pageSize = 30)
        {
            var all = (await _unitOfWork.Repository<AuditLog>().GetAllAsync()).ToList();

            if (!string.IsNullOrWhiteSpace(entityFilter))
                all = all.Where(a => a.EntityName.Contains(entityFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(actionFilter))
                all = all.Where(a => a.Action.Contains(actionFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            var dtos = all
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id         = a.Id,
                    EntityName = a.EntityName,
                    Action     = a.Action,
                    UserId     = a.UserId,
                    UserName   = a.UserName,
                    Details    = a.Details,
                    OldValues  = a.OldValues,
                    NewValues  = a.NewValues,
                    CreatedAt  = a.CreatedAt
                }).ToList();

            return Result<List<AuditLogDto>>.Ok(dtos);
        }
    }
}
