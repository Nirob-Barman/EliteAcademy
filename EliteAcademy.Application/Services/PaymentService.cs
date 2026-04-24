using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using System.Text.Json;

namespace EliteAcademy.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentProcessorFactory _processorFactory;
        private readonly IPaymentGatewayService _gatewayService;
        private readonly IUserContextService _userContextService;
        private readonly IUserManager _userManager;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymentProcessorFactory processorFactory,
            IPaymentGatewayService gatewayService,
            IUserContextService userContextService,
            IUserManager userManager,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _unitOfWork          = unitOfWork;
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
            var studentId    = _userContextService.UserId!;
            var preEnrollment = await _unitOfWork.Repository<PreEnrollment>().GetByIdAsync(preEnrollmentId);
            if (preEnrollment == null || preEnrollment.StudentId != studentId)
                return Result<string>.Fail("Selection not found.");
            if (preEnrollment.PaymentStatus != PaymentStatus.Pending)
                return Result<string>.Fail("This selection has already been paid.");

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(preEnrollment.ClassId);
            if (cls == null || cls.AvailableSeats <= 0)
                return Result<string>.Fail("No available seats remaining.");

            // Resolve gateway
            var gateway = await _unitOfWork.Repository<PaymentGateway>()
                .FirstOrDefaultAsync(g => g.Slug == gatewaySlug && g.IsActive);
            if (gateway == null)
                return Result<string>.Fail("Payment gateway not available.");

            var processor = _processorFactory.GetProcessor(gatewaySlug);
            if (processor == null)
                return Result<string>.Fail("Payment processor not found.");

            // Determine final amount
            var amount      = cls.Price - preEnrollment.DiscountAmount;
            var successUrl  = $"{baseUrl}/Payment/Success?txId=0&gateway={gatewaySlug}";
            var cancelUrl   = $"{baseUrl}/Payment/Cancel?txId=0";

            // Create pending transaction
            var tx = new PaymentTransaction
            {
                PreEnrollmentId = preEnrollmentId,
                GatewayId       = gateway.Id,
                Amount          = amount,
                Status          = PaymentTransactionStatus.Pending,
                CreatedAt       = DateTime.UtcNow,
                CreatedBy       = studentId
            };
            await _unitOfWork.Repository<PaymentTransaction>().AddAsync(tx);
            await _unitOfWork.SaveChangesAsync();

            // Rebuild URLs with real txId
            successUrl = $"{baseUrl}/Payment/Success?txId={tx.Id}&gateway={gatewaySlug}";
            cancelUrl  = $"{baseUrl}/Payment/Cancel?txId={tx.Id}";

            // Get decrypted config
            var configResult = await _gatewayService.GetDecryptedConfigAsync(gateway.Id);
            var config = configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data) ?? new()
                : new Dictionary<string, string>();

            var initiateResult = await processor.InitiateAsync(config, amount, tx.Id, successUrl, cancelUrl);
            if (!initiateResult.Success)
            {
                tx.Status    = PaymentTransactionStatus.Failed;
                tx.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<PaymentTransaction>().Update(tx);
                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Fail(initiateResult.ErrorMessage ?? "Payment initiation failed.");
            }

            return Result<string>.Ok(initiateResult.RedirectUrl!, "Payment initiated.");
        }

        public async Task<Result<bool>> HandleSuccessAsync(
            int txId, string gatewaySlug, Dictionary<string, string> callbackParams)
        {
            var tx = await _unitOfWork.Repository<PaymentTransaction>().GetByIdAsync(txId);
            if (tx == null)
                return Result<bool>.Fail("Transaction not found.");
            if (tx.Status != PaymentTransactionStatus.Pending)
                return Result<bool>.Fail("Transaction already processed.");

            // Verify with processor
            var gateway = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(tx.GatewayId);
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
                _unitOfWork.Repository<PaymentTransaction>().Update(tx);
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Fail("Payment verification failed.");
            }

            tx.Status    = PaymentTransactionStatus.Success;
            tx.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<PaymentTransaction>().Update(tx);

            // Enroll the student
            var preEnrollment = await _unitOfWork.Repository<PreEnrollment>().GetByIdAsync(tx.PreEnrollmentId);
            if (preEnrollment == null || preEnrollment.PaymentStatus != PaymentStatus.Pending)
            {
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Ok(true, "Payment received (already enrolled).");
            }

            var cls = await _unitOfWork.Repository<Class>().GetByIdAsync(preEnrollment.ClassId);
            if (cls == null)
                return Result<bool>.Fail("Class not found.");

            preEnrollment.PaymentStatus = PaymentStatus.Paid;
            preEnrollment.UpdatedAt     = DateTime.UtcNow;
            preEnrollment.UpdatedBy     = preEnrollment.StudentId;
            _unitOfWork.Repository<PreEnrollment>().Update(preEnrollment);

            await _unitOfWork.Repository<Enrollment>().AddAsync(new Enrollment
            {
                ClassId    = preEnrollment.ClassId,
                StudentId  = preEnrollment.StudentId,
                EnrolledAt = DateTime.UtcNow,
                CreatedAt  = DateTime.UtcNow,
                CreatedBy  = preEnrollment.StudentId
            });

            cls.AvailableSeats--;
            cls.UpdatedAt = DateTime.UtcNow;
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

            // Post-enrollment side-effects (fire-and-forget style — don't fail payment on email error)
            try
            {
                var student = await _userManager.FindByIdAsync(preEnrollment.StudentId!);
                var studentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "A student";

                // Notify instructor
                if (!string.IsNullOrWhiteSpace(cls.InstructorId))
                {
                    await _notificationService.CreateAsync(
                        cls.InstructorId,
                        "New Enrollment",
                        $"{studentName} enrolled in \"{cls.ClassName}\".",
                        $"/Instructor/ClassStudents/{cls.Id}");
                }

                // Confirmation email to student
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

        public async Task<Result<bool>> HandleCancelAsync(int txId)
        {
            var tx = await _unitOfWork.Repository<PaymentTransaction>().GetByIdAsync(txId);
            if (tx == null) return Result<bool>.Fail("Transaction not found.");

            if (tx.Status == PaymentTransactionStatus.Pending)
            {
                tx.Status    = PaymentTransactionStatus.Cancelled;
                tx.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<PaymentTransaction>().Update(tx);
                await _unitOfWork.SaveChangesAsync();
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
