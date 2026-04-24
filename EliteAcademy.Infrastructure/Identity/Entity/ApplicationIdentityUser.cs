using Microsoft.AspNetCore.Identity;

namespace EliteAcademy.Infrastructure.Identity.Entity
{
    public class ApplicationIdentityUser: IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ImageUrl { get; set; }
        public string? Address { get; set; }
        public bool IsAgreedToTerms { get; set; }
    }
}
