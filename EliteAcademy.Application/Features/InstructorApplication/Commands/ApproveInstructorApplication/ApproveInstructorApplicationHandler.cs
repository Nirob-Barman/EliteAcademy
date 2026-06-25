using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.ApproveInstructorApplication;

public class ApproveInstructorApplicationHandler : IRequestHandler<ApproveInstructorApplicationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public ApproveInstructorApplicationHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context     = context;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(ApproveInstructorApplicationCommand request, CancellationToken cancellationToken)
    {
        var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (app == null)
            return Result<bool>.Fail("Application not found.");

        if (app.Status != InstructorApplicationStatus.Pending)
            return Result<bool>.Fail("Only pending applications can be approved.");

        var user = await _userManager.FindByIdAsync(app.ApplicantId!);
        if (user == null)
            return Result<bool>.Fail("Applicant account not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in currentRoles)
            await _userManager.RemoveFromRoleAsync(user, role);

        var addResult = await _userManager.AddToRoleAsync(user, "Instructor");
        if (!addResult.Succeeded)
            return Result<bool>.Fail(addResult.Errors.FirstOrDefault() ?? "Failed to assign Instructor role.");

        app.Status     = InstructorApplicationStatus.Approved;
        app.ReviewedAt = DateTime.UtcNow;
        app.UpdatedAt  = DateTime.UtcNow;

        app.AddDomainEvent(new InstructorApplicationApprovedEvent(app.ApplicantId!, app.FullName!, app.Email!));

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, $"{app.FullName}'s application approved. They are now an Instructor.");
    }
}
