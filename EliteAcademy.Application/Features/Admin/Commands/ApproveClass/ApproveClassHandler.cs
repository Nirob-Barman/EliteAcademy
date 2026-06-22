using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Commands.ApproveClass;

public class ApproveClassHandler : IRequestHandler<ApproveClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;

    public ApproveClassHandler(
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

    public async Task<Result<bool>> Handle(ApproveClassCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Class not found.");

        entity.Status = ClassStatus.Approved;
        entity.Feedback = null;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _userContextService.UserId;
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(entity.InstructorId))
        {
            await _notificationService.CreateAsync(
                entity.InstructorId,
                "Class Approved",
                $"Your class \"{entity.ClassName}\" has been approved and is now live.",
                "/Instructor/MyClasses");
        }

        await _auditLogService.LogAsync("Class", "Approve",
            details: $"Approved class \"{entity.ClassName}\" (ID: {request.ClassId})");

        if (!string.IsNullOrWhiteSpace(entity.InstructorId))
        {
            try
            {
                var instructor = await _userManager.FindByIdAsync(entity.InstructorId);
                if (!string.IsNullOrWhiteSpace(instructor?.Email))
                {
                    await _emailService.SendEmailAsync(
                        subject: $"Class Approved — {entity.ClassName}",
                        message: $"""
                            <div style="font-family:Arial,sans-serif;max-width:520px">
                              <h2 style="color:#198754">Your class has been approved!</h2>
                              <p>Hi <strong>{instructor.FirstName}</strong>,</p>
                              <p>Your class <strong>{entity.ClassName}</strong> has been approved and is now live for students to enroll.</p>
                              <p>Visit your <a href="/Instructor/MyClasses">My Classes</a> page to manage it.</p>
                              <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                            </div>
                            """,
                        toEmails: new List<string> { instructor.Email });
                }
            }
            catch { /* don't fail approval if email throws */ }
        }

        return Result<bool>.Ok(true, "Class approved.");
    }
}
