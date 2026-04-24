using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Instructor
{
    public class ClassFormViewModel
    {
        [Required]
        [StringLength(200)]
        public string ClassName { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Must be at least 1")]
        public int AvailableSeats { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }
}
