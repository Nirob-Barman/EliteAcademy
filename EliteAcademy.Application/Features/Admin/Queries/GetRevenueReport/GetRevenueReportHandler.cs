using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetRevenueReport;

public class GetRevenueReportHandler : IRequestHandler<GetRevenueReportQuery, Result<RevenueReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetRevenueReportHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<RevenueReportDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _context.PaymentTransactions.AsNoTracking().Where(
            t => t.Status == PaymentTransactionStatus.Success && t.CreatedAt.Year == request.Year)
            .ToListAsync(cancellationToken);

        var preEnrollmentIds = transactions.Select(t => t.PreEnrollmentId).Distinct().ToList();
        var preEnrollments = await _context.PreEnrollments.AsNoTracking()
            .Where(p => preEnrollmentIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var classIds = preEnrollments.Select(p => p.ClassId).Distinct().ToList();
        var classes = await _context.Classes.AsNoTracking()
            .Where(c => classIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var allUsers = (await _userManager.GetAllUsersAsync()).ToDictionary(u => u.Id ?? "");
        var classMap = classes.ToDictionary(c => c.Id);
        var preEnrollMap = preEnrollments.ToDictionary(p => p.Id);

        var byMonth = Enumerable.Range(1, 12).Select(m =>
        {
            var monthTx = transactions.Where(t => t.CreatedAt.Month == m).ToList();
            return new MonthlyRevenueDto
            {
                Month = m,
                MonthName = new DateTime(request.Year, m, 1).ToString("MMMM"),
                Revenue = monthTx.Sum(t => t.Amount),
                Transactions = monthTx.Count
            };
        }).ToList();

        var byClass = transactions
            .GroupBy(t =>
            {
                preEnrollMap.TryGetValue(t.PreEnrollmentId, out var pe);
                return pe?.ClassId ?? 0;
            })
            .Where(g => g.Key != 0)
            .Select(g =>
            {
                classMap.TryGetValue(g.Key, out var cls);
                return new ClassRevenueDto
                {
                    ClassId = g.Key,
                    ClassName = cls?.ClassName ?? "Unknown",
                    Revenue = g.Sum(t => t.Amount),
                    Enrolled = g.Count()
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var byInstructor = transactions
            .GroupBy(t =>
            {
                preEnrollMap.TryGetValue(t.PreEnrollmentId, out var pe);
                if (pe == null) return null;
                classMap.TryGetValue(pe.ClassId, out var cls);
                return cls?.InstructorId;
            })
            .Where(g => g.Key != null)
            .Select(g =>
            {
                allUsers.TryGetValue(g.Key!, out var instructor);
                return new InstructorRevenueDto
                {
                    InstructorId = g.Key,
                    InstructorName = instructor != null
                        ? $"{instructor.FirstName} {instructor.LastName}".Trim()
                        : "Unknown",
                    Revenue = g.Sum(t => t.Amount),
                    Enrolled = g.Count()
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return Result<RevenueReportDto>.Ok(new RevenueReportDto
        {
            Year = request.Year,
            TotalRevenue = transactions.Sum(t => t.Amount),
            TotalTransactions = transactions.Count,
            ByMonth = byMonth,
            ByClass = byClass,
            ByInstructor = byInstructor
        });
    }
}
