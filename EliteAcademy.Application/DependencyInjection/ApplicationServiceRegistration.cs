using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EliteAcademy.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
            services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();

            return services;
        }
    }
}
