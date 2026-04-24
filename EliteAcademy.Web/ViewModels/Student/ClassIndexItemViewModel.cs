using EliteAcademy.Application.DTOs.Class;

namespace EliteAcademy.Web.ViewModels.Student
{
    public class ClassIndexItemViewModel
    {
        public ClassDto Class { get; set; } = new();
        public bool IsSelected { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsWishlisted { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
