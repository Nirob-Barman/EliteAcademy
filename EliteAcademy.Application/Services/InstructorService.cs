using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly IApplicationDbContext _context;
        private readonly IAsyncQueryExecutor _executor;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;
        private readonly IFileStorage _fileStorage;

        public InstructorService(
            IApplicationDbContext context,
            IAsyncQueryExecutor executor,
            IUserManager userManager,
            IUserContextService userContextService,
            IFileStorage fileStorage)
        {
            _context            = context;
            _executor           = executor;
            _userManager        = userManager;
            _userContextService = userContextService;
            _fileStorage        = fileStorage;
        }

        public async Task<Result<InstructorProfileDto>> GetProfileAsync()
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null)
                return Result<InstructorProfileDto>.Fail("User not found.");

            return Result<InstructorProfileDto>.Ok(InstructorMapper.ToProfileDto(user));
        }

        public async Task<Result<bool>> UpdateProfileAsync(
            InstructorProfileDto dto, Stream? imageStream, string? imageFileName)
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            user.FirstName = dto.FirstName;
            user.LastName  = dto.LastName;

            if (imageStream != null && !string.IsNullOrWhiteSpace(imageFileName))
            {
                if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                    await _fileStorage.DeleteFileAsync(user.ImageUrl);

                user.ImageUrl = await _fileStorage.UploadFileAsync(imageStream, imageFileName, "uploads/profiles");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Update failed.");

            return Result<bool>.Ok(true, "Profile updated.");
        }

        public async Task<Result<InstructorDashboardDto>> GetDashboardAsync()
        {
            var instructorId = _userContextService.UserId!;
            var classes = await _executor.ToListAsync(_context.Classes.Where(c => c.InstructorId == instructorId));

            var classIds = classes.Select(c => c.Id).ToHashSet();

            var enrollments = classIds.Any()
                ? await _executor.ToListAsync(_context.Enrollments.Where(e => classIds.Contains(e.ClassId)))
                : new List<Enrollment>();

            var paidPreEnrollments = classIds.Any()
                ? await _executor.ToListAsync(_context.PreEnrollments.Where(p => classIds.Contains(p.ClassId) && p.PaymentStatus == PaymentStatus.Paid))
                : new List<PreEnrollment>();

            var totalRevenue = paidPreEnrollments
                .Sum(p =>
                {
                    var cls = classes.FirstOrDefault(c => c.Id == p.ClassId);
                    return (cls?.Price ?? 0) - p.DiscountAmount;
                });

            var allMonths = new List<MonthlyRevenueItem>();
            for (int i = 11; i >= 0; i--)
            {
                var d = DateTime.UtcNow.AddMonths(-i);
                var monthEnrollments = enrollments
                    .Where(e => e.EnrolledAt.Year == d.Year && e.EnrolledAt.Month == d.Month)
                    .ToList();

                var monthRevenue = monthEnrollments.Sum(e =>
                {
                    var cls = classes.FirstOrDefault(c => c.Id == e.ClassId);
                    var pe  = paidPreEnrollments.FirstOrDefault(p => p.ClassId == e.ClassId && p.StudentId == e.StudentId);
                    return (cls?.Price ?? 0) - (pe?.DiscountAmount ?? 0);
                });

                allMonths.Add(new MonthlyRevenueItem
                {
                    Year        = d.Year,
                    Month       = d.Month,
                    Enrollments = monthEnrollments.Count,
                    Revenue     = monthRevenue
                });
            }

            return Result<InstructorDashboardDto>.Ok(new InstructorDashboardDto
            {
                TotalClasses    = classes.Count,
                PendingClasses  = classes.Count(c => c.Status == ClassStatus.Pending),
                ApprovedClasses = classes.Count(c => c.Status == ClassStatus.Approved),
                RejectedClasses = classes.Count(c => c.Status == ClassStatus.Rejected),
                TotalStudents   = enrollments.Select(e => e.StudentId).Distinct().Count(),
                TotalRevenue    = totalRevenue,
                MonthlyRevenue  = allMonths
            });
        }

        public async Task<Result<List<ClassStudentDto>>> GetClassStudentsAsync(int classId)
        {
            var instructorId = _userContextService.UserId!;
            var cls = await _executor.FirstOrDefaultAsync(_context.Classes.Where(c => c.Id == classId));
            if (cls == null || cls.InstructorId != instructorId)
                return Result<List<ClassStudentDto>>.Fail("Class not found.");

            var enrollments = await _executor.ToListAsync(_context.Enrollments.Where(e => e.ClassId == classId));

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => u);

            var dtos = enrollments.Select(e =>
            {
                var user = userMap.GetValueOrDefault(e.StudentId ?? "");
                return new ClassStudentDto
                {
                    StudentId  = e.StudentId,
                    FullName   = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                    Email      = user?.Email,
                    EnrolledAt = e.EnrolledAt
                };
            }).ToList();

            return Result<List<ClassStudentDto>>.Ok(dtos);
        }

        public async Task<Result<List<InstructorProfileDto>>> GetPublicInstructorListAsync()
        {
            var instructors = (await _userManager.GetUsersByRoleAsync("Instructor")).ToList();
            var instructorIds = instructors.Select(u => u.Id ?? "").ToHashSet();

            var allClasses = await _executor.ToListAsync(
                _context.Classes.Where(c => c.Status == ClassStatus.Approved && instructorIds.Contains(c.InstructorId ?? "")));

            var classIds = allClasses.Select(c => c.Id).ToHashSet();
            var allEnrollments = classIds.Any()
                ? await _executor.ToListAsync(_context.Enrollments.Where(e => classIds.Contains(e.ClassId)))
                : new List<Enrollment>();

            var classCountMap = allClasses
                .GroupBy(c => c.InstructorId ?? "")
                .ToDictionary(g => g.Key, g => g.Count());

            var studentCountMap = allClasses
                .GroupBy(c => c.InstructorId ?? "")
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var ids = g.Select(c => c.Id).ToHashSet();
                        return allEnrollments
                            .Where(e => ids.Contains(e.ClassId))
                            .Select(e => e.StudentId)
                            .Distinct()
                            .Count();
                    });

            var dtos = instructors.Select(u => new InstructorProfileDto
            {
                FirstName    = u.FirstName,
                LastName     = u.LastName,
                Email        = u.Email,
                ImageUrl     = u.ImageUrl,
                ClassCount   = classCountMap.GetValueOrDefault(u.Id ?? ""),
                StudentCount = studentCountMap.GetValueOrDefault(u.Id ?? "")
            }).ToList();

            return Result<List<InstructorProfileDto>>.Ok(dtos);
        }
    }
}
