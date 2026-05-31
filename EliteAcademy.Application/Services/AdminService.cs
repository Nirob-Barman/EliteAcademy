using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.DTOs.Home;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IEmailService _emailService;

        public AdminService(
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

        public async Task<Result<AdminDashboardDto>> GetDashboardAsync()
        {
            var allUsers = (await _userManager.GetAllUsersAsync()).ToList();
            var instructors = (await _userManager.GetUsersByRoleAsync("Instructor")).ToList();
            var students = (await _userManager.GetUsersByRoleAsync("Student")).ToList();
            var allClasses = await _context.Classes.AsNoTracking().ToListAsync();
            var pendingApps = await _context.InstructorApplications.CountAsync(a => a.Status == InstructorApplicationStatus.Pending);

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

        public async Task<Result<List<AdminUserDto>>> GetAllUsersAsync()
        {
            var users = await _userManager.GetAllUsersAsync();
            var dtos = new List<AdminUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                dtos.Add(new AdminUserDto
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return Result<List<AdminUserDto>>.Ok(dtos);
        }

        public async Task<Result<bool>> ChangeUserRoleAsync(string userId, string newRole)
        {
            var validRoles = new[] { "Admin", "Instructor", "Student" };
            if (!validRoles.Contains(newRole))
                return Result<bool>.Fail("Invalid role.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var oldRole = currentRoles.FirstOrDefault() ?? "None";

            foreach (var role in currentRoles)
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, role);
                if (!removeResult.Succeeded)
                    return Result<bool>.Fail(removeResult.Errors.FirstOrDefault() ?? "Failed to remove existing role.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
                return Result<bool>.Fail(addResult.Errors.FirstOrDefault() ?? "Failed to assign new role.");

            await _auditLogService.LogAsync("User", "ChangeRole",
                details: $"Changed role for {user.Email} from {oldRole} to {newRole}");

            return Result<bool>.Ok(true, $"Role changed to {newRole}.");
        }

        public async Task<Result<List<ClassDto>>> GetAllClassesAsync()
        {
            var classes = await _context.Classes.AsNoTracking().ToListAsync();
            var users = await _userManager.GetAllUsersAsync();
            var instructorMap = users.ToDictionary(
                u => u.Id ?? "",
                u => $"{u.FirstName} {u.LastName}".Trim());

            var dtos = classes
                .Select(c => ClassMapper.ToDto(c, instructorMap.GetValueOrDefault(c.InstructorId ?? "")))
                .ToList();

            return Result<List<ClassDto>>.Ok(dtos);
        }

        public async Task<Result<bool>> ApproveClassAsync(int classId)
        {
            var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
            if (entity == null)
                return Result<bool>.Fail("Class not found.");

            entity.Status = ClassStatus.Approved;
            entity.Feedback = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _userContextService.UserId;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(entity.InstructorId))
            {
                await _notificationService.CreateAsync(
                    entity.InstructorId,
                    "Class Approved",
                    $"Your class \"{entity.ClassName}\" has been approved and is now live.",
                    "/Instructor/MyClasses");
            }

            await _auditLogService.LogAsync("Class", "Approve",
                details: $"Approved class \"{entity.ClassName}\" (ID: {classId})");

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

        public async Task<Result<bool>> RejectClassAsync(int classId, string feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback))
                return Result<bool>.Fail("Feedback is required when rejecting a class.");

            var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
            if (entity == null)
                return Result<bool>.Fail("Class not found.");

            entity.Status = ClassStatus.Rejected;
            entity.Feedback = feedback;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _userContextService.UserId;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(entity.InstructorId))
            {
                await _notificationService.CreateAsync(
                    entity.InstructorId,
                    "Class Rejected",
                    $"Your class \"{entity.ClassName}\" was not approved. Feedback: {feedback}",
                    "/Instructor/MyClasses");
            }

            await _auditLogService.LogAsync("Class", "Reject",
                details: $"Rejected class \"{entity.ClassName}\" (ID: {classId}). Feedback: {feedback}");

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
                                  <p><strong>Feedback:</strong> {feedback}</p>
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

        public async Task<Result<PlatformStatsDto>> GetPlatformStatsAsync()
        {
            var students = await _userManager.GetUsersByRoleAsync("Student");
            var instructors = await _userManager.GetUsersByRoleAsync("Instructor");
            var enrollments = await _context.Enrollments.CountAsync();
            var classes = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Approved);

            return Result<PlatformStatsDto>.Ok(new PlatformStatsDto
            {
                ActiveStudents = students.Count(),
                ExpertInstructors = instructors.Count(),
                TotalEnrollments = enrollments,
                ApprovedClasses = classes
            });
        }

        // ── Student Management ──────────────────────────────────────────────────

        public async Task<Result<List<AdminStudentDto>>> GetAllStudentsAsync()
        {
            var students = (await _userManager.GetUsersByRoleAsync("Student")).ToList();
            var enrollments = await _context.Enrollments.AsNoTracking().ToListAsync();

            var dtos = students.Select(s => new AdminStudentDto
            {
                Id = s.Id,
                FullName = $"{s.FirstName} {s.LastName}".Trim(),
                Email = s.Email,
                EnrollmentCount = enrollments.Count(e => e.StudentId == s.Id),
                IsBanned = s.IsBanned,
                JoinedAt = DateTime.UtcNow
            }).ToList();

            return Result<List<AdminStudentDto>>.Ok(dtos);
        }

        public async Task<Result<bool>> BanStudentAsync(string studentId)
        {
            var user = await _userManager.FindByIdAsync(studentId);
            if (user == null)
                return Result<bool>.Fail("Student not found.");

            var result = await _userManager.BanUserAsync(studentId);
            if (!result.Succeeded)
                return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Failed to ban student.");

            await _auditLogService.LogAsync("User", "Ban",
                details: $"Banned student {user.Email} (ID: {studentId})");

            return Result<bool>.Ok(true, $"{user.Email} has been banned.");
        }

        public async Task<Result<bool>> UnbanStudentAsync(string studentId)
        {
            var user = await _userManager.FindByIdAsync(studentId);
            if (user == null)
                return Result<bool>.Fail("Student not found.");

            var result = await _userManager.UnbanUserAsync(studentId);
            if (!result.Succeeded)
                return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Failed to unban student.");

            await _auditLogService.LogAsync("User", "Unban",
                details: $"Unbanned student {user.Email} (ID: {studentId})");

            return Result<bool>.Ok(true, $"{user.Email} has been unbanned.");
        }

        // ── Class Enrollments ───────────────────────────────────────────────────

        public async Task<Result<AdminClassEnrollmentsDto>> GetClassEnrollmentsAsync(int classId)
        {
            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId);
            if (cls == null)
                return Result<AdminClassEnrollmentsDto>.Fail("Class not found.");

            var allUsers = (await _userManager.GetAllUsersAsync()).ToDictionary(u => u.Id ?? "");
            var instructorName = allUsers.TryGetValue(cls.InstructorId ?? "", out var inst)
                ? $"{inst.FirstName} {inst.LastName}".Trim()
                : "Unknown";

            var enrollments = await _context.Enrollments.AsNoTracking().Where(e => e.ClassId == classId).ToListAsync();

            var rows = enrollments.Select(e =>
            {
                allUsers.TryGetValue(e.StudentId ?? "", out var student);
                return new StudentEnrollmentRowDto
                {
                    StudentId = e.StudentId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "Unknown",
                    Email = student?.Email,
                    EnrolledAt = e.EnrolledAt
                };
            }).ToList();

            return Result<AdminClassEnrollmentsDto>.Ok(new AdminClassEnrollmentsDto
            {
                ClassId = cls.Id,
                ClassName = cls.ClassName,
                InstructorName = instructorName,
                Price = cls.Price,
                AvailableSeats = cls.AvailableSeats,
                Enrollments = rows
            });
        }

        // ── Revenue Report ──────────────────────────────────────────────────────

        public async Task<Result<RevenueReportDto>> GetRevenueReportAsync(int year)
        {
            var transactions = await _context.PaymentTransactions.AsNoTracking().Where(
                t => t.Status == PaymentTransactionStatus.Success && t.CreatedAt.Year == year).ToListAsync();

            var preEnrollmentIds = transactions.Select(t => t.PreEnrollmentId).Distinct().ToList();
            var preEnrollments = await _context.PreEnrollments.AsNoTracking().Where(p => preEnrollmentIds.Contains(p.Id)).ToListAsync();

            var classIds = preEnrollments.Select(p => p.ClassId).Distinct().ToList();
            var classes = await _context.Classes.AsNoTracking().Where(c => classIds.Contains(c.Id)).ToListAsync();

            var allUsers = (await _userManager.GetAllUsersAsync()).ToDictionary(u => u.Id ?? "");
            var classMap = classes.ToDictionary(c => c.Id);
            var preEnrollMap = preEnrollments.ToDictionary(p => p.Id);

            var byMonth = Enumerable.Range(1, 12).Select(m =>
            {
                var monthTx = transactions.Where(t => t.CreatedAt.Month == m).ToList();
                return new MonthlyRevenueDto
                {
                    Month = m,
                    MonthName = new DateTime(year, m, 1).ToString("MMMM"),
                    Revenue = monthTx.Sum(t => t.Amount),
                    Transactions = monthTx.Count
                };
            }).ToList();

            var byClass = transactions
                .GroupBy(t =>
                {
                    preEnrollMap.TryGetValue(t.PreEnrollmentId, out var pe);
                    return pe?.ClassId ?? 0;
                })
                .Where(g => g.Key != 0)
                .Select(g =>
                {
                    classMap.TryGetValue(g.Key, out var cls);
                    return new ClassRevenueDto
                    {
                        ClassId = g.Key,
                        ClassName = cls?.ClassName ?? "Unknown",
                        Revenue = g.Sum(t => t.Amount),
                        Enrolled = g.Count()
                    };
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var byInstructor = transactions
                .GroupBy(t =>
                {
                    preEnrollMap.TryGetValue(t.PreEnrollmentId, out var pe);
                    if (pe == null) return null;
                    classMap.TryGetValue(pe.ClassId, out var cls);
                    return cls?.InstructorId;
                })
                .Where(g => g.Key != null)
                .Select(g =>
                {
                    allUsers.TryGetValue(g.Key!, out var instructor);
                    return new InstructorRevenueDto
                    {
                        InstructorId = g.Key,
                        InstructorName = instructor != null
                            ? $"{instructor.FirstName} {instructor.LastName}".Trim()
                            : "Unknown",
                        Revenue = g.Sum(t => t.Amount),
                        Enrolled = g.Count()
                    };
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            return Result<RevenueReportDto>.Ok(new RevenueReportDto
            {
                Year = year,
                TotalRevenue = transactions.Sum(t => t.Amount),
                TotalTransactions = transactions.Count,
                ByMonth = byMonth,
                ByClass = byClass,
                ByInstructor = byInstructor
            });
        }
    }
}
