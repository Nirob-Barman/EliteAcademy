using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Features.Announcement.Commands.CreateAnnouncement;
using EliteAcademy.Application.Features.Announcement.Commands.DeleteAnnouncement;
using EliteAcademy.Application.Features.Announcement.Queries.GetClassAnnouncements;
using EliteAcademy.Application.Features.Class.Commands.CreateClass;
using EliteAcademy.Application.Features.Class.Commands.UpdateClass;
using EliteAcademy.Application.Features.Class.Queries.GetClassById;
using EliteAcademy.Application.Features.Class.Queries.GetClassesByInstructor;
using EliteAcademy.Application.Features.Instructor.Queries.GetClassStudents;
using EliteAcademy.Application.Features.Qa.Commands.AnswerQuestion;
using EliteAcademy.Application.Features.Qa.Commands.DeleteAnswer;
using EliteAcademy.Application.Features.Qa.Commands.DeleteQuestion;
using EliteAcademy.Application.Features.Qa.Queries.GetClassQa;
using EliteAcademy.Web.ViewModels.Instructor;
using EliteAcademy.Web.ViewModels.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorClassController : Controller
    {
        private readonly IMediator _mediator;

        public InstructorClassController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ── Class CRUD ────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetClassesByInstructorQuery());
            return View(result.Data ?? new List<ClassDto>());
        }

        [HttpGet]
        public IActionResult Create() => View(new ClassFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = InstructorViewModelMapper.ToDto(vm);
            Stream? stream = null;
            string? fileName = null;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                stream = vm.ImageFile.OpenReadStream();
                fileName = vm.ImageFile.FileName;
            }

            var result = await _mediator.Send(new CreateClassCommand(dto, stream, fileName));
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to create class.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetClassByIdQuery(id));
            if (!result.Success) return NotFound();
            return View(InstructorViewModelMapper.ToEditVm(result.Data!));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassEditFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = InstructorViewModelMapper.ToDto(vm);
            Stream? stream = null;
            string? fileName = null;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                stream = vm.ImageFile.OpenReadStream();
                fileName = vm.ImageFile.FileName;
            }

            var result = await _mediator.Send(new UpdateClassCommand(dto, stream, fileName));
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to update class.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // ── Student Management ────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Students(int id)
        {
            var classResult = await _mediator.Send(new GetClassByIdQuery(id));
            if (!classResult.Success) return NotFound();

            var studentsResult = await _mediator.Send(new GetClassStudentsQuery(id));
            ViewBag.ClassName = classResult.Data!.ClassName;
            ViewBag.ClassId = id;
            return View(studentsResult.Data ?? new List<ClassStudentDto>());
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudentsCsv(int id)
        {
            var result = await _mediator.Send(new GetClassStudentsQuery(id));
            if (!result.Success) return NotFound();

            var sb = new StringBuilder();
            sb.AppendLine("Full Name,Email,Enrolled At");
            foreach (var s in result.Data!)
                sb.AppendLine($"\"{s.FullName}\",\"{s.Email}\",\"{s.EnrolledAt:yyyy-MM-dd HH:mm}\"");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"students-class-{id}.csv");
        }

        // ── Q&A ───────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Qa(int id)
        {
            var classResult = await _mediator.Send(new GetClassByIdQuery(id));
            if (!classResult.Success) return NotFound();

            var qaResult = await _mediator.Send(new GetClassQaQuery(id));
            ViewBag.ClassName = classResult.Data!.ClassName;
            ViewBag.ClassId = id;
            return View(qaResult.Data ?? new List<QaQuestionDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerQuestion(int questionId, int classId, string answerText)
        {
            var result = await _mediator.Send(new AnswerQuestionCommand(new QaAnswerFormDto
            {
                QuestionId = questionId,
                AnswerText = answerText
            }));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Qa), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int classId)
        {
            var result = await _mediator.Send(new DeleteQuestionCommand(questionId));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Qa), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int answerId, int classId)
        {
            var result = await _mediator.Send(new DeleteAnswerCommand(answerId));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Qa), new { id = classId });
        }

        // ── Announcements ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Announcements(int id)
        {
            var classResult = await _mediator.Send(new GetClassByIdQuery(id));
            if (!classResult.Success) return NotFound();

            var result = await _mediator.Send(new GetClassAnnouncementsQuery(id));
            ViewBag.ClassName = classResult.Data!.ClassName;
            ViewBag.ClassId = id;
            return View(result.Data ?? new List<AnnouncementDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnnouncement(int classId, string title, string body)
        {
            var result = await _mediator.Send(new CreateAnnouncementCommand(new AnnouncementFormDto
            {
                ClassId = classId,
                Title = title,
                Body = body
            }));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Announcements), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId, int classId)
        {
            var result = await _mediator.Send(new DeleteAnnouncementCommand(announcementId));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Announcements), new { id = classId });
        }
    }
}
