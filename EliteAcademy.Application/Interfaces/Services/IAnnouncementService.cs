using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IAnnouncementService
    {
        Task<Result<List<AnnouncementDto>>> GetClassAnnouncementsAsync(int classId);
        Task<Result<bool>> CreateAsync(AnnouncementFormDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
