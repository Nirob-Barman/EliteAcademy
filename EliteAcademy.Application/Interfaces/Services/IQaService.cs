using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IQaService
    {
        Task<Result<List<QaQuestionDto>>> GetClassQaAsync(int classId);
        Task<Result<bool>> AskAsync(QaAskDto dto);
        Task<Result<bool>> AnswerAsync(QaAnswerFormDto dto);
        Task<Result<bool>> DeleteQuestionAsync(int questionId);
        Task<Result<bool>> DeleteAnswerAsync(int answerId);
    }
}
