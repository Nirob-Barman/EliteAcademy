using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Student;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Commands.PayForClass;

public class PayForClassHandler : IRequestHandler<PayForClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;
    private readonly INotificationService _notificationService;

    public PayForClassHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        IUserContextService userContextService,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _userContextService = userContextService;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(PayForClassCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var preEnrollment = await _context.PreEnrollments.FirstOrDefaultAsync(p => p.Id == request.PreEnrollmentId, cancellationToken);
        if (preEnrollment == null)
            return Result<bool>.Fail("Selection not found.");
        if (preEnrollment.StudentId != studentId)
            return Result<bool>.Fail("Not authorized.");

        var cls = await _context.Classes.FirstOrDefaultAsync(c => c.Id == preEnrollment.ClassId, cancellationToken);
        if (cls == null)
            return Result<bool>.Fail("Class not found.");

        var markPaidResult = preEnrollment.MarkPaid();
        if (!markPaidResult.IsSuccess)
            return Result<bool>.Fail(markPaidResult.Error);

        var decrementResult = cls.DecrementSeat();
        if (!decrementResult.IsSuccess)
            return Result<bool>.Fail(decrementResult.Error);

        cls.UpdatedBy = studentId;

        _context.Enrollments.Add(Enrollment.Create(studentId, preEnrollment.ClassId));

        if (!string.IsNullOrWhiteSpace(preEnrollment.CouponCode))
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == preEnrollment.CouponCode, cancellationToken);
            coupon?.RecordUsage();
        }

        await _context.SaveChangesAsync(cancellationToken);

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
}
