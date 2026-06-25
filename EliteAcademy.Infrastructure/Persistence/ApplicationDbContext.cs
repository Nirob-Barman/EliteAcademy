using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Domain.Entities;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Infrastructure.Identity.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Announcement = EliteAcademy.Domain.Entities.Instructor.Announcement;

namespace EliteAcademy.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser>, IApplicationDbContext
    {
        private readonly IMediator _mediator;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<LoginAudit> LoginAudits { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<PreEnrollment> PreEnrollments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<QaQuestion> QaQuestions { get; set; }
        public DbSet<QaAnswer> QaAnswers { get; set; }
        public DbSet<AppNotification> AppNotifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PaymentGateway> PaymentGateways { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<InstructorApplication> InstructorApplications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            var events = ChangeTracker.Entries<BaseEntity>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                entry.Entity.ClearDomainEvents();

            foreach (var domainEvent in events)
                await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
            => await Database.BeginTransactionAsync(cancellationToken);

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = Database.CurrentTransaction
                ?? throw new InvalidOperationException("No transaction is active.");
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = Database.CurrentTransaction
                ?? throw new InvalidOperationException("No transaction is active.");
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}
