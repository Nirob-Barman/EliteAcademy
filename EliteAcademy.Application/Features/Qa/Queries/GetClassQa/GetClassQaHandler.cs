using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Qa.Queries.GetClassQa;

public class GetClassQaHandler : IRequestHandler<GetClassQaQuery, Result<List<QaQuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetClassQaHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<List<QaQuestionDto>>> Handle(GetClassQaQuery request, CancellationToken cancellationToken)
    {
        var questions = await _context.QaQuestions.AsNoTracking()
            .Where(q => q.ClassId == request.ClassId)
            .ToListAsync(cancellationToken);

        if (!questions.Any())
            return Result<List<QaQuestionDto>>.Ok(new List<QaQuestionDto>());

        var questionIds = questions.Select(q => q.Id).ToHashSet();
        var answers = await _context.QaAnswers.AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync(cancellationToken);

        var users = await _userManager.GetAllUsersAsync();
        var userMap = users.ToDictionary(u => u.Id ?? "", u => u);

        var dtos = questions
            .OrderByDescending(q => q.CreatedAt)
            .Select(q =>
            {
                var student = userMap.GetValueOrDefault(q.StudentId ?? "");
                return new QaQuestionDto
                {
                    Id = q.Id,
                    ClassId = q.ClassId,
                    StudentId = q.StudentId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "Student",
                    QuestionText = q.QuestionText,
                    AskedAt = q.CreatedAt,
                    Answers      = answers
                        .Where(a => a.QuestionId == q.Id)
                        .OrderBy(a => a.CreatedAt)
                        .Select(a =>
                        {
                            var instructor = userMap.GetValueOrDefault(a.InstructorId ?? "");
                            return new QaAnswerDto
                            {
                                Id = a.Id,
                                InstructorId = a.InstructorId,
                                InstructorName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}".Trim() : "Instructor",
                                AnswerText = a.AnswerText,
                                AnsweredAt = a.CreatedAt
                            };
                        }).ToList()
                };
            }).ToList();

        return Result<List<QaQuestionDto>>.Ok(dtos);
    }
}
