using System.Text;
using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Features.Admin.Commands.ApproveClass;
using EliteAcademy.Application.Features.Admin.Commands.BanStudent;
using EliteAcademy.Application.Features.Admin.Commands.RejectClass;
using EliteAcademy.Application.Features.Admin.Commands.UnbanStudent;
using EliteAcademy.Application.Features.Admin.Queries.GetAdminDashboard;
using EliteAcademy.Application.Features.Admin.Queries.GetAllClasses;
using EliteAcademy.Application.Features.Admin.Queries.GetAllStudents;
using EliteAcademy.Application.Features.Admin.Queries.GetClassEnrollments;
using EliteAcademy.Application.Features.Admin.Queries.GetRevenueReport;
using EliteAcademy.Application.Features.InstructorApplication.Commands.ApproveInstructorApplication;
using EliteAcademy.Application.Features.InstructorApplication.Commands.RejectInstructorApplication;
using EliteAcademy.Application.Features.InstructorApplication.Queries.GetAllInstructorApplications;
using EliteAcademy.Web.ViewModels.Admin;
using EliteAcademy.Web.ViewModels.InstructorApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Dashboard()
        {
            var result = await _mediator.Send(new GetAdminDashboardQuery());
            return View(result.Data ?? new AdminDashboardDto());
        }

        public async Task<IActionResult> Classes()
        {
            var result = await _mediator.Send(new GetAllClassesQuery());
            return View(result.Data ?? new List<ClassDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClass(int id)
        {
            var result = await _mediator.Send(new ApproveClassCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Classes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClass(ClassFeedbackViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Feedback is required.";
                return RedirectToAction(nameof(Classes));
            }

            var result = await _mediator.Send(new RejectClassCommand(vm.ClassId, vm.Feedback));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Classes));
        }

        // ── Student Management ──────────────────────────────────────────────────

        public async Task<IActionResult> Students()
        {
            var result = await _mediator.Send(new GetAllStudentsQuery());
            return View(result.Data ?? new List<AdminStudentDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanStudent(string id)
        {
            var result = await _mediator.Send(new BanStudentCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanStudent(string id)
        {
            var result = await _mediator.Send(new UnbanStudentCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Students));
        }

        // ── Class Enrollments ───────────────────────────────────────────────────

        public async Task<IActionResult> ClassEnrollments(int id)
        {
            var result = await _mediator.Send(new GetClassEnrollmentsQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Classes));
            }
            return View(result.Data);
        }

        public async Task<IActionResult> ExportEnrollments(int id)
        {
            var result = await _mediator.Send(new GetClassEnrollmentsQuery(id));
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Classes));
            }

            var data = result.Data;
            var csv = new StringBuilder();
            csv.AppendLine("Student Name,Email,Enrolled At");
            foreach (var row in data.Enrollments)
                csv.AppendLine($"\"{row.StudentName}\",\"{row.Email}\",\"{row.EnrolledAt:yyyy-MM-dd HH:mm}\"");

            var fileName = $"enrollments_{data.ClassId}_{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // ── Revenue Report ──────────────────────────────────────────────────────

        public async Task<IActionResult> RevenueReport(int? year)
        {
            var reportYear = year ?? DateTime.UtcNow.Year;
            var result = await _mediator.Send(new GetRevenueReportQuery(reportYear));
            return View(result.Data ?? new RevenueReportDto { Year = reportYear });
        }

        public async Task<IActionResult> ExportRevenueReport(int? year)
        {
            var reportYear = year ?? DateTime.UtcNow.Year;
            var result = await _mediator.Send(new GetRevenueReportQuery(reportYear));
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(RevenueReport));
            }

            var data = result.Data;
            var csv = new StringBuilder();

            csv.AppendLine($"Revenue Report — {data.Year}");
            csv.AppendLine($"Total Revenue,{data.TotalRevenue:F2}");
            csv.AppendLine($"Total Transactions,{data.TotalTransactions}");
            csv.AppendLine();

            csv.AppendLine("== Monthly Breakdown ==");
            csv.AppendLine("Month,Revenue,Transactions");
            foreach (var m in data.ByMonth)
                csv.AppendLine($"{m.MonthName},{m.Revenue:F2},{m.Transactions}");
            csv.AppendLine();

            csv.AppendLine("== By Class ==");
            csv.AppendLine("Class Name,Revenue,Enrollments");
            foreach (var c in data.ByClass)
                csv.AppendLine($"\"{c.ClassName}\",{c.Revenue:F2},{c.Enrolled}");
            csv.AppendLine();

            csv.AppendLine("== By Instructor ==");
            csv.AppendLine("Instructor,Revenue,Enrollments");
            foreach (var ins in data.ByInstructor)
                csv.AppendLine($"\"{ins.InstructorName}\",{ins.Revenue:F2},{ins.Enrolled}");

            var fileName = $"revenue_report_{data.Year}_{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // ── Instructor Applications ─────────────────────────────────────────────

        public async Task<IActionResult> InstructorApplications()
        {
            var result = await _mediator.Send(new GetAllInstructorApplicationsQuery());
            return View(result.Data ?? new List<Application.DTOs.InstructorApplication.InstructorApplicationDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id)
        {
            var result = await _mediator.Send(new ApproveInstructorApplicationCommand(id));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(InstructorApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(RejectApplicationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "A rejection reason is required.";
                return RedirectToAction(nameof(InstructorApplications));
            }

            var result = await _mediator.Send(new RejectInstructorApplicationCommand(vm.ApplicationId, vm.AdminNotes));
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(InstructorApplications));
        }
    }
}
