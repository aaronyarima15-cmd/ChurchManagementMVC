using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary")]
    public class CommunionController : Controller
    {
        private const string ApiBase =
            "https://localhost:44360/api";

        private readonly HttpClient _httpClient;

        public CommunionController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient =
                httpClientFactory.CreateClient();
        }


        // =========================================================
        // COMMUNION ATTENDANCE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            var selectedDate =
                date?.Date ?? DateTime.Today;


            // -----------------------------------------------------
            // GET MEMBERS
            // -----------------------------------------------------

            var membersResponse =
                await _httpClient.GetAsync(
                    $"{ApiBase}/Members");

            if (!membersResponse.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load members.";

                return View(
                    new CommunionAttendanceVM
                    {
                        CommunionDate =
                            selectedDate
                    });
            }


            var members =
                await membersResponse.Content
                    .ReadFromJsonAsync<
                        List<MembershipManagement>>()
                ?? new List<MembershipManagement>();


            // -----------------------------------------------------
            // GET ATTENDANCE FOR SELECTED DATE
            // -----------------------------------------------------

            var attendanceResponse =
                await _httpClient.GetAsync(
                    $"{ApiBase}/Attendance/date/{selectedDate:yyyy-MM-dd}");

            var attendance =
                attendanceResponse.IsSuccessStatusCode
                    ? await attendanceResponse.Content
                        .ReadFromJsonAsync<
                            List<AttendanceManagement>>()
                        ?? new List<AttendanceManagement>()
                    : new List<AttendanceManagement>();


            // -----------------------------------------------------
            // GET COMMUNION SERVICE INFORMATION
            // -----------------------------------------------------

            var communionResponse =
                await _httpClient.GetAsync(
                    $"{ApiBase}/Communion");

            var communionServices =
                communionResponse.IsSuccessStatusCode
                    ? await communionResponse.Content
                        .ReadFromJsonAsync<
                            List<CommunionService>>()
                        ?? new List<CommunionService>()
                    : new List<CommunionService>();


            var communionService =
                communionServices
                    .FirstOrDefault(c =>
                        c.CommunionDate.Date ==
                        selectedDate);


            // -----------------------------------------------------
            // BUILD MEMBERS LIST
            // -----------------------------------------------------

            var modelMembers =
                members
                    .Where(m =>
                        m.IsBaptized &&
                        !string.IsNullOrWhiteSpace(
                            m.CardNumber))
                    .OrderBy(m => m.FirstName)
                    .ThenBy(m => m.LastName)
                    .Select(member =>
                    {
                        var existingAttendance =
                            attendance
                                .FirstOrDefault(a =>
                                    a.MemberId ==
                                        member.Id &&
                                    a.ServiceType.Equals(
                                        "Holy Communion",
                                        StringComparison
                                            .OrdinalIgnoreCase));

                        return new CommunionAttendanceMemberVM
                        {
                            MemberId =
                                member.Id,

                            MemberName =
                                $"{member.FirstName} {member.LastName} {member.OtherNames}"
                                .Trim(),

                            CardNumber =
                                member.CardNumber,

                            PhoneNumber =
                                member.PhoneNumber,

                            Status =
                                existingAttendance?.Status
                                ?? AttendanceStatus.Absent,

                            Remarks =
                                existingAttendance?.Remarks
                        };
                    })
                    .ToList();


            var model =
                new CommunionAttendanceVM
                {
                    CommunionDate =
                        selectedDate,

                    Minister =
                        communionService?.Minister
                        ?? string.Empty,

                    Remarks =
                        communionService?.Remarks,

                    Members =
                        modelMembers
                };


            return View(model);
        }


        // =========================================================
        // SAVE COMMUNION ATTENDANCE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            CommunionAttendanceVM model)
        {
            if (model.CommunionDate == default)
            {
                ModelState.AddModelError(
                    nameof(model.CommunionDate),
                    "Communion date is required.");
            }


            if (string.IsNullOrWhiteSpace(
                model.Minister))
            {
                ModelState.AddModelError(
                    nameof(model.Minister),
                    "Minister is required.");
            }


            // -----------------------------------------------------
            // FIX FOR POSSIBLE NULL MEMBERS
            // -----------------------------------------------------

            var submittedMembers =
                model.Members
                ?? new List<CommunionAttendanceMemberVM>();


            if (submittedMembers.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "No members were submitted.");
            }


            if (!ModelState.IsValid)
            {
                await ReloadMembersAsync(model);

                return View(model);
            }


            // -----------------------------------------------------
            // PREPARE API PAYLOAD
            // -----------------------------------------------------

            var payload = new
            {
                CommunionDate =
                    model.CommunionDate.Date,

                Minister =
                    model.Minister.Trim(),

                Remarks =
                    model.Remarks?.Trim(),

                Members =
                    submittedMembers
                        .Select(x => new
                        {
                            MemberId =
                                x.MemberId,

                            Status =
                                x.Status,

                            Remarks =
                                x.Remarks
                        })
                        .ToList()
            };


            // -----------------------------------------------------
            // SEND TO API
            // -----------------------------------------------------

            var response =
                await _httpClient.PostAsJsonAsync(
                    $"{ApiBase}/Communion/attendance",
                    payload);


            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    "Unable to save Communion attendance. " +
                    error);

                await ReloadMembersAsync(model);

                return View(model);
            }


            TempData["Success"] =
                "Holy Communion attendance saved successfully.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    date =
                        model.CommunionDate
                });
        }


        // =========================================================
        // COMMUNION REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Report(
            DateTime? date)
        {
            var selectedDate =
                date?.Date ?? DateTime.Today;

            var selectedYear =
                selectedDate.Year;


            // -----------------------------------------------------
            // GET ALL MEMBERS
            // -----------------------------------------------------

            var membersResponse =
                await _httpClient.GetAsync(
                    $"{ApiBase}/Members");


            if (!membersResponse.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load members.";

                return View(
                    new CommunionReportVM
                    {
                        CommunionDate =
                            selectedDate,

                        Year =
                            selectedYear
                    });
            }


            var members =
                await membersResponse.Content
                    .ReadFromJsonAsync<
                        List<MembershipManagement>>()
                ?? new List<MembershipManagement>();


            // -----------------------------------------------------
            // GET ALL ATTENDANCE
            // -----------------------------------------------------

            var attendanceResponse =
                await _httpClient.GetAsync(
                    $"{ApiBase}/Attendance");


            if (!attendanceResponse.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load Communion attendance.";

                return View(
                    new CommunionReportVM
                    {
                        CommunionDate =
                            selectedDate,

                        Year =
                            selectedYear
                    });
            }


            var attendance =
                await attendanceResponse.Content
                    .ReadFromJsonAsync<
                        List<AttendanceManagement>>()
                ?? new List<AttendanceManagement>();


            // -----------------------------------------------------
            // ONLY HOLY COMMUNION RECORDS
            // -----------------------------------------------------

            var communionAttendance =
                attendance
                    .Where(a =>
                        !string.IsNullOrWhiteSpace(
                            a.ServiceType) &&
                        a.ServiceType.Equals(
                            "Holy Communion",
                            StringComparison
                                .OrdinalIgnoreCase))
                    .ToList();


            // =====================================================
            // GET ALL GROUPS
            // =====================================================

            var groups =
                await _httpClient.GetFromJsonAsync<
                    List<GroupManagement>>(
                        $"{ApiBase}/Groups")
                ?? new List<GroupManagement>();


            // =====================================================
            // GET ALL GROUP MEMBERS
            // =====================================================

            var groupMembers =
                new List<GroupMember>();


            foreach (var group in groups)
            {
                var membersInGroup =
                    await _httpClient.GetFromJsonAsync<
                        List<GroupMember>>(
                            $"{ApiBase}/GroupMembers/group/{group.Id}")
                    ?? new List<GroupMember>();


                foreach (var groupMember in membersInGroup)
                {
                    if (groupMember.Group == null)
                    {
                        groupMember.Group =
                            group;
                    }

                    groupMembers.Add(
                        groupMember);
                }
            }


            // =====================================================
            // JANUARY - DECEMBER REPORT
            // =====================================================

            var monthlyMembers =
                new List<CommunionMonthlyMemberVM>();


            foreach (
                var member in members
                    .Where(m => m.IsBaptized).OrderBy(m =>
                        m.FirstName)
                    .ThenBy(m =>
                        m.LastName))
            {
                var monthlyStatus =
                    new string[12];


                // -------------------------------------------------
                // FIND MEMBER GROUP
                // -------------------------------------------------

                var memberGroup =
                    groupMembers
                        .FirstOrDefault(x =>
                            x.MemberId ==
                            member.Id);


                var groupName =
                    memberGroup?.Group?.GroupName;


                if (string.IsNullOrWhiteSpace(
                    groupName) &&
                    memberGroup != null)
                {
                    var groupFromList =
                        groups.FirstOrDefault(x =>
                            x.Id ==
                            memberGroup.GroupId);

                    groupName =
                        groupFromList?.GroupName;
                }


                if (string.IsNullOrWhiteSpace(
                    groupName))
                {
                    groupName =
                        "No Group";
                }


                // -------------------------------------------------
                // BUILD MONTHLY STATUS
                // -------------------------------------------------

                for (int month = 1;
                     month <= 12;
                     month++)
                {
                    var records =
                        communionAttendance
                            .Where(a =>
                                a.MemberId ==
                                    member.Id &&
                                a.AttendanceDate.Year ==
                                    selectedYear &&
                                a.AttendanceDate.Month ==
                                    month)
                            .ToList();


                    // ---------------------------------------------
                    // NO RECORD
                    // ---------------------------------------------

                    if (records.Count == 0)
                    {
                        monthlyStatus[month - 1] =
                            "";
                    }


                    // ---------------------------------------------
                    // PRESENT AT LEAST ONCE
                    // ---------------------------------------------

                    else if (
                        records.Any(a =>
                            a.Status ==
                            AttendanceStatus.Present))
                    {
                        monthlyStatus[month - 1] =
                            "P";
                    }


                    // ---------------------------------------------
                    // RECORD EXISTS BUT NOT PRESENT
                    // ---------------------------------------------

                    else
                    {
                        monthlyStatus[month - 1] =
                            "A";
                    }
                }


                monthlyMembers.Add(
                    new CommunionMonthlyMemberVM
                    {
                        MemberId =
                            member.Id,

                        Name =
                            $"{member.FirstName} {member.LastName} {member.OtherNames}"
                            .Trim(),

                        Group =
                            groupName,

                        Months =
                            monthlyStatus
                    });
            }


            // =====================================================
            // BUILD REPORT MODEL
            // =====================================================

            var model =
                new CommunionReportVM
                {
                    CommunionDate =
                        selectedDate,

                    Year =
                        selectedYear,
MonthlyMembers =
                        monthlyMembers
                };


            return View(model);
        }


        // =========================================================
        // =========================================================
        // MEMBERS PRESENT FOR A SELECTED COMMUNION DATE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> PresentMembers(DateTime? date)
        {
            var selectedDate = date?.Date ?? DateTime.Today;

            var membersResponse =
                await _httpClient.GetAsync($"{ApiBase}/Members");

            if (!membersResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load members.";
                return View("MembersPresent", new CommunionReportVM
                {
                    CommunionDate = selectedDate
                });
            }

            var members =
                await membersResponse.Content
                    .ReadFromJsonAsync<List<MembershipManagement>>()
                ?? new List<MembershipManagement>();

            var attendanceResponse =
                await _httpClient.GetAsync($"{ApiBase}/Attendance");

            if (!attendanceResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load Communion attendance.";
                return View("MembersPresent", new CommunionReportVM
                {
                    CommunionDate = selectedDate
                });
            }

            var attendance =
                await attendanceResponse.Content
                    .ReadFromJsonAsync<List<AttendanceManagement>>()
                ?? new List<AttendanceManagement>();

            var presentMemberIds = attendance
                .Where(a =>
                    a.AttendanceDate.Date == selectedDate &&
                    a.Status == AttendanceStatus.Present &&
                    !string.IsNullOrWhiteSpace(a.ServiceType) &&
                    a.ServiceType.Equals(
                        "Holy Communion",
                        StringComparison.OrdinalIgnoreCase))
                .Select(a => a.MemberId)
                .Distinct()
                .ToHashSet();

            var membersPresent = members
                .Where(m => presentMemberIds.Contains(m.Id))
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .Select(m => new CommunionPresentMemberVM
                {
                    MemberId = m.Id,
                    Name = $"{m.FirstName} {m.LastName} {m.OtherNames}".Trim(),
                    CardNumber = m.CardNumber,
                    PhoneNumber = m.PhoneNumber
                })
                .ToList();

            var model = new CommunionReportVM
            {
                CommunionDate = selectedDate,
                MembersPresent = membersPresent
            };

            return View("MembersPresent", model);
        }

        // EXPORT COMPLETE COMMUNION REPORT TO EXCEL
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? date)
        {
            var selectedDate = date?.Date ?? DateTime.Today;
            var members = await _httpClient.GetFromJsonAsync<List<MembershipManagement>>($"{ApiBase}/Members")
                ?? new List<MembershipManagement>();
            var attendance = await _httpClient.GetFromJsonAsync<List<AttendanceManagement>>($"{ApiBase}/Attendance")
                ?? new List<AttendanceManagement>();
            var communionAttendance = attendance.Where(a =>
                !string.IsNullOrWhiteSpace(a.ServiceType) &&
                a.ServiceType.Equals("Holy Communion", StringComparison.OrdinalIgnoreCase)).ToList();
            var groups = await _httpClient.GetFromJsonAsync<List<GroupManagement>>($"{ApiBase}/Groups")
                ?? new List<GroupManagement>();
            var groupMembers = new List<GroupMember>();
            foreach (var group in groups)
            {
                var groupRecords = await _httpClient.GetFromJsonAsync<List<GroupMember>>(
                    $"{ApiBase}/GroupMembers/group/{group.Id}") ?? new List<GroupMember>();
                groupMembers.AddRange(groupRecords);
            }

            using var workbook = new XLWorkbook();
            var presentWorksheet = workbook.Worksheets.Add("Members Present");
            presentWorksheet.Cell("A1").Value = "Communion Date";
            presentWorksheet.Cell("B1").Value = selectedDate.ToString("dd/MM/yyyy");
            var presentHeaders = new[] { "#", "Member", "Card Number", "Phone" };
            for (var column = 0; column < presentHeaders.Length; column++)
                presentWorksheet.Cell(3, column + 1).Value = presentHeaders[column];
            var presentIds = communionAttendance.Where(a => a.AttendanceDate.Date == selectedDate &&
                    a.Status == AttendanceStatus.Present)
                .Select(a => a.MemberId).Distinct().ToHashSet();
            var presentMembers = members.Where(m => presentIds.Contains(m.Id))
                .OrderBy(m => m.FirstName).ThenBy(m => m.LastName).ToList();
            for (var index = 0; index < presentMembers.Count; index++)
            {
                var member = presentMembers[index];
                presentWorksheet.Cell(index + 4, 1).Value = index + 1;
                presentWorksheet.Cell(index + 4, 2).Value = $"{member.FirstName} {member.LastName} {member.OtherNames}".Trim();
                presentWorksheet.Cell(index + 4, 3).Value = member.CardNumber ?? string.Empty;
                presentWorksheet.Cell(index + 4, 4).Value = member.PhoneNumber;
            }
            presentWorksheet.Range("A3:D3").Style.Font.Bold = true;
            presentWorksheet.Columns().AdjustToContents();

            var annualWorksheet = workbook.Worksheets.Add("Annual Report");
            var annualHeaders = new[] { "Member", "Group", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            for (var column = 0; column < annualHeaders.Length; column++)
                annualWorksheet.Cell(1, column + 1).Value = annualHeaders[column];
            var row = 2;
            foreach (var member in members.Where(m => m.IsBaptized)
                         .OrderBy(m => m.FirstName).ThenBy(m => m.LastName))
            {
                var memberGroup = groupMembers.FirstOrDefault(gm => gm.MemberId == member.Id);
                var groupName = groups.FirstOrDefault(g => g.Id == memberGroup?.GroupId)?.GroupName ?? "No Group";
                annualWorksheet.Cell(row, 1).Value = $"{member.FirstName} {member.LastName} {member.OtherNames}".Trim();
                annualWorksheet.Cell(row, 2).Value = groupName;
                for (var month = 1; month <= 12; month++)
                {
                    var records = communionAttendance.Where(a => a.MemberId == member.Id &&
                        a.AttendanceDate.Year == selectedDate.Year && a.AttendanceDate.Month == month).ToList();
                    var status = records.Count == 0 ? string.Empty :
                        records.Any(a => a.Status == AttendanceStatus.Present) ? "P" : "A";
                    annualWorksheet.Cell(row, month + 2).Value = status;
                }
                row++;
            }
            annualWorksheet.Range(1, 1, 1, annualHeaders.Length).Style.Font.Bold = true;
            annualWorksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Communion_Report_{selectedDate:yyyy-MM-dd}.xlsx");
        }

        // INDIVIDUAL MEMBER COMMUNION REPORT

        // INDIVIDUAL MEMBER COMMUNION REPORT
        // INDIVIDUAL MEMBER COMMUNION REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ExportIndividualMemberToPdf(int memberId, int year)
        {
            if (memberId <= 0)
            {
                return NotFound();
            }
            if (year <= 0)
            {
                year = DateTime.Today.Year;
            }

            var members = await _httpClient.GetFromJsonAsync<List<MembershipManagement>>($"{ApiBase}/Members")
                ?? new List<MembershipManagement>();
            var member = members.FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                return NotFound();
            }

            var attendance = await _httpClient.GetFromJsonAsync<List<AttendanceManagement>>($"{ApiBase}/Attendance")
                ?? new List<AttendanceManagement>();
            var communionAttendance = attendance.Where(a => a.MemberId == memberId &&
                !string.IsNullOrWhiteSpace(a.ServiceType) &&
                a.ServiceType.Equals("Holy Communion", StringComparison.OrdinalIgnoreCase) &&
                a.AttendanceDate.Year == year).ToList();

            var groups = await _httpClient.GetFromJsonAsync<List<GroupManagement>>($"{ApiBase}/Groups")
                ?? new List<GroupManagement>();
            var groupName = "No Group";
            foreach (var group in groups)
            {
                var groupRecords = await _httpClient.GetFromJsonAsync<List<GroupMember>>(
                    $"{ApiBase}/GroupMembers/group/{group.Id}") ?? new List<GroupMember>();
                if (groupRecords.Any(gm => gm.MemberId == memberId))
                {
                    groupName = group.GroupName;
                    break;
                }
            }

            var months = new[] { "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December" };
            var monthRows = new List<List<string>>();
            for (var month = 1; month <= 12; month++)
            {
                var records = communionAttendance.Where(a => a.AttendanceDate.Month == month).ToList();
                var status = records.Count == 0 ? string.Empty :
                    records.Any(a => a.Status == AttendanceStatus.Present) ? "Present" : "Absent";
                monthRows.Add(new List<string> { months[month - 1], status });
            }

            var model = new PdfExportVM
            {
                ReportTitle = "Holy Communion Participation Report",
                Subtitle = $"{member.FirstName} {member.LastName} {member.OtherNames}".Trim(),
                Sections = new List<PdfExportSectionVM>
                {
                    new PdfExportSectionVM
                    {
                        Title = "Member Details",
                        Headers = new List<string> { "Name", "Group", "Year", "Card Number", "Phone" },
                        Rows = new List<List<string>>
                        {
                            new List<string> { $"{member.FirstName} {member.LastName} {member.OtherNames}".Trim(), groupName,
                                year.ToString(), member.CardNumber ?? string.Empty, member.PhoneNumber }
                        }
                    },
                    new PdfExportSectionVM
                    {
                        Title = "Monthly Status",
                        Headers = new List<string> { "Month", "Status" },
                        Rows = monthRows
                    }
                }
            };

            return View("~/Views/Shared/ExportPreview.cshtml", model);
        }

        // RELOAD MEMBERS AFTER VALIDATION ERROR
        // =========================================================

        private async Task ReloadMembersAsync(
            CommunionAttendanceVM model)
        {
            var membersResponse =
                await _httpClient.GetAsync(
                    $"{ApiBase}/Members");


            if (!membersResponse.IsSuccessStatusCode)
            {
                return;
            }


            var members =
                await membersResponse.Content
                    .ReadFromJsonAsync<
                        List<MembershipManagement>>()
                ?? new List<MembershipManagement>();


            var submittedMembers =
                model.Members
                ?? new List<CommunionAttendanceMemberVM>();


            var submittedLookup =
                submittedMembers
                    .GroupBy(x =>
                        x.MemberId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());


            model.Members =
                members
                    .Where(m =>
                        m.IsBaptized &&
                        !string.IsNullOrWhiteSpace(
                            m.CardNumber))
                    .OrderBy(m =>
                        m.FirstName)
                    .ThenBy(m =>
                        m.LastName)
                    .Select(member =>
                    {
                        submittedLookup.TryGetValue(
                            member.Id,
                            out var submitted);


                        return new CommunionAttendanceMemberVM
                        {
                            MemberId =
                                member.Id,

                            MemberName =
                                $"{member.FirstName} {member.LastName} {member.OtherNames}"
                                .Trim(),

                            CardNumber =
                                member.CardNumber,

                            PhoneNumber =
                                member.PhoneNumber,

                            Status =
                                submitted?.Status
                                ?? AttendanceStatus.Absent,

                            Remarks =
                                submitted?.Remarks
                        };
                    })
                    .ToList();
        }
    }
}