using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using Announcement = EliteAcademy.Domain.Entities.Instructor.Announcement;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;

namespace EliteAcademy.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser>, IApplicationDbContext, IUnitOfWork
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Standard public DbSet properties — EF Core uses these names for table naming convention
        public DbSet<LoginAudit> LoginAudits { get; set; } = null!;
        public DbSet<Class> Classes { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<PreEnrollment> PreEnrollments { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Wishlist> Wishlists { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<QaQuestion> QaQuestions { get; set; } = null!;
        public DbSet<QaAnswer> QaAnswers { get; set; } = null!;
        public DbSet<AppNotification> AppNotifications { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<PaymentGateway> PaymentGateways { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<InstructorApplication> InstructorApplications { get; set; } = null!;
        public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

        // IApplicationDbContext explicit implementation — services receive IQueryable<T> via the interface
        IQueryable<LoginAudit> IApplicationDbContext.LoginAudits => LoginAudits;
        IQueryable<Class> IApplicationDbContext.Classes => Classes;
        IQueryable<Enrollment> IApplicationDbContext.Enrollments => Enrollments;
        IQueryable<PreEnrollment> IApplicationDbContext.PreEnrollments => PreEnrollments;
        IQueryable<Review> IApplicationDbContext.Reviews => Reviews;
        IQueryable<Wishlist> IApplicationDbContext.Wishlists => Wishlists;
        IQueryable<Coupon> IApplicationDbContext.Coupons => Coupons;
        IQueryable<QaQuestion> IApplicationDbContext.QaQuestions => QaQuestions;
        IQueryable<QaAnswer> IApplicationDbContext.QaAnswers => QaAnswers;
        IQueryable<AppNotification> IApplicationDbContext.AppNotifications => AppNotifications;
        IQueryable<AuditLog> IApplicationDbContext.AuditLogs => AuditLogs;
        IQueryable<PaymentGateway> IApplicationDbContext.PaymentGateways => PaymentGateways;
        IQueryable<PaymentTransaction> IApplicationDbContext.PaymentTransactions => PaymentTransactions;
        IQueryable<Announcement> IApplicationDbContext.Announcements => Announcements;
        IQueryable<InstructorApplication> IApplicationDbContext.InstructorApplications => InstructorApplications;
        IQueryable<NotificationPreference> IApplicationDbContext.NotificationPreferences => NotificationPreferences;

        public new void Add<TEntity>(TEntity entity) where TEntity : class => base.Add(entity);
        public void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => base.AddRange(entities);
        public new void Remove<TEntity>(TEntity entity) where TEntity : class => base.Remove(entity);


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }


        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
            => await Database.BeginTransactionAsync(cancellationToken);

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await SaveChangesAsync(cancellationToken);
            var transaction = Database.CurrentTransaction;
            if (transaction == null)
                throw new InvalidOperationException("No transaction is active.");

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = Database.CurrentTransaction;
            if (transaction == null)
                throw new InvalidOperationException("No transaction is active.");

            await transaction.RollbackAsync(cancellationToken);
        }
    }
}
