using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.RejectInstructorApplication;

public class RejectInstructorApplicationHandler : IRequestHandler<RejectInstructorApplicationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RejectInstructorApplicationHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(RejectInstructorApplicationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AdminNotes))
            return Result<bool>.Fail("A reason is required when rejecting an application.");

        var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (app == null)
            return Result<bool>.Fail("Application not found.");

        if (app.Status != InstructorApplicationStatus.Pending)
            return Result<bool>.Fail("Only pending applications can be rejected.");

        app.Status = InstructorApplicationStatus.Rejected;
        app.AdminNotes = request.AdminNotes;
        app.ReviewedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;

        app.AddDomainEvent(new InstructorApplicationRejectedEvent(app.ApplicantId!, app.FullName!, app.Email!, request.AdminNotes));

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Application rejected.");
    }
}
