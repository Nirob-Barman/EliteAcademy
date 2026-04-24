using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Web.ViewModels.Student;

namespace EliteAcademy.Web.ViewModels.Mappers
{
    public static class StudentViewModelMapper
    {
        public static StudentProfileDto ToDto(StudentProfileViewModel vm) => new()
        {
            FirstName = vm.FirstName,
            LastName  = vm.LastName,
            ImageUrl  = vm.ExistingPhotoUrl
        };

        public static StudentProfileViewModel ToVm(StudentProfileDto dto) => new()
        {
            FirstName        = dto.FirstName ?? "",
            LastName         = dto.LastName  ?? "",
            Email            = dto.Email,
            ExistingPhotoUrl = dto.ImageUrl
        };
    }
}
