using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EliteAcademy.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IApplicationDbContext    _context;
        private readonly IPaymentProcessorFactory _processorFactory;
        private readonly IPaymentGatewayService   _gatewayService;
        private readonly IUserContextService      _userContextService;
        private readonly IUserManager             _userManager;
        private readonly INotificationService     _notificationService;
        private readonly IEmailService            _emailService;

        public PaymentService(
            IApplicationDbContext    context,
            IPaymentProcessorFactory processorFactory,
            IPaymentGatewayService   gatewayService,
            IUserContextService      userContextService,
            IUserManager             userManager,
            INotificationService     notificationService,
            IEmailService            emailService)
        {
            _context             = context;
            _processorFactory    = processorFactory;
            _gatewayService      = gatewayService;
            _userContextService  = userContextService;
            _userManager         = userManager;
            _notificationService = notificationService;
            _emailService        = emailService;
        }

        public async Task<Result<string>> InitiateAsync(
            int preEnrollmentId, string gatewaySlug, string baseUrl)
        {
            var studentId     = _userContextService.UserId!;
            var preEnrollment = await _context.PreEnrollments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == preEnrollmentId);
            if (preEnrollment == null || preEnrollment.StudentId != studentId)
                return Result<string>.Fail("Selection not found.");
            if (preEnrollment.PaymentStatus != PaymentStatus.Pending)
                return Result<string>.Fail("This selection has already been paid.");

            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == preEnrollment.ClassId);
            if (cls == null || cls.AvailableSeats <= 0)
                return Result<string>.Fail("No available seats remaining.");

            var gateway = await _context.PaymentGateways.AsNoTracking().FirstOrDefaultAsync(g => g.Slug == gatewaySlug && g.IsActive);
            if (gateway == null)
                return Result<string>.Fail("Payment gateway not available.");

            var processor = _processorFactory.GetProcessor(gatewaySlug);
            if (processor == null)
                return Result<string>.Fail("Payment processor not found.");

            var amount     = cls.Price - preEnrollment.DiscountAmount;
            var successUrl = $"{baseUrl}/Payment/Success?txId=0&gateway={gatewaySlug}";
            var cancelUrl  = $"{baseUrl}/Payment/Cancel?txId=0";

            await _context.BeginTransactionAsync();
            try
            {
                var tx = new PaymentTransaction
                {
                    PreEnrollmentId = preEnrollmentId,
                    GatewayId       = gateway.Id,
                    Amount          = amount,
                    Status          = PaymentTransactionStatus.Pending,
                    CreatedAt       = DateTime.UtcNow,
                    CreatedBy       = studentId
                };
                _context.PaymentTransactions.Add(tx);
                await _context.SaveChangesAsync();

                successUrl = $"{baseUrl}/Payment/Success?txId={tx.Id}&gateway={gatewaySlug}";
                cancelUrl  = $"{baseUrl}/Payment/Cancel?txId={tx.Id}";

                var configResult = await _gatewayService.GetDecryptedConfigAsync(gateway.Id);
                var config = configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data) ?? new()
                    : new Dictionary<string, string>();

                var initiateResult = await processor.InitiateAsync(config, amount, tx.Id, successUrl, cancelUrl);
                if (!initiateResult.Success)
                {
                    tx.Status    = PaymentTransactionStatus.Failed;
                    tx.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await _context.CommitTransactionAsync();
                    return Result<string>.Fail(initiateResult.ErrorMessage ?? "Payment initiation failed.");
                }

                await _context.CommitTransactionAsync();
                return Result<string>.Ok(initiateResult.RedirectUrl!, "Payment initiated.");
            }
            catch
            {
                await _context.RollbackTransactionAsync();
                return Result<string>.Fail("Payment initiation failed.");
            }
        }

        public async Task<Result<bool>> HandleSuccessAsync(
            int txId, string gatewaySlug, Dictionary<string, string> callbackParams)
        {
            var tx = await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.Id == txId);
            if (tx == null)
                return Result<bool>.Fail("Transaction not found.");
            if (tx.Status != PaymentTransactionStatus.Pending)
                return Result<bool>.Fail("Transaction already processed.");

            var gateway = await _context.PaymentGateways.AsNoTracking().FirstOrDefaultAsync(g => g.Id == tx.GatewayId);
            if (gateway == null)
                return Result<bool>.Fail("Gateway not found.");

            var processor = _processorFactory.GetProcessor(gatewaySlug);
            if (processor == null)
                return Result<bool>.Fail("Processor not found.");

            var configResult = await _gatewayService.GetDecryptedConfigAsync(gateway.Id);
            var config = configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data) ?? new()
                : new Dictionary<string, string>();

            var verified = await processor.VerifyAsync(config, callbackParams);
            if (!verified)
            {
                tx.Status    = PaymentTransactionStatus.Failed;
                tx.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Result<bool>.Fail("Payment verification failed.");
            }

            await _context.BeginTransactionAsync();
            try
            {
                tx.Status    = PaymentTransactionStatus.Success;
                tx.UpdatedAt = DateTime.UtcNow;

                var preEnrollment = await _context.PreEnrollments.FirstOrDefaultAsync(p => p.Id == tx.PreEnrollmentId);
                if (preEnrollment == null || preEnrollment.PaymentStatus != PaymentStatus.Pending)
                {
                    await _context.SaveChangesAsync();
                    await _context.CommitTransactionAsync();
                    return Result<bool>.Ok(true, "Payment received (already enrolled).");
                }

                var cls = await _context.Classes.FirstOrDefaultAsync(c => c.Id == preEnrollment.ClassId);
                if (cls == null)
                    return Result<bool>.Fail("Class not found.");

                preEnrollment.PaymentStatus = PaymentStatus.Paid;
                preEnrollment.UpdatedAt     = DateTime.UtcNow;
                preEnrollment.UpdatedBy     = preEnrollment.StudentId;

                _context.Enrollments.Add(new Enrollment
                {
                    ClassId    = preEnrollment.ClassId,
                    StudentId  = preEnrollment.StudentId,
                    EnrolledAt = DateTime.UtcNow,
                    CreatedAt  = DateTime.UtcNow,
                    CreatedBy  = preEnrollment.StudentId
                });

                cls.AvailableSeats--;
                cls.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(preEnrollment.CouponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == preEnrollment.CouponCode);
                    if (coupon != null)
                    {
                        coupon.UsageCount++;
                        coupon.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await _context.CommitTransactionAsync();

                try
                {
                    var student = await _userManager.FindByIdAsync(preEnrollment.StudentId!);
                    var studentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "A student";

                    if (!string.IsNullOrWhiteSpace(cls.InstructorId))
                    {
                        await _notificationService.CreateAsync(
                            cls.InstructorId,
                            "New Enrollment",
                            $"{studentName} enrolled in \"{cls.ClassName}\".",
                            $"/Instructor/ClassStudents/{cls.Id}");
                    }

                    if (!string.IsNullOrWhiteSpace(student?.Email))
                    {
                        await _emailService.SendEmailAsync(
                            subject: $"Enrollment Confirmed — {cls.ClassName}",
                            message: BuildEnrollmentEmail(studentName, cls.ClassName ?? ""),
                            toEmails: new List<string> { student.Email });
                    }
                }
                catch { /* don't fail payment if notification/email throws */ }

                return Result<bool>.Ok(true, "Payment successful! You are now enrolled.");
            }
            catch
            {
                await _context.RollbackTransactionAsync();
                return Result<bool>.Fail("Payment processing failed. Please contact support.");
            }
        }

        public async Task<Result<bool>> HandleCancelAsync(int txId)
        {
            var tx = await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.Id == txId);
            if (tx == null) return Result<bool>.Fail("Transaction not found.");

            if (tx.Status == PaymentTransactionStatus.Pending)
            {
                tx.Status    = PaymentTransactionStatus.Cancelled;
                tx.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Result<bool>.Ok(true, "Payment cancelled.");
        }

        private static string BuildEnrollmentEmail(string studentName, string className) => $"""
            <div style="font-family:Arial,sans-serif;max-width:520px;margin:0 auto">
              <h2 style="color:#0d6efd">Enrollment Confirmed!</h2>
              <p>Hi <strong>{studentName}</strong>,</p>
              <p>You have successfully enrolled in <strong>{className}</strong>.</p>
              <p>You can access your class from your <a href="/Student/EnrolledClasses">My Classes</a> page.</p>
              <hr/>
              <p style="color:#888;font-size:12px">Elite Academy</p>
            </div>
            """;
    }
}
