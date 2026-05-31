using EliteAcademy.Application.Common;
using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;

        public AnnouncementService(
            IApplicationDbContext context,
            IUserContextService userContextService,
            INotificationService notificationService)
        {
            _context             = context;
            _userContextService  = userContextService;
            _notificationService = notificationService;
        }

        public async Task<Result<List<AnnouncementDto>>> GetClassAnnouncementsAsync(int classId)
        {
            var items = await _context.Announcements.AsNoTracking()
                .Where(a => a.ClassId == classId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementDto
                {
                    Id        = a.Id,
                    ClassId   = a.ClassId,
                    Title     = a.Title,
                    Body      = a.Body,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Result<List<AnnouncementDto>>.Ok(items);
        }

        public async Task<Result<bool>> CreateAsync(AnnouncementFormDto dto)
        {
            var instructorId = _userContextService.UserId!;

            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            var domainResult = Announcement.Create(instructorId, cls, dto.Title, dto.Body);
            if (!domainResult.IsSuccess)
                return Result<bool>.Fail(domainResult.Error);

            _context.Announcements.Add(domainResult.Value!);
            await _context.SaveChangesAsync();

            var enrollments = await _context.Enrollments.AsNoTracking().Where(e => e.ClassId == dto.ClassId).ToListAsync();
            foreach (var enrollment in enrollments)
            {
                if (!string.IsNullOrWhiteSpace(enrollment.StudentId))
                {
                    await _notificationService.CreateAsync(
                        enrollment.StudentId,
                        $"New announcement: {dto.Title}",
                        $"Your instructor posted an announcement in \"{cls?.ClassName}\".",
                        $"/Student/ClassAnnouncements?classId={dto.ClassId}");
                }
            }

            return Result<bool>.Ok(true, "Announcement posted.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var instructorId = _userContextService.UserId!;
            var entity = await _context.Announcements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
                return Result<bool>.Fail("Announcement not found.");

            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == entity.ClassId);
            if (cls?.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized.");

            _context.Announcements.Remove(entity);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Announcement deleted.");
        }
    }
}
