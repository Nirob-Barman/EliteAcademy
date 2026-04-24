using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.DTOs.Home;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Web.ViewModels.Student;

namespace EliteAcademy.Web.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<ClassIndexItemViewModel> Classes { get; set; } = new();
        public PlatformStatsDto Stats { get; set; } = new();
        public List<InstructorProfileDto> FeaturedInstructors { get; set; } = new();
        public List<CouponDto> ActiveCoupons { get; set; } = new();
    }
}
