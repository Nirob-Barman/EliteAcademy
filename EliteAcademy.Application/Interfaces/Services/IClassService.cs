using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IClassService
    {
        Task<Result<List<ClassDto>>>  GetApprovedAsync();
        Task<Result<List<ClassDto>>>  GetByInstructorAsync();
        Task<Result<ClassDto>>        GetByIdAsync(int id);
        Task<Result<int>>             CreateAsync(ClassFormDto dto, Stream? imageStream, string? imageFileName);
        Task<Result<bool>>            UpdateAsync(ClassFormDto dto, Stream? imageStream, string? imageFileName);
    }
}
