using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.RejectInstructorApplication;

public class RejectInstructorApplicationHandler : IRequestHandler<RejectInstructorApplicationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RejectInstructorApplicationHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(RejectInstructorApplicationCommand request, CancellationToken cancellationToken)
    {
        var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (app == null)
            return Result<bool>.Fail("Application not found.");

        var rejectResult = app.Reject(request.AdminNotes);
        if (!rejectResult.IsSuccess)
            return Result<bool>.Fail(rejectResult.Error);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Application rejected.");
    }
}
