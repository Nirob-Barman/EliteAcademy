using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.InstructorApplication
{
    public class InstructorApplicationFormViewModel
    {
        [Required(ErrorMessage = "Please write a short bio.")]
        [StringLength(2000, MinimumLength = 50, ErrorMessage = "Bio must be between 50 and 2000 characters.")]
        [Display(Name = "About You")]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe your expertise.")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "Expertise must be between 10 and 300 characters.")]
        [Display(Name = "Area of Expertise")]
        public string Expertise { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please explain your motivation.")]
        [StringLength(2000, MinimumLength = 50, ErrorMessage = "Motivation must be between 50 and 2000 characters.")]
        [Display(Name = "Why Do You Want to Teach?")]
        public string Motivation { get; set; } = string.Empty;
    }
}
