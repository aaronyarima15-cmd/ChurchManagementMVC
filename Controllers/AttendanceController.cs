using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary,Member")]
    public class AttendanceController : Controller
    {
        private readonly HttpClient _httpClient;

        private const string ApiBase =
            "https://localhost:44360/api";

        public AttendanceController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient =
                httpClientFactory.CreateClient();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response =
                await _httpClient.GetAsync(
                    $"{ApiBase}/AttendanceSummary");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load attendance.";

                return View(
                    new List<AttendanceSummary>());
            }

            var attendance =
                await response.Content
                    .ReadFromJsonAsync<
                        List<AttendanceSummary>>();

            return View(
                attendance ??
                new List<AttendanceSummary>());
        }


        // EXPORT ATTENDANCE TO EXCEL
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var response = await _httpClient.GetAsync($"{ApiBase}/AttendanceSummary");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to export attendance.";
                return RedirectToAction(nameof(Index));
            }

            var attendance = await response.Content.ReadFromJsonAsync<List<AttendanceSummary>>()
                ?? new List<AttendanceSummary>();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance");
            worksheet.Cell(1, 1).Value = "Attendance Report";
            worksheet.Range(1, 1, 1, 6).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            var headers = new[] { "Date", "Service", "Men", "Women", "Children", "Total" };
            for (var column = 0; column < headers.Length; column++)
            {
                worksheet.Cell(3, column + 1).Value = headers[column];
            }
            worksheet.Range(3, 1, 3, headers.Length).Style.Font.Bold = true;
            worksheet.Range(3, 1, 3, headers.Length).Style.Fill.SetBackgroundColor(XLColor.LightGreen);
            var row = 4;
            foreach (var item in attendance.OrderByDescending(a => a.AttendanceDate))
            {
                worksheet.Cell(row, 1).Value = item.AttendanceDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 2).Value = item.ServiceType;
                worksheet.Cell(row, 3).Value = item.Men;
                worksheet.Cell(row, 4).Value = item.Women;
                worksheet.Cell(row, 5).Value = item.Children;
                worksheet.Cell(row, 6).Value = item.Total;
                row++;
            }
            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Attendance_Report_{DateTime.Today:yyyy-MM-dd}.xlsx");
        }




        [Authorize(
            Roles = "Admin,Pastor,Secretary")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AttendanceSummary
            {
                AttendanceDate =
                    DateTime.Today
            };

            return View(model);
        }


        [Authorize(
            Roles = "Admin,Pastor,Secretary")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AttendanceSummary attendance)
        {
            if (!ModelState.IsValid)
            {
                return View(attendance);
            }

            attendance.AttendanceDate =
                attendance.AttendanceDate.Date;

            attendance.Total =
                attendance.Men +
                attendance.Women +
                attendance.Children;

            var response =
                await _httpClient.PostAsJsonAsync(
                    $"{ApiBase}/AttendanceSummary",
                    attendance);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    "Unable to save attendance. " +
                    error);

                return View(attendance);
            }

            TempData["Success"] =
                "Attendance recorded successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        [Authorize(
            Roles = "Admin,Pastor,Secretary")]
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var response =
                await _httpClient.GetAsync(
                    $"{ApiBase}/AttendanceSummary/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Attendance record not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            var attendance =
                await response.Content
                    .ReadFromJsonAsync<
                        AttendanceSummary>();

            if (attendance == null)
            {
                TempData["Error"] =
                    "Attendance record not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(attendance);
        }


        [Authorize(
            Roles = "Admin,Pastor,Secretary")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            AttendanceSummary attendance)
        {
            if (id != attendance.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(attendance);
            }

            attendance.AttendanceDate =
                attendance.AttendanceDate.Date;

            attendance.Total =
                attendance.Men +
                attendance.Women +
                attendance.Children;

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"{ApiBase}/AttendanceSummary/{id}",
                    attendance);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    "Unable to update attendance. " +
                    error);

                return View(attendance);
            }

            TempData["Success"] =
                "Attendance updated successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        [Authorize(
            Roles = "Admin,Pastor,Secretary")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"{ApiBase}/AttendanceSummary/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Attendance record deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Unable to delete attendance record.";
            }

            return RedirectToAction(
                nameof(Index));
        }
    }
}