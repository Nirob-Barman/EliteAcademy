using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;
        private readonly IFileStorage _fileStorage;

        public ClassService(
            IUnitOfWork unitOfWork,
            IUserManager userManager,
            IUserContextService userContextService,
            IFileStorage fileStorage)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userContextService = userContextService;
            _fileStorage = fileStorage;
        }

        public async Task<Result<List<ClassDto>>> GetApprovedAsync()
        {
            var classes = await _unitOfWork.Repository<Class>()
                .Where(c => c.Status == ClassStatus.Approved);

            var users = await _userManager.GetAllUsersAsync();
            var instructorMap = users.ToDictionary(
                u => u.Id ?? "",
                u => $"{u.FirstName} {u.LastName}".Trim());

            var dtos = classes
                .Select(c => ClassMapper.ToDto(c,
                    instructorMap.GetValueOrDefault(c.InstructorId ?? "")))
                .ToList();

            return Result<List<ClassDto>>.Ok(dtos);
        }

        public async Task<Result<List<ClassDto>>> GetByInstructorAsync()
        {
            var instructorId = _userContextService.UserId!;
            var user = await _userManager.FindByIdAsync(instructorId);
            var instructorName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim();

            var classes = await _unitOfWork.Repository<Class>()
                .Where(c => c.InstructorId == instructorId);

            var dtos = classes
                .Select(c => ClassMapper.ToDto(c, instructorName))
                .ToList();

            return Result<List<ClassDto>>.Ok(dtos);
        }

        public async Task<Result<ClassDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Class>().GetByIdAsync(id);
            if (entity == null)
                return Result<ClassDto>.Fail("Class not found.");

            var user = entity.InstructorId != null
                ? await _userManager.FindByIdAsync(entity.InstructorId)
                : null;
            var instructorName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim();

            return Result<ClassDto>.Ok(ClassMapper.ToDto(entity, instructorName));
        }

        public async Task<Result<int>> CreateAsync(ClassFormDto dto, Stream? imageStream, string? imageFileName)
        {
            var entity = new Class
            {
                ClassName     = dto.ClassName,
                AvailableSeats = dto.AvailableSeats,
                Price         = dto.Price,
                InstructorId  = _userContextService.UserId,
                Status        = ClassStatus.Pending,
                CreatedBy     = _userContextService.UserId,
                CreatedAt     = DateTime.UtcNow
            };

            if (imageStream != null && !string.IsNullOrWhiteSpace(imageFileName))
                entity.ClassImage = await _fileStorage.UploadFileAsync(imageStream, imageFileName, "uploads/classes");

            await _unitOfWork.Repository<Class>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return Result<int>.Ok(entity.Id, "Class submitted for approval.");
        }

        public async Task<Result<bool>> UpdateAsync(ClassFormDto dto, Stream? imageStream, string? imageFileName)
        {
            var entity = await _unitOfWork.Repository<Class>().GetByIdAsync(dto.Id);
            if (entity == null)
                return Result<bool>.Fail("Class not found.");

            if (entity.InstructorId != _userContextService.UserId)
                return Result<bool>.Fail("You do not own this class.");

            entity.ClassName      = dto.ClassName;
            entity.AvailableSeats = dto.AvailableSeats;
            entity.Price          = dto.Price;
            entity.UpdatedBy      = _userContextService.UserId;
            entity.UpdatedAt      = DateTime.UtcNow;

            if (imageStream != null && !string.IsNullOrWhiteSpace(imageFileName))
            {
                if (!string.IsNullOrWhiteSpace(entity.ClassImage))
                    await _fileStorage.DeleteFileAsync(entity.ClassImage);

                entity.ClassImage = await _fileStorage.UploadFileAsync(imageStream, imageFileName, "uploads/classes");
            }

            _unitOfWork.Repository<Class>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Class updated.");
        }
    }
}
