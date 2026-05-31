using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class InstructorApplicationService : IInstructorApplicationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IUserManager _userManager;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public InstructorApplicationService(
            IApplicationDbContext context,
            IUserContextService userContextService,
            IUserManager userManager,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _userContextService = userContextService;
            _userManager = userManager;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<Result<InstructorApplicationDto>> ApplyAsync(InstructorApplicationFormDto dto)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<InstructorApplicationDto>.Fail("You must be logged in to apply.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<InstructorApplicationDto>.Fail("User not found.");

            if (await _userManager.IsUserInRoleAsync(user, "Instructor"))
                return Result<InstructorApplicationDto>.Fail("You are already an instructor.");

            var existing = await _context.InstructorApplications.AsNoTracking().FirstOrDefaultAsync(
                a => a.ApplicantId == userId
                  && (a.Status == InstructorApplicationStatus.Pending
                   || a.Status == InstructorApplicationStatus.Approved));

            if (existing != null)
            {
                var reason = existing.Status == InstructorApplicationStatus.Pending
                    ? "You already have a pending application. Please wait for admin review."
                    : "Your application has already been approved.";
                return Result<InstructorApplicationDto>.Fail(reason);
            }

            var entity = new InstructorApplication
            {
                ApplicantId = userId,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Bio = dto.Bio,
                Expertise = dto.Expertise,
                Motivation = dto.Motivation,
                Status = InstructorApplicationStatus.Pending,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.InstructorApplications.Add(entity);
            await _context.SaveChangesAsync();

            return Result<InstructorApplicationDto>.Ok(
                InstructorApplicationMapper.ToDto(entity),
                "Your application has been submitted. We will review it shortly.");
        }

        public async Task<Result<InstructorApplicationDto?>> GetMyApplicationAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<InstructorApplicationDto?>.Ok(null);

            var apps = await _context.InstructorApplications.AsNoTracking().Where(a => a.ApplicantId == userId).ToListAsync();
            var latest = apps.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            return Result<InstructorApplicationDto?>.Ok(
                latest != null ? InstructorApplicationMapper.ToDto(latest) : null);
        }

        public async Task<Result<List<InstructorApplicationDto>>> GetAllAsync()
        {
            var apps = (await _context.InstructorApplications.AsNoTracking().ToListAsync())
                .OrderByDescending(a => a.CreatedAt)
                .Select(InstructorApplicationMapper.ToDto)
                .ToList();

            return Result<List<InstructorApplicationDto>>.Ok(apps);
        }

        public async Task<Result<List<InstructorApplicationDto>>> GetPendingAsync()
        {
            var apps = (await _context.InstructorApplications.AsNoTracking().Where(a => a.Status == InstructorApplicationStatus.Pending).ToListAsync())
                .OrderBy(a => a.CreatedAt)
                .Select(InstructorApplicationMapper.ToDto)
                .ToList();

            return Result<List<InstructorApplicationDto>>.Ok(apps);
        }

        public async Task<Result<bool>> ApproveAsync(int applicationId)
        {
            var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == applicationId);
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

            app.Status = InstructorApplicationStatus.Approved;
            app.ReviewedAt = DateTime.UtcNow;
            app.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                app.ApplicantId!,
                "Instructor Application Approved",
                "Congratulations! Your instructor application has been approved. You can now create classes.",
                "/Instructor/Dashboard");

            try
            {
                if (!string.IsNullOrWhiteSpace(app.Email))
                {
                    await _emailService.SendEmailAsync(
                        subject: "Your Instructor Application — Approved!",
                        message: $"""
                            <div style="font-family:Arial,sans-serif;max-width:520px">
                              <h2 style="color:#198754">Application Approved!</h2>
                              <p>Hi <strong>{app.FullName}</strong>,</p>
                              <p>Great news — your application to become an instructor on <strong>Elite Academy</strong> has been approved.</p>
                              <p>You can now log in and start creating classes from your <strong>Instructor Dashboard</strong>.</p>
                              <p>Note: you may need to log out and log back in for the role change to take effect.</p>
                              <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                            </div>
                            """,
                        toEmails: new List<string> { app.Email });
                }
            }
            catch { /* don't fail approval if email throws */ }

            return Result<bool>.Ok(true, $"{app.FullName}'s application approved. They are now an Instructor.");
        }

        public async Task<Result<bool>> RejectAsync(int applicationId, string adminNotes)
        {
            if (string.IsNullOrWhiteSpace(adminNotes))
                return Result<bool>.Fail("A reason is required when rejecting an application.");

            var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == applicationId);
            if (app == null)
                return Result<bool>.Fail("Application not found.");

            if (app.Status != InstructorApplicationStatus.Pending)
                return Result<bool>.Fail("Only pending applications can be rejected.");

            app.Status = InstructorApplicationStatus.Rejected;
            app.AdminNotes = adminNotes;
            app.ReviewedAt = DateTime.UtcNow;
            app.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                app.ApplicantId!,
                "Instructor Application Update",
                $"Your instructor application was not approved. Reason: {adminNotes}",
                "/InstructorApplication/MyApplication");

            try
            {
                if (!string.IsNullOrWhiteSpace(app.Email))
                {
                    await _emailService.SendEmailAsync(
                        subject: "Your Instructor Application — Update",
                        message: $"""
                            <div style="font-family:Arial,sans-serif;max-width:520px">
                              <h2 style="color:#dc3545">Application Not Approved</h2>
                              <p>Hi <strong>{app.FullName}</strong>,</p>
                              <p>Thank you for applying to become an instructor on <strong>Elite Academy</strong>.</p>
                              <p>After review, we were unable to approve your application at this time.</p>
                              <p><strong>Reason:</strong> {adminNotes}</p>
                              <p>You are welcome to apply again after addressing the feedback above.</p>
                              <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                            </div>
                            """,
                        toEmails: new List<string> { app.Email });
                }
            }
            catch { /* don't fail rejection if email throws */ }

            return Result<bool>.Ok(true, "Application rejected.");
        }
    }
}
