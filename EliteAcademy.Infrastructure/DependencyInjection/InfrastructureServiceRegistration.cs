using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Email;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Services;
using EliteAcademy.Infrastructure.Identity;
using EliteAcademy.Infrastructure.Identity.Entity;
using EliteAcademy.Infrastructure.Payments;
using EliteAcademy.Infrastructure.Persistence;
using EliteAcademy.Infrastructure.Persistence.Repositories;
using EliteAcademy.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EliteAcademy.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IUserManager, IdentityUserManager>();
            services.AddScoped<ISignInManager, IdentitySignInManager>();
            services.AddScoped<IRoleManager, RoleManager>();

            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            services.AddDataProtection();
            services.AddScoped<IConfigEncryptor, DataProtectionConfigEncryptor>();
            services.AddScoped<IPaymentProcessor, MockPaymentProcessor>();
            services.AddScoped<IPaymentProcessor, StripeCheckoutProcessor>();
            services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();

            services.AddHttpContextAccessor();
            services.AddScoped<IFileStorage, LocalFileStorage>();
            services.AddScoped<IUserContextService, UserContextService>();

            services.AddIdentity<ApplicationIdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie will expire after 30 minutes
                options.SlidingExpiration = true; // Refresh cookie expiration on each request
                options.Cookie.HttpOnly = true; // Cookie can't be accessed by JavaScript
            });

            return services;
        }
    }
}
