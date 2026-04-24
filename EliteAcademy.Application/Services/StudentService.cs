using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;
        private readonly IFileStorage _fileStorage;
        private readonly INotificationService _notificationService;

        public StudentService(
            IUnitOfWork unitOfWork,
            IUserManager userManager,
            IUserContextService userContextService,
            IFileStorage fileStorage,
            INotificationService notificationService)
        {
            _unitOfWork          = unitOfWork;
            _userManager         = userManager;
            _userContextService  = userContextService;
            _fileStorage         = fileStorage;
            _notificationService = notificationService;
        }

        public async Task<Result<StudentDashboardDto>> GetDashboardAsync()
        {
            var studentId = _userContextService.UserId!;

            var selectedCount  = await _unitOfWork.Repository<PreEnrollment>()
                .CountAsync(p => p.StudentId == studentId && p.PaymentStatus == PaymentStatus.Pending);
            var enrolledCount  = await _unitOfWork.Repository<Enrollment>()
                .CountAsync(e => e.StudentId == studentId);
            var availableCount = await _unitOfWork.Repository<Class>()
                .CountAsync(c => c.Status == ClassStatus.Approved);
            var wishlistCount  = await _unitOfWork.Repository<Wishlist>()
                .CountAsync(w => w.StudentId == studentId);

            return Result<StudentDashboardDto>.Ok(new StudentDashboardDto
            {
                SelectedCount         = selectedCount,
                EnrolledCount         = enrolledCount,
                TotalAvailableClasses = availableCount,
                WishlistCount         = wishlistCount
            });
        }

        public async Task<Result<List<PreEnrollmentDto>>> GetSelectedClassesAsync()
        {
            var studentId = _userContextService.UserId!;

            var preEnrollments = (await _unitOfWork.Repository<PreEnrollment>()
                .Where(p => p.StudentId == studentId && p.PaymentStatus == PaymentStatus.Pending))
                .ToList();

            var users = await _userManager.GetAllUsersAsync();
            var instructorMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var dtos = new List<PreEnrollmentDto>();
            foreach (var pe in preEnrollments)
            {
                var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(pe.ClassId);
                var instructorName = cls?.InstructorId != null
                    ? instructorMap.GetValueOrDefault(cls.InstructorId, "")
                    : "";
                dtos.Add(EnrollmentMapper.ToPreEnrollmentDto(pe, cls, instructorName));
            }

            return Result<List<PreEnrollmentDto>>.Ok(dtos);
        }

        public async Task<Result<bool>> SelectClassAsync(int classId)
        {
            var studentId = _userContextService.UserId!;

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(classId);
            if (cls == null)
                return Result<bool>.Fail("Class not found.");
            if (cls.Status != ClassStatus.Approved)
                return Result<bool>.Fail("Class is not available.");
            if (cls.AvailableSeats <= 0)
                return Result<bool>.Fail("No available seats.");

            var alreadySelected = await _unitOfWork.Repository<PreEnrollment>()
                .AnyAsync(p => p.StudentId == studentId && p.ClassId == classId && p.PaymentStatus == PaymentStatus.Pending);
            if (alreadySelected)
                return Result<bool>.Fail("Class is already in your selections.");

            var alreadyEnrolled = await _unitOfWork.Repository<Enrollment>()
                .AnyAsync(e => e.StudentId == studentId && e.ClassId == classId);
            if (alreadyEnrolled)
                return Result<bool>.Fail("You are already enrolled in this class.");

            await _unitOfWork.Repository<PreEnrollment>().AddAsync(new PreEnrollment
            {
                ClassId       = classId,
                StudentId     = studentId,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt     = DateTime.UtcNow,
                CreatedBy     = studentId
            });
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Class added to selections.");
        }

        public async Task<Result<bool>> DeleteSelectedClassAsync(int preEnrollmentId)
        {
            var studentId    = _userContextService.UserId!;
            var preEnrollment = await _unitOfWork.Repository<PreEnrollment>().GetByIdAsync(preEnrollmentId);
            if (preEnrollment == null)
                return Result<bool>.Fail("Selection not found.");
            if (preEnrollment.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");
            if (preEnrollment.PaymentStatus != PaymentStatus.Pending)
                return Result<bool>.Fail("Cannot remove a paid selection.");

            _unitOfWork.Repository<PreEnrollment>().Remove(preEnrollment);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Selection removed.");
        }

        public async Task<Result<bool>> PayForClassAsync(int preEnrollmentId)
        {
            var studentId    = _userContextService.UserId!;
            var preEnrollment = await _unitOfWork.Repository<PreEnrollment>().GetByIdAsync(preEnrollmentId);
            if (preEnrollment == null)
                return Result<bool>.Fail("Selection not found.");
            if (preEnrollment.StudentId != studentId)
                return Result<bool>.Fail("Not authorized.");
            if (preEnrollment.PaymentStatus != PaymentStatus.Pending)
                return Result<bool>.Fail("Already paid.");

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(preEnrollment.ClassId);
            if (cls == null)
                return Result<bool>.Fail("Class not found.");
            if (cls.AvailableSeats <= 0)
                return Result<bool>.Fail("No available seats remaining.");

            preEnrollment.PaymentStatus = PaymentStatus.Paid;
            preEnrollment.UpdatedAt     = DateTime.UtcNow;
            preEnrollment.UpdatedBy     = studentId;
            _unitOfWork.Repository<PreEnrollment>().Update(preEnrollment);

            await _unitOfWork.Repository<Enrollment>().AddAsync(new Enrollment
            {
                ClassId    = preEnrollment.ClassId,
                StudentId  = studentId,
                EnrolledAt = DateTime.UtcNow,
                CreatedAt  = DateTime.UtcNow,
                CreatedBy  = studentId
            });

            cls.AvailableSeats--;
            cls.UpdatedAt = DateTime.UtcNow;
            cls.UpdatedBy = studentId;
            _unitOfWork.Repository<Class>().Update(cls);

            if (!string.IsNullOrWhiteSpace(preEnrollment.CouponCode))
            {
                var coupon = await _unitOfWork.Repository<Coupon>()
                    .FirstOrDefaultAsync(c => c.Code == preEnrollment.CouponCode);
                if (coupon != null)
                {
                    coupon.UsageCount++;
                    coupon.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Coupon>().Update(coupon);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Notify the instructor
            if (!string.IsNullOrWhiteSpace(cls.InstructorId))
            {
                var student = await _userManager.FindByIdAsync(studentId);
                var studentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "A student";
                await _notificationService.CreateAsync(
                    cls.InstructorId,
                    "New Enrollment",
                    $"{studentName} enrolled in \"{cls.ClassName}\".",
                    $"/Instructor/ClassStudents/{cls.Id}");
            }

            return Result<bool>.Ok(true, "Payment successful! You are now enrolled.");
        }

        public async Task<Result<List<EnrollmentDto>>> GetEnrolledClassesAsync()
        {
            var studentId = _userContextService.UserId!;

            var enrollments = (await _unitOfWork.Repository<Enrollment>()
                .Where(e => e.StudentId == studentId))
                .ToList();

            var users = await _userManager.GetAllUsersAsync();
            var instructorMap = users.ToDictionary(u => u.Id ?? "", u => $"{u.FirstName} {u.LastName}".Trim());

            var dtos = new List<EnrollmentDto>();
            foreach (var enrollment in enrollments)
            {
                var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(enrollment.ClassId);
                var instructorName = cls?.InstructorId != null
                    ? instructorMap.GetValueOrDefault(cls.InstructorId, "")
                    : "";
                dtos.Add(EnrollmentMapper.ToEnrollmentDto(enrollment, cls, instructorName));
            }

            return Result<List<EnrollmentDto>>.Ok(dtos);
        }

        public async Task<Result<(HashSet<int> Selected, HashSet<int> Enrolled)>> GetEnrollmentStatusAsync()
        {
            var studentId = _userContextService.UserId!;

            var selectedIds = (await _unitOfWork.Repository<PreEnrollment>()
                .Where(p => p.StudentId == studentId && p.PaymentStatus == PaymentStatus.Pending))
                .Select(p => p.ClassId)
                .ToHashSet();

            var enrolledIds = (await _unitOfWork.Repository<Enrollment>()
                .Where(e => e.StudentId == studentId))
                .Select(e => e.ClassId)
                .ToHashSet();

            return Result<(HashSet<int>, HashSet<int>)>.Ok((selectedIds, enrolledIds));
        }

        public async Task<Result<StudentProfileDto>> GetProfileAsync()
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null)
                return Result<StudentProfileDto>.Fail("User not found.");

            return Result<StudentProfileDto>.Ok(new StudentProfileDto
            {
                FirstName = user.FirstName,
                LastName  = user.LastName,
                Email     = user.Email,
                ImageUrl  = user.ImageUrl
            });
        }

        public async Task<Result<bool>> UpdateProfileAsync(StudentProfileDto dto, Stream? imageStream, string? imageFileName)
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

        public async Task<Result<bool>> ApplyCouponAsync(int preEnrollmentId, string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return Result<bool>.Fail("Please enter a coupon code.");

            var studentId = _userContextService.UserId!;
            var pe = await _unitOfWork.Repository<PreEnrollment>().GetByIdAsync(preEnrollmentId);
            if (pe == null || pe.StudentId != studentId)
                return Result<bool>.Fail("Selection not found.");
            if (pe.PaymentStatus != PaymentStatus.Pending)
                return Result<bool>.Fail("Cannot apply coupon to a paid selection.");

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(pe.ClassId);
            if (cls == null)
                return Result<bool>.Fail("Class not found.");

            var upper  = couponCode.Trim().ToUpper();
            var coupon = await _unitOfWork.Repository<Coupon>()
                .FirstOrDefaultAsync(c => c.Code == upper);

            if (coupon == null)
                return Result<bool>.Fail("Invalid coupon code.");
            if (!coupon.IsActive)
                return Result<bool>.Fail("This coupon is not active.");
            if (DateTime.UtcNow > coupon.ExpiresAt)
                return Result<bool>.Fail("This coupon has expired.");
            if (coupon.MaxUsages > 0 && coupon.UsageCount >= coupon.MaxUsages)
                return Result<bool>.Fail("This coupon has reached its usage limit.");

            pe.CouponCode     = upper;
            pe.DiscountAmount = Math.Round(cls.Price * coupon.DiscountPercent / 100, 2);
            pe.UpdatedAt      = DateTime.UtcNow;
            pe.UpdatedBy      = studentId;

            _unitOfWork.Repository<PreEnrollment>().Update(pe);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, $"{coupon.DiscountPercent}% discount applied! You save ${pe.DiscountAmount:0.00}.");
        }

        public async Task<Result<bool>> RemoveCouponAsync(int preEnrollmentId)
        {
            var studentId = _userContextService.UserId!;
            var pe = await _unitOfWork.Repository<PreEnrollment>().GetByIdAsync(preEnrollmentId);
            if (pe == null || pe.StudentId != studentId)
                return Result<bool>.Fail("Selection not found.");
            if (pe.PaymentStatus != PaymentStatus.Pending)
                return Result<bool>.Fail("Cannot modify a paid selection.");

            pe.CouponCode     = null;
            pe.DiscountAmount = 0;
            pe.UpdatedAt      = DateTime.UtcNow;
            pe.UpdatedBy      = studentId;

            _unitOfWork.Repository<PreEnrollment>().Update(pe);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Coupon removed.");
        }
    }
}
