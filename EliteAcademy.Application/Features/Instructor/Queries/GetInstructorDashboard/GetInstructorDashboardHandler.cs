using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetInstructorDashboard;

public class GetInstructorDashboardHandler : IRequestHandler<GetInstructorDashboardQuery, Result<InstructorDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetInstructorDashboardHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<InstructorDashboardDto>> Handle(GetInstructorDashboardQuery request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;
        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c => c.InstructorId == instructorId)
            .ToListAsync(cancellationToken);

        var classIds = classes.Select(c => c.Id).ToHashSet();

        var enrollments = classIds.Any()
            ? await _context.Enrollments.AsNoTracking().Where(e => classIds.Contains(e.ClassId)).ToListAsync(cancellationToken)
            : new List<Enrollment>();

        var paidPreEnrollments = classIds.Any()
            ? await _context.PreEnrollments.AsNoTracking().Where(p => classIds.Contains(p.ClassId) && p.PaymentStatus == PaymentStatus.Paid).ToListAsync(cancellationToken)
            : new List<PreEnrollment>();

        var totalRevenue = paidPreEnrollments
            .Sum(p =>
            {
                var cls = classes.FirstOrDefault(c => c.Id == p.ClassId);
                return (cls?.Price ?? 0) - p.DiscountAmount;
            });

        var allMonths = new List<MonthlyRevenueItem>();
        for (int i = 11; i >= 0; i--)
        {
            var d = DateTime.UtcNow.AddMonths(-i);
            var monthEnrollments = enrollments
                .Where(e => e.EnrolledAt.Year == d.Year && e.EnrolledAt.Month == d.Month)
                .ToList();

            var monthRevenue = monthEnrollments.Sum(e =>
            {
                var cls = classes.FirstOrDefault(c => c.Id == e.ClassId);
                var pe = paidPreEnrollments.FirstOrDefault(p => p.ClassId == e.ClassId && p.StudentId == e.StudentId);
                return (cls?.Price ?? 0) - (pe?.DiscountAmount ?? 0);
            });

            allMonths.Add(new MonthlyRevenueItem
            {
                Year = d.Year,
                Month = d.Month,
                Enrollments = monthEnrollments.Count,
                Revenue = monthRevenue
            });
        }

        return Result<InstructorDashboardDto>.Ok(new InstructorDashboardDto
        {
            TotalClasses = classes.Count,
            PendingClasses = classes.Count(c => c.Status == ClassStatus.Pending),
            ApprovedClasses = classes.Count(c => c.Status == ClassStatus.Approved),
            RejectedClasses = classes.Count(c => c.Status == ClassStatus.Rejected),
            TotalStudents = enrollments.Select(e => e.StudentId).Distinct().Count(),
            TotalRevenue = totalRevenue,
            MonthlyRevenue = allMonths
        });
    }
}
