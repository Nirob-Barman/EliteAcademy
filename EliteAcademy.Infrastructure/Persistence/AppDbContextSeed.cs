using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Enums;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EliteAcademy.Infrastructure.Persistence
{
    public static class AppDbContextSeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationIdentityUser>>();

            string[] roles = { "Admin", "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var users = new[]
            {
                // Admins
                new { Email="admin@eliteacademy.com", Password="Admin@123", First="Admin", Last="User", UserName="admin", Role="Admin" },
                //new { Email="superadmin@eliteacademy.com", Password="Admin@123", First="Super", Last="Admin", UserName="superadmin", Role="Admin" },
                //new { Email="manager@eliteacademy.com", Password="Admin@123", First="Course", Last="Manager", UserName="manager", Role="Admin" },

                // Instructors
                new { Email="james@eliteacademy.com", Password="Instructor@123", First="James", Last="Wright", UserName="james", Role="Instructor" },
                new { Email="sarah@eliteacademy.com", Password="Instructor@123", First="Sarah", Last="Connor", UserName="sarah", Role="Instructor" },
                new { Email="david@eliteacademy.com", Password="Instructor@123", First="David", Last="Miller", UserName="david", Role="Instructor" },
                new { Email="linda@eliteacademy.com", Password="Instructor@123", First="Linda", Last="Taylor", UserName="linda", Role="Instructor" },
                new { Email="robert@eliteacademy.com", Password="Instructor@123", First="Robert", Last="Anderson", UserName="robert", Role="Instructor" },

                // Students
                new { Email="alice@eliteacademy.com", Password="Student@123", First="Alice", Last="Johnson", UserName="alice", Role="Student" },
                new { Email="bob@eliteacademy.com", Password="Student@123", First="Bob", Last="Smith", UserName="bob", Role="Student" },
                new { Email="emma@eliteacademy.com", Password="Student@123", First="Emma", Last="Brown", UserName="emma", Role="Student" },
                new { Email="michael@eliteacademy.com", Password="Student@123", First="Michael", Last="Clark", UserName="michael", Role="Student" },
                new { Email="olivia@eliteacademy.com", Password="Student@123", First="Olivia", Last="Davis", UserName="olivia", Role="Student" },
                new { Email="noah@eliteacademy.com", Password="Student@123", First="Noah", Last="Wilson", UserName="noah", Role="Student" },
                new { Email="ava@eliteacademy.com", Password="Student@123", First="Ava", Last="Moore", UserName="ava", Role="Student" },
                new { Email="liam@eliteacademy.com", Password="Student@123", First="Liam", Last="Taylor", UserName="liam", Role="Student" },
                new { Email="sophia@eliteacademy.com", Password="Student@123", First="Sophia", Last="Martinez", UserName="sophia", Role="Student" }
            };

            //await SeedUserAsync(userManager, "admin@eliteacademy.com",    "Admin@123",      "Admin",  "User",    "admin",   "Admin");

            foreach (var u in users)
            {
                await SeedUserAsync( userManager, u.Email, u.Password, u.First, u.Last, u.UserName, u.Role);
            }

            var classes = new[]
            {
                new { Name = "Web Development Basics", Seats = 20, Price = 99.99m, InstructorEmail = "james@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "C# for Beginners", Seats = 25, Price = 79.99m, InstructorEmail = "sarah@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Database Design Essentials", Seats = 18, Price = 89.99m, InstructorEmail = "david@eliteacademy.com", Status = ClassStatus.Pending },
                new { Name = "UI UX Fundamentals", Seats = 22, Price = 69.99m, InstructorEmail = "linda@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Software Testing Masterclass", Seats = 16, Price = 109.99m, InstructorEmail = "robert@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "ASP.NET Core MVC Bootcamp", Seats = 24, Price = 119.99m, InstructorEmail = "james@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "JavaScript Essentials", Seats = 30, Price = 74.99m, InstructorEmail = "james@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Responsive Web Design", Seats = 21, Price = 84.99m, InstructorEmail = "sarah@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Object Oriented Programming with C#", Seats = 20, Price = 94.99m, InstructorEmail = "sarah@eliteacademy.com", Status = ClassStatus.Pending },
                new { Name = "SQL Query Mastery", Seats = 26, Price = 64.99m, InstructorEmail = "david@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Entity Framework Core in Practice", Seats = 19, Price = 99.99m, InstructorEmail = "david@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Mobile First Interface Design", Seats = 17, Price = 72.99m, InstructorEmail = "linda@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Design Systems and Prototyping", Seats = 14, Price = 114.99m, InstructorEmail = "linda@eliteacademy.com", Status = ClassStatus.Pending },
                new { Name = "Manual Testing Fundamentals", Seats = 28, Price = 59.99m, InstructorEmail = "robert@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Automation Testing with Selenium", Seats = 15, Price = 129.99m, InstructorEmail = "robert@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "REST API Development", Seats = 18, Price = 104.99m, InstructorEmail = "james@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Git and GitHub Collaboration", Seats = 35, Price = 49.99m, InstructorEmail = "sarah@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Data Modeling and Relationships", Seats = 20, Price = 86.99m, InstructorEmail = "david@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Wireframing to High Fidelity Design", Seats = 12, Price = 119.99m, InstructorEmail = "linda@eliteacademy.com", Status = ClassStatus.Approved },
                new { Name = "Quality Assurance Project Workshop", Seats = 18, Price = 89.99m, InstructorEmail = "robert@eliteacademy.com", Status = ClassStatus.Pending }
            };

            foreach (var c in classes)
            {
                if (await db.Classes.AnyAsync(x => x.ClassName == c.Name))
                    continue;

                var instructor = await userManager.FindByEmailAsync(c.InstructorEmail);
                if (instructor == null)
                    continue;

                var domainResult = Class.Create(instructor.Id!, c.Name, c.Seats, c.Price);
                if (!domainResult.IsSuccess)
                    continue;

                var entity = domainResult.Value!;

                if (c.Status == ClassStatus.Approved)
                {
                    entity.Approve();
                    entity.ClearDomainEvents(); // seed data — don't dispatch approval notifications/emails
                }

                db.Add(entity);
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedUserAsync(
            UserManager<ApplicationIdentityUser> userManager,
            string email, string password,
            string firstName, string lastName,
            string userName, string role)
        {
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new ApplicationIdentityUser
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsAgreedToTerms = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
