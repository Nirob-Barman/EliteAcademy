using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Student
{
    public class ReviewFormViewModel
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment must be under 1000 characters.")]
        public string? Comment { get; set; }
    }
}
