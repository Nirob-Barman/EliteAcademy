using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.ApproveInstructorApplication;

public class ApproveInstructorApplicationHandler : IRequestHandler<ApproveInstructorApplicationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public ApproveInstructorApplicationHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(ApproveInstructorApplicationCommand request, CancellationToken cancellationToken)
    {
        var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (app == null)
            return Result<bool>.Fail("Application not found.");

        var approveResult = app.Approve();
        if (!approveResult.IsSuccess)
            return Result<bool>.Fail(approveResult.Error);

        var user = await _userManager.FindByIdAsync(app.ApplicantId!);
        if (user == null)
            return Result<bool>.Fail("Applicant account not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in currentRoles)
            await _userManager.RemoveFromRoleAsync(user, role);

        var addResult = await _userManager.AddToRoleAsync(user, "Instructor");
        if (!addResult.Succeeded)
            return Result<bool>.Fail(addResult.Errors.FirstOrDefault() ?? "Failed to assign Instructor role.");

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, $"{app.FullName}'s application approved. They are now an Instructor.");
    }
}
