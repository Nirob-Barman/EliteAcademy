using EliteAcademy.Application.DTOs.AuditLog;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string entityName, string action, string? details = null,
            string? oldValues = null, string? newValues = null);

        Task<Result<List<AuditLogDto>>> GetAllAsync(
            string? entityFilter = null,
            string? actionFilter = null,
            int page = 1,
            int pageSize = 30);
    }
}
