using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EliteAcademy.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IInstructorService, InstructorService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IWishlistService, WishlistService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IQaService, QaService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IInstructorApplicationService, InstructorApplicationService>();
            services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();

            return services;
        }
    }
}
