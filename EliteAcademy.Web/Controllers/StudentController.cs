using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Web.ViewModels.Mappers;
using EliteAcademy.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IReviewService _reviewService;
        private readonly IWishlistService _wishlistService;
        private readonly IQaService _qaService;
        private readonly IAnnouncementService _announcementService;

        public StudentController(
            IStudentService studentService,
            IReviewService reviewService,
            IWishlistService wishlistService,
            IQaService qaService,
            IAnnouncementService announcementService)
        {
            _studentService      = studentService;
            _reviewService       = reviewService;
            _wishlistService     = wishlistService;
            _qaService           = qaService;
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var result = await _studentService.GetDashboardAsync();
            return View(result.Data ?? new StudentDashboardDto());
        }

        // ── Selections / Cart ─────────────────────────────────────────────────

        public async Task<IActionResult> Cart()
        {
            var result = await _studentService.GetSelectedClassesAsync();
            return View(result.Data ?? new List<PreEnrollmentDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectClass(int classId)
        {
            var result = await _studentService.SelectClassAsync(classId);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int id)
        {
            var result = await _studentService.DeleteSelectedClassAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public async Task<IActionResult> PayForClass(int id)
        {
            var result = await _studentService.PayForClassAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return result.Success
                ? RedirectToAction(nameof(EnrolledClasses))
                : RedirectToAction(nameof(Cart));
        }

        // ── Coupon ────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(int preEnrollmentId, string couponCode)
        {
            var result = await _studentService.ApplyCouponAsync(preEnrollmentId, couponCode);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoupon(int preEnrollmentId)
        {
            var result = await _studentService.RemoveCouponAsync(preEnrollmentId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Cart));
        }

        // ── Enrolled Classes ──────────────────────────────────────────────────

        public async Task<IActionResult> EnrolledClasses()
        {
            var enrollmentsResult = await _studentService.GetEnrolledClassesAsync();
            var reviewedResult    = await _reviewService.GetReviewedClassIdsAsync();

            return View(new EnrolledClassesViewModel
            {
                Enrollments      = enrollmentsResult.Data ?? new(),
                ReviewedClassIds = reviewedResult.Data ?? new()
            });
        }

        // ── Reviews ───────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> LeaveReview(int classId)
        {
            var enrolledResult = await _studentService.GetEnrollmentStatusAsync();
            if (!enrolledResult.Success || !enrolledResult.Data.Enrolled.Contains(classId))
            {
                TempData["Error"] = "You must be enrolled to leave a review.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var reviewedResult = await _reviewService.GetReviewedClassIdsAsync();
            if (reviewedResult.Data?.Contains(classId) == true)
            {
                TempData["Error"] = "You have already reviewed this class.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var classesResult = await _studentService.GetEnrolledClassesAsync();
            var cls = classesResult.Data?.FirstOrDefault(e => e.ClassId == classId);

            return View(new ReviewFormViewModel
            {
                ClassId   = classId,
                ClassName = cls?.ClassName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveReview(ReviewFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _reviewService.CreateAsync(new ReviewFormDto
            {
                ClassId = vm.ClassId,
                Rating  = vm.Rating,
                Comment = vm.Comment
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Could not submit review.");
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(EnrolledClasses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var result = await _reviewService.DeleteAsync(reviewId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(EnrolledClasses));
        }

        // ── Wishlist ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Wishlist()
        {
            var result = await _wishlistService.GetMyWishlistAsync();
            return View(result.Data ?? new());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int classId)
        {
            var result = await _wishlistService.AddAsync(classId);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var result = await _wishlistService.RemoveAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Wishlist));
        }

        // ── Q&A ───────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ClassQa(int classId)
        {
            var enrolledResult = await _studentService.GetEnrollmentStatusAsync();
            if (!enrolledResult.Success || !enrolledResult.Data.Enrolled.Contains(classId))
            {
                TempData["Error"] = "You must be enrolled to view class Q&A.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var enrolledClasses = await _studentService.GetEnrolledClassesAsync();
            var cls = enrolledClasses.Data?.FirstOrDefault(e => e.ClassId == classId);
            ViewBag.ClassName = cls?.ClassName ?? "Class";
            ViewBag.ClassId   = classId;

            var qaResult = await _qaService.GetClassQaAsync(classId);
            return View(qaResult.Data ?? new List<QaQuestionDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskQuestion(int classId, string questionText)
        {
            var result = await _qaService.AskAsync(new QaAskDto
            {
                ClassId      = classId,
                QuestionText = questionText
            });
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(ClassQa), new { classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int classId)
        {
            var result = await _qaService.DeleteQuestionAsync(questionId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(ClassQa), new { classId });
        }

        // ── Announcements ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ClassAnnouncements(int classId)
        {
            var enrolledResult = await _studentService.GetEnrollmentStatusAsync();
            if (!enrolledResult.Success || !enrolledResult.Data.Enrolled.Contains(classId))
            {
                TempData["Error"] = "You must be enrolled to view announcements.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var enrolledClasses = await _studentService.GetEnrolledClassesAsync();
            var cls = enrolledClasses.Data?.FirstOrDefault(e => e.ClassId == classId);
            ViewBag.ClassName = cls?.ClassName ?? "Class";
            ViewBag.ClassId   = classId;

            var result = await _announcementService.GetClassAnnouncementsAsync(classId);
            return View(result.Data ?? new List<AnnouncementDto>());
        }

    }
}
