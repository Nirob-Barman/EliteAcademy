using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAdminDashboard;

public class GetAdminDashboardHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetAdminDashboardHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var allUsers = (await _userManager.GetAllUsersAsync()).ToList();
        var instructors = (await _userManager.GetUsersByRoleAsync("Instructor")).ToList();
        var students = (await _userManager.GetUsersByRoleAsync("Student")).ToList();
        var allClasses = await _context.Classes.AsNoTracking().ToListAsync(cancellationToken);
        var pendingApps = await _context.InstructorApplications.CountAsync(a => a.Status == InstructorApplicationStatus.Pending, cancellationToken);

        return Result<AdminDashboardDto>.Ok(new AdminDashboardDto
        {
            TotalUsers = allUsers.Count,
            TotalInstructors = instructors.Count,
            TotalStudents = students.Count,
            TotalClasses = allClasses.Count,
            PendingClasses = allClasses.Count(c => c.Status == ClassStatus.Pending),
            ApprovedClasses = allClasses.Count(c => c.Status == ClassStatus.Approved),
            RejectedClasses = allClasses.Count(c => c.Status == ClassStatus.Rejected),
            PendingInstructorApplications = pendingApps
        });
    }
}
