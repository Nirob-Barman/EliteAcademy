using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IApplicationDbContext _context;
        private readonly IAsyncQueryExecutor _executor;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;

        public AnnouncementService(
            IApplicationDbContext context,
            IAsyncQueryExecutor executor,
            IUserContextService userContextService,
            INotificationService notificationService)
        {
            _context             = context;
            _executor            = executor;
            _userContextService  = userContextService;
            _notificationService = notificationService;
        }

        public async Task<Result<List<AnnouncementDto>>> GetClassAnnouncementsAsync(int classId)
        {
            var items = await _executor.ToListAsync(_context.Announcements
                .Where(a => a.ClassId == classId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementDto
                {
                    Id        = a.Id,
                    ClassId   = a.ClassId,
                    Title     = a.Title,
                    Body      = a.Body,
                    CreatedAt = a.CreatedAt
                }));

            return Result<List<AnnouncementDto>>.Ok(items);
        }

        public async Task<Result<bool>> CreateAsync(AnnouncementFormDto dto)
        {
            var instructorId = _userContextService.UserId!;

            if (string.IsNullOrWhiteSpace(dto.Title))
                return Result<bool>.FailField("Title", "Title is required.");

            var cls = await _executor.FirstOrDefaultAsync(_context.Classes.Where(c => c.Id == dto.ClassId));
            if (cls == null || cls.InstructorId != instructorId)
                return Result<bool>.Fail("Class not found.");
            if (cls.Status != ClassStatus.Approved)
                return Result<bool>.Fail("You can only post announcements for approved classes.");

            var announcement = new Announcement
            {
                ClassId   = dto.ClassId,
                Title     = dto.Title.Trim(),
                Body      = dto.Body.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = instructorId
            };
            _context.Add(announcement);
            await _context.SaveChangesAsync();

            var enrollments = await _executor.ToListAsync(_context.Enrollments.Where(e => e.ClassId == dto.ClassId));
            foreach (var enrollment in enrollments)
            {
                if (!string.IsNullOrWhiteSpace(enrollment.StudentId))
                {
                    await _notificationService.CreateAsync(
                        enrollment.StudentId,
                        $"New announcement: {dto.Title}",
                        $"Your instructor posted an announcement in \"{cls.ClassName}\".",
                        $"/Student/ClassAnnouncements?classId={dto.ClassId}");
                }
            }

            return Result<bool>.Ok(true, "Announcement posted.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var instructorId = _userContextService.UserId!;
            var entity = await _executor.FirstOrDefaultAsync(_context.Announcements.Where(a => a.Id == id));
            if (entity == null)
                return Result<bool>.Fail("Announcement not found.");

            var cls = await _executor.FirstOrDefaultAsync(_context.Classes.Where(c => c.Id == entity.ClassId));
            if (cls?.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized.");

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Announcement deleted.");
        }
    }
}
