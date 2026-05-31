using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class QaService : IQaService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserManager _userManager;
        private readonly IUserContextService _userContextService;

        public QaService(
            IApplicationDbContext context,
            IUserManager userManager,
            IUserContextService userContextService)
        {
            _context = context;
            _userManager = userManager;
            _userContextService = userContextService;
        }

        public async Task<Result<List<QaQuestionDto>>> GetClassQaAsync(int classId)
        {
            var questions = await _context.QaQuestions.AsNoTracking().Where(q => q.ClassId == classId).ToListAsync();

            if (!questions.Any())
                return Result<List<QaQuestionDto>>.Ok(new List<QaQuestionDto>());

            var questionIds = questions.Select(q => q.Id).ToHashSet();
            var answers = await _context.QaAnswers.AsNoTracking().Where(a => questionIds.Contains(a.QuestionId)).ToListAsync();

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
            var enrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == dto.ClassId);
            if (!enrolled)
                return Result<bool>.Fail("You must be enrolled to ask a question.");

            _context.QaQuestions.Add(new QaQuestion
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

            var question = await _context.QaQuestions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == dto.QuestionId);
            if (question == null)
                return Result<bool>.Fail("Question not found.");

            // Verify the question belongs to one of this instructor's classes
            var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == question.ClassId);
            if (cls == null || cls.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized to answer this question.");

            _context.QaAnswers.Add(new QaAnswer
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
            var question = await _context.QaQuestions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == questionId);
            if (question == null)
                return Result<bool>.Fail("Question not found.");

            var isInstructor = _userContextService.IsInRole("Instructor");
            if (!isInstructor && question.StudentId != userId)
                return Result<bool>.Fail("Not authorized.");

            _context.QaQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Question deleted.");
        }

        public async Task<Result<bool>> DeleteAnswerAsync(int answerId)
        {
            var instructorId = _userContextService.UserId!;
            var answer = await _context.QaAnswers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == answerId);
            if (answer == null)
                return Result<bool>.Fail("Answer not found.");
            if (answer.InstructorId != instructorId)
                return Result<bool>.Fail("Not authorized.");

            _context.QaAnswers.Remove(answer);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true, "Answer deleted.");
        }
    }
}
