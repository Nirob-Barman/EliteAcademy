using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Web.ViewModels.Instructor;

namespace EliteAcademy.Web.ViewModels.Mappers
{
    public static class InstructorViewModelMapper
    {
        public static ClassFormDto ToDto(ClassFormViewModel vm) => new()
        {
            ClassName      = vm.ClassName,
            AvailableSeats = vm.AvailableSeats,
            Price          = vm.Price
        };

        public static ClassFormDto ToDto(ClassEditFormViewModel vm) => new()
        {
            Id             = vm.Id,
            ClassName      = vm.ClassName,
            AvailableSeats = vm.AvailableSeats,
            Price          = vm.Price,
            ExistingImage  = vm.ExistingImage
        };

        public static ClassEditFormViewModel ToEditVm(ClassDto dto) => new()
        {
            Id             = dto.Id,
            ClassName      = dto.ClassName ?? "",
            ExistingImage  = dto.ClassImage,
            AvailableSeats = dto.AvailableSeats,
            Price          = dto.Price
        };

        public static InstructorProfileDto ToDto(InstructorProfileViewModel vm) => new()
        {
            FirstName = vm.FirstName,
            LastName  = vm.LastName,
            ImageUrl  = vm.ExistingPhotoUrl
        };

        public static InstructorProfileViewModel ToVm(InstructorProfileDto dto) => new()
        {
            FirstName       = dto.FirstName ?? "",
            LastName        = dto.LastName  ?? "",
            Email           = dto.Email,
            ExistingPhotoUrl = dto.ImageUrl
        };
    }
}
