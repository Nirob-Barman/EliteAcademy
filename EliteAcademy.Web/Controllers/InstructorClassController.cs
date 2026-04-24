using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.ViewModels.Instructor;
using EliteAcademy.Web.ViewModels.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly IInstructorService _instructorService;
        private readonly IQaService _qaService;
        private readonly IAnnouncementService _announcementService;

        public InstructorClassController(
            IClassService classService,
            IInstructorService instructorService,
            IQaService qaService,
            IAnnouncementService announcementService)
        {
            _classService        = classService;
            _instructorService   = instructorService;
            _qaService           = qaService;
            _announcementService = announcementService;
        }

        // ── Class CRUD ────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var result = await _classService.GetByInstructorAsync();
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
                stream   = vm.ImageFile.OpenReadStream();
                fileName = vm.ImageFile.FileName;
            }

            var result = await _classService.CreateAsync(dto, stream, fileName);
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
            var result = await _classService.GetByIdAsync(id);
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
                stream   = vm.ImageFile.OpenReadStream();
                fileName = vm.ImageFile.FileName;
            }

            var result = await _classService.UpdateAsync(dto, stream, fileName);
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
            var classResult = await _classService.GetByIdAsync(id);
            if (!classResult.Success) return NotFound();

            var studentsResult = await _instructorService.GetClassStudentsAsync(id);
            ViewBag.ClassName = classResult.Data!.ClassName;
            ViewBag.ClassId   = id;
            return View(studentsResult.Data ?? new List<ClassStudentDto>());
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudentsCsv(int id)
        {
            var result = await _instructorService.GetClassStudentsAsync(id);
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
            var classResult = await _classService.GetByIdAsync(id);
            if (!classResult.Success) return NotFound();

            var qaResult = await _qaService.GetClassQaAsync(id);
            ViewBag.ClassName = classResult.Data!.ClassName;
            ViewBag.ClassId   = id;
            return View(qaResult.Data ?? new List<QaQuestionDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerQuestion(int questionId, int classId, string answerText)
        {
            var result = await _qaService.AnswerAsync(new QaAnswerFormDto
            {
                QuestionId = questionId,
                AnswerText = answerText
            });
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Qa), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int classId)
        {
            var result = await _qaService.DeleteQuestionAsync(questionId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Qa), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int answerId, int classId)
        {
            var result = await _qaService.DeleteAnswerAsync(answerId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Qa), new { id = classId });
        }

        // ── Announcements ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Announcements(int id)
        {
            var classResult = await _classService.GetByIdAsync(id);
            if (!classResult.Success) return NotFound();

            var result = await _announcementService.GetClassAnnouncementsAsync(id);
            ViewBag.ClassName = classResult.Data!.ClassName;
            ViewBag.ClassId   = id;
            return View(result.Data ?? new List<AnnouncementDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnnouncement(int classId, string title, string body)
        {
            var result = await _announcementService.CreateAsync(new AnnouncementFormDto
            {
                ClassId = classId,
                Title   = title,
                Body    = body
            });
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Announcements), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId, int classId)
        {
            var result = await _announcementService.DeleteAsync(announcementId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Announcements), new { id = classId });
        }
    }
}
