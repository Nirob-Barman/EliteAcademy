using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;

        public AnnouncementService(
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            INotificationService notificationService)
        {
            _unitOfWork          = unitOfWork;
            _userContextService  = userContextService;
            _notificationService = notificationService;
        }

        public async Task<Result<List<AnnouncementDto>>> GetClassAnnouncementsAsync(int classId)
        {
            var items = (await _unitOfWork.Repository<Announcement>()
                .Where(a => a.ClassId == classId))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementDto
                {
                    Id        = a.Id,
                    ClassId   = a.ClassId,
                    Title     = a.Title,
                    Body      = a.Body,
                    CreatedAt = a.CreatedAt
                }).ToList();

            return Result<List<AnnouncementDto>>.Ok(items);
        }

        public async Task<Result<bool>> CreateAsync(AnnouncementFormDto dto)
        {
            var instructorId = _userContextService.UserId!;

            if (string.IsNullOrWhiteSpace(dto.Title))
                return Result<bool>.FailField("Title", "Title is required.");

            // Verify instructor owns the class
            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(dto.ClassId);
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
            await _unitOfWork.Repository<Announcement>().AddAsync(announcement);
            await _unitOfWork.SaveChangesAsync();

            // Notify all enrolled students
            var enrollments = (await _unitOfWork.Repository<Enrollment>()
                .Where(e => e.ClassId == dto.ClassId)).ToList();

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
            var entity = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Announcement not found.");

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(entity.ClassId);
            if (cls?.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized.");

            _unitOfWork.Repository<Announcement>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Announcement deleted.");
        }
    }
}
