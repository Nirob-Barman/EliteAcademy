using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Web.ViewModels.Account;

namespace EliteAcademy.Web.ViewModels.Mappers
{
    public static class AccountMapper
    {
        public static RegisterDto ToDto(RegisterViewModel vm)
        => new RegisterDto
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Username = vm.Username,
            Email = vm.Email,
            PhoneNumber = vm.PhoneNumber,
            Password = vm.Password,
            DateOfBirth = vm.DateOfBirth,
            Gender = vm.Gender,
            Address = vm.Address,
        };

        public static LoginDto ToDto(LoginViewModel vm)
            => new LoginDto
            {
                Email = vm.Email,
                Password = vm.Password,
                RememberMe = vm.RememberMe
            };

        //public static EditProfileDto ToDto(ProfileViewModel vm)
        //    => new EditProfileDto
        //    {
        //        FullName = vm?.FullName,
        //        Address = vm?.Address,
        //    };

        //public static ChangePasswordDto ToDto(ChangePasswordViewModel vm)
        //    => new ChangePasswordDto
        //    {
        //        CurrentPassword = vm.CurrentPassword,
        //        NewPassword = vm.NewPassword,
        //        ConfirmPassword = vm.ConfirmPassword
        //    };

        //public static ForgotPasswordDto ToDto(ForgotPasswordViewModel vm)
        //    => new ForgotPasswordDto { Email = vm.Email };

        //public static ResetPasswordDto ToDto(ResetPasswordViewModel vm)
        //    => new ResetPasswordDto
        //    {
        //        Email = vm.Email,
        //        Token = vm.Token,
        //        NewPassword = vm.NewPassword,
        //    };

        //public static ProfileViewModel ToViewModel(EditProfileDto dto)
        //    => new ProfileViewModel
        //    {
        //        FullName = dto.FullName,
        //        Address = dto.Address,
        //    };
    }
}
