using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;

namespace EliteAcademy.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        IQueryable<LoginAudit> LoginAudits { get; }
        IQueryable<Class> Classes { get; }
        IQueryable<Enrollment> Enrollments { get; }
        IQueryable<PreEnrollment> PreEnrollments { get; }
        IQueryable<Review> Reviews { get; }
        IQueryable<Wishlist> Wishlists { get; }
        IQueryable<Coupon> Coupons { get; }
        IQueryable<QaQuestion> QaQuestions { get; }
        IQueryable<QaAnswer> QaAnswers { get; }
        IQueryable<AppNotification> AppNotifications { get; }
        IQueryable<AuditLog> AuditLogs { get; }
        IQueryable<PaymentGateway> PaymentGateways { get; }
        IQueryable<PaymentTransaction> PaymentTransactions { get; }
        IQueryable<Announcement> Announcements { get; }
        IQueryable<InstructorApplication> InstructorApplications { get; }
        IQueryable<NotificationPreference> NotificationPreferences { get; }

        void Add<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
