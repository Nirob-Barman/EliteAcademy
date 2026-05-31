using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<LoginAudit> LoginAudits { get; }
        DbSet<Class> Classes { get; }
        DbSet<Enrollment> Enrollments { get; }
        DbSet<PreEnrollment> PreEnrollments { get; }
        DbSet<Review> Reviews { get; }
        DbSet<Wishlist> Wishlists { get; }
        DbSet<Coupon> Coupons { get; }
        DbSet<QaQuestion> QaQuestions { get; }
        DbSet<QaAnswer> QaAnswers { get; }
        DbSet<AppNotification> AppNotifications { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<PaymentGateway> PaymentGateways { get; }
        DbSet<PaymentTransaction> PaymentTransactions { get; }
        DbSet<Announcement> Announcements { get; }
        DbSet<InstructorApplication> InstructorApplications { get; }
        DbSet<NotificationPreference> NotificationPreferences { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
