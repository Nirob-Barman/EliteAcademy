using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Commands.RejectClass;

public class RejectClassHandler : IRequestHandler<RejectClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;

    public RejectClassHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _userContextService = userContextService;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(RejectClassCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Feedback))
            return Result<bool>.Fail("Feedback is required when rejecting a class.");

        var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Class not found.");

        entity.Status = ClassStatus.Rejected;
        entity.Feedback = request.Feedback;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _userContextService.UserId;
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.InstructorId))
        {
            await _notificationService.CreateAsync(
                entity.InstructorId,
                "Class Rejected",
                $"Your class \"{entity.ClassName}\" was not approved. Feedback: {request.Feedback}",
                "/Instructor/MyClasses");
        }

        await _auditLogService.LogAsync("Class", "Reject",
            details: $"Rejected class \"{entity.ClassName}\" (ID: {request.ClassId}). Feedback: {request.Feedback}");

        if (!string.IsNullOrWhiteSpace(entity.InstructorId))
        {
            try
            {
                var instructor = await _userManager.FindByIdAsync(entity.InstructorId);
                if (!string.IsNullOrWhiteSpace(instructor?.Email))
                {
                    await _emailService.SendEmailAsync(
                        subject: $"Class Not Approved — {entity.ClassName}",
                        message: $"""
                            <div style="font-family:Arial,sans-serif;max-width:520px">
                              <h2 style="color:#dc3545">Class Not Approved</h2>
                              <p>Hi <strong>{instructor.FirstName}</strong>,</p>
                              <p>Your class <strong>{entity.ClassName}</strong> was not approved.</p>
                              <p><strong>Feedback:</strong> {request.Feedback}</p>
                              <p>Please review your class and resubmit after making the necessary changes.</p>
                              <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                            </div>
                            """,
                        toEmails: new List<string> { instructor.Email });
                }
            }
            catch { /* don't fail rejection if email throws */ }
        }

        return Result<bool>.Ok(true, "Class rejected.");
    }
}
