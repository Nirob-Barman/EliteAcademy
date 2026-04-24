namespace EliteAcademy.Domain.Entities.Account
{
    public class AppUser
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        //public string? Password { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAgreedToTerms { get; set; }
        public bool IsBanned { get; set; }
    }
}
