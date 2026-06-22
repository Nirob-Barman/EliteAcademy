using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Queries.GetStudentDashboard;

public class GetStudentDashboardHandler : IRequestHandler<GetStudentDashboardQuery, Result<StudentDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetStudentDashboardHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<StudentDashboardDto>> Handle(GetStudentDashboardQuery request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;

        var selectedCount = await _context.PreEnrollments.CountAsync(p => p.StudentId == studentId && p.PaymentStatus == PaymentStatus.Pending, cancellationToken);
        var enrolledCount = await _context.Enrollments.CountAsync(e => e.StudentId == studentId, cancellationToken);
        var availableCount = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Approved, cancellationToken);
        var wishlistCount = await _context.Wishlists.CountAsync(w => w.StudentId == studentId, cancellationToken);

        return Result<StudentDashboardDto>.Ok(new StudentDashboardDto
        {
            SelectedCount = selectedCount,
            EnrolledCount = enrolledCount,
            TotalAvailableClasses = availableCount,
            WishlistCount = wishlistCount
        });
    }
}
