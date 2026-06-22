using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.DTOs.QA;
using EliteAcademy.Application.DTOs.Review;
using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Features.Announcement.Queries.GetClassAnnouncements;
using EliteAcademy.Application.Features.Qa.Commands.AskQuestion;
using EliteAcademy.Application.Features.Qa.Commands.DeleteQuestion;
using EliteAcademy.Application.Features.Qa.Queries.GetClassQa;
using EliteAcademy.Application.Features.Review.Commands.CreateReview;
using EliteAcademy.Application.Features.Review.Commands.DeleteReview;
using EliteAcademy.Application.Features.Review.Queries.GetReviewedClassIds;
using EliteAcademy.Application.Features.Student.Commands.ApplyCoupon;
using EliteAcademy.Application.Features.Student.Commands.DeleteSelectedClass;
using EliteAcademy.Application.Features.Student.Commands.PayForClass;
using EliteAcademy.Application.Features.Student.Commands.RemoveCoupon;
using EliteAcademy.Application.Features.Student.Commands.SelectClass;
using EliteAcademy.Application.Features.Student.Queries.GetEnrolledClasses;
using EliteAcademy.Application.Features.Student.Queries.GetEnrollmentStatus;
using EliteAcademy.Application.Features.Student.Queries.GetSelectedClasses;
using EliteAcademy.Application.Features.Student.Queries.GetStudentDashboard;
using EliteAcademy.Application.Features.Wishlist.Commands.AddToWishlist;
using EliteAcademy.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using EliteAcademy.Application.Features.Wishlist.Queries.GetMyWishlist;
using EliteAcademy.Web.ViewModels.Mappers;
using EliteAcademy.Web.ViewModels.Student;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Dashboard()
        {
            var result = await _mediator.Send(new GetStudentDashboardQuery());
            return View(result.Data ?? new StudentDashboardDto());
        }

        // ── Selections / Cart ─────────────────────────────────────────────────

        public async Task<IActionResult> Cart()
        {
            var result = await _mediator.Send(new GetSelectedClassesQuery());
            return View(result.Data ?? new List<PreEnrollmentDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectClass(int classId)
        {
            var result = await _mediator.Send(new SelectClassCommand(classId));
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int id)
        {
            var result = await _mediator.Send(new DeleteSelectedClassCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public async Task<IActionResult> PayForClass(int id)
        {
            var result = await _mediator.Send(new PayForClassCommand(id));
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
            var result = await _mediator.Send(new ApplyCouponCommand(preEnrollmentId, couponCode));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoupon(int preEnrollmentId)
        {
            var result = await _mediator.Send(new RemoveCouponCommand(preEnrollmentId));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Cart));
        }

        // ── Enrolled Classes ──────────────────────────────────────────────────

        public async Task<IActionResult> EnrolledClasses()
        {
            var enrollmentsResult = await _mediator.Send(new GetEnrolledClassesQuery());
            var reviewedResult    = await _mediator.Send(new GetReviewedClassIdsQuery());

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
            var enrolledResult = await _mediator.Send(new GetEnrollmentStatusQuery());
            if (!enrolledResult.Success || !enrolledResult.Data.Enrolled.Contains(classId))
            {
                TempData["Error"] = "You must be enrolled to leave a review.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var reviewedResult = await _mediator.Send(new GetReviewedClassIdsQuery());
            if (reviewedResult.Data?.Contains(classId) == true)
            {
                TempData["Error"] = "You have already reviewed this class.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var classesResult = await _mediator.Send(new GetEnrolledClassesQuery());
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

            var result = await _mediator.Send(new CreateReviewCommand(new ReviewFormDto
            {
                ClassId = vm.ClassId,
                Rating  = vm.Rating,
                Comment = vm.Comment
            }));

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
            var result = await _mediator.Send(new DeleteReviewCommand(reviewId));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(EnrolledClasses));
        }

        // ── Wishlist ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Wishlist()
        {
            var result = await _mediator.Send(new GetMyWishlistQuery());
            return View(result.Data ?? new());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int classId)
        {
            var result = await _mediator.Send(new AddToWishlistCommand(classId));
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var result = await _mediator.Send(new RemoveFromWishlistCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Wishlist));
        }

        // ── Q&A ───────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ClassQa(int classId)
        {
            var enrolledResult = await _mediator.Send(new GetEnrollmentStatusQuery());
            if (!enrolledResult.Success || !enrolledResult.Data.Enrolled.Contains(classId))
            {
                TempData["Error"] = "You must be enrolled to view class Q&A.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var enrolledClasses = await _mediator.Send(new GetEnrolledClassesQuery());
            var cls = enrolledClasses.Data?.FirstOrDefault(e => e.ClassId == classId);
            ViewBag.ClassName = cls?.ClassName ?? "Class";
            ViewBag.ClassId   = classId;

            var qaResult = await _mediator.Send(new GetClassQaQuery(classId));
            return View(qaResult.Data ?? new List<QaQuestionDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskQuestion(int classId, string questionText)
        {
            var result = await _mediator.Send(new AskQuestionCommand(new QaAskDto
            {
                ClassId      = classId,
                QuestionText = questionText
            }));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(ClassQa), new { classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int classId)
        {
            var result = await _mediator.Send(new DeleteQuestionCommand(questionId));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(ClassQa), new { classId });
        }

        // ── Announcements ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ClassAnnouncements(int classId)
        {
            var enrolledResult = await _mediator.Send(new GetEnrollmentStatusQuery());
            if (!enrolledResult.Success || !enrolledResult.Data.Enrolled.Contains(classId))
            {
                TempData["Error"] = "You must be enrolled to view announcements.";
                return RedirectToAction(nameof(EnrolledClasses));
            }

            var enrolledClasses = await _mediator.Send(new GetEnrolledClassesQuery());
            var cls = enrolledClasses.Data?.FirstOrDefault(e => e.ClassId == classId);
            ViewBag.ClassName = cls?.ClassName ?? "Class";
            ViewBag.ClassId   = classId;

            var result = await _mediator.Send(new GetClassAnnouncementsQuery(classId));
            return View(result.Data ?? new List<AnnouncementDto>());
        }

    }
}
