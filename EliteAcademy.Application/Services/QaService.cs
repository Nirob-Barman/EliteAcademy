using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.Services
{
    public class QaService : IQaService
    {
        private readonly IApplicationDbContext _context;
        private readonly IAsyncQueryExecutor _executor;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public QaService(
            IApplicationDbContext context,
            IAsyncQueryExecutor executor,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _context = context;
            _executor = executor;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<QaQuestionDto>>> GetClassQaAsync(int classId)
        {
            var questions = await _executor.ToListAsync(_context.QaQuestions.Where(q => q.ClassId == classId), noTracking: true);

            if (!questions.Any())
                return Result<List<QaQuestionDto>>.Ok(new List<QaQuestionDto>());

            var questionIds = questions.Select(q => q.Id).ToHashSet();
            var answers = await _executor.ToListAsync(_context.QaAnswers.Where(a => questionIds.Contains(a.QuestionId)), noTracking: true);

            var users = await _userManager.GetAllUsersAsync();
            var userMap = users.ToDictionary(u => u.Id ?? "", u => u);

            var dtos = questions
                .OrderByDescending(q => q.CreatedAt)
                .Select(q =>
                {
                    var student = userMap.GetValueOrDefault(q.StudentId ?? "");
                    return new QaQuestionDto
                    {
                        Id          = q.Id,
                        ClassId     = q.ClassId,
                        StudentId   = q.StudentId,
                        StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "Student",
                        QuestionText = q.QuestionText,
                        AskedAt     = q.CreatedAt,
                        Answers     = answers
                            .Where(a => a.QuestionId == q.Id)
                            .OrderBy(a => a.CreatedAt)
                            .Select(a =>
                            {
                                var instructor = userMap.GetValueOrDefault(a.InstructorId ?? "");
                                return new QaAnswerDto
                                {
                                    Id             = a.Id,
                                    InstructorId   = a.InstructorId,
                                    InstructorName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}".Trim() : "Instructor",
                                    AnswerText     = a.AnswerText,
                                    AnsweredAt     = a.CreatedAt
                                };
                            }).ToList()
                    };
                }).ToList();

            return Result<List<QaQuestionDto>>.Ok(dtos);
        }

        public async Task<Result<bool>> AskAsync(QaAskDto dto)
        {
            var studentId = _userContextService.UserId!;

            if (string.IsNullOrWhiteSpace(dto.QuestionText))
                return Result<bool>.FailField("QuestionText", "Question cannot be empty.");

            // Ensure the student is enrolled
            var enrolled = await _executor.AnyAsync(_context.Enrollments.Where(e => e.StudentId == studentId && e.ClassId == dto.ClassId));
            if (!enrolled)
                return Result<bool>.Fail("You must be enrolled to ask a question.");

            _context.Add(new QaQuestion
            {
                ClassId = dto.ClassId,
                StudentId = studentId,
                QuestionText = dto.QuestionText.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Question posted.");
        }

        public async Task<Result<bool>> AnswerAsync(QaAnswerFormDto dto)
        {
            var instructorId = _userContextService.UserId!;

            if (string.IsNullOrWhiteSpace(dto.AnswerText))
                return Result<bool>.FailField("AnswerText", "Answer cannot be empty.");

            var question = await _executor.FirstOrDefaultAsync(_context.QaQuestions.Where(q => q.Id == dto.QuestionId), noTracking: true);
            if (question == null)
                return Result<bool>.Fail("Question not found.");

            // Verify the question belongs to one of this instructor's classes
            var cls = await _executor.FirstOrDefaultAsync(_context.Classes.Where(c => c.Id == question.ClassId), noTracking: true);
            if (cls == null || cls.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized to answer this question.");

            _context.Add(new QaAnswer
            {
                QuestionId = dto.QuestionId,
                InstructorId = instructorId,
                AnswerText = dto.AnswerText.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = instructorId
            });
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Answer posted.");
        }

        public async Task<Result<bool>> DeleteQuestionAsync(int questionId)
        {
            var userId = _userContextService.UserId!;
            var question = await _executor.FirstOrDefaultAsync(_context.QaQuestions.Where(q => q.Id == questionId), noTracking: true);
            if (question == null)
                return Result<bool>.Fail("Question not found.");

            var isInstructor = _userContextService.IsInRole("Instructor");
            if (!isInstructor && question.StudentId != userId)
                return Result<bool>.Fail("Not authorized.");

            _context.Remove(question);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Question deleted.");
        }

        public async Task<Result<bool>> DeleteAnswerAsync(int answerId)
        {
            var instructorId = _userContextService.UserId!;
            var answer = await _executor.FirstOrDefaultAsync(_context.QaAnswers.Where(a => a.Id == answerId), noTracking: true);
            if (answer == null)
                return Result<bool>.Fail("Answer not found.");
            if (answer.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized.");

            _context.Remove(answer);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Answer deleted.");
        }
    }
}
