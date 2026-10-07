using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary")]
    public class GroupsController : Controller
    {
        private readonly HttpClient _httpClient;

        public GroupsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            var groups =
                await _httpClient.GetFromJsonAsync<List<GroupManagement>>(
                    "https://localhost:44360/api/Groups")
                ?? new List<GroupManagement>();

            return View(groups);
        }


        // GET: Groups/Manage/5
        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
        var response = await _httpClient.GetAsync(
            $"https://localhost:44360/api/Groups/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var group = await response.Content.ReadFromJsonAsync<GroupManagement>();

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
        }

        // GET: Groups/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadGroupData();

            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupManagement group)
        {
            if (!ModelState.IsValid)
            {
                await LoadGroupData();
                return View(group);
            }

            group.DateCreated = DateTime.Now;

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/Groups",
                    group);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to create the group: {error}");

                await LoadGroupData();
                return View(group);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync(
                $"https://localhost:44360/api/Groups/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var group = await response.Content.ReadFromJsonAsync<GroupManagement>();

            if (group == null)
            {
                return NotFound();
            }

            await LoadGroupData();

            return View(group);
        }

        // POST: Groups/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupManagement group)
        {
            if (!ModelState.IsValid)
            {
                await LoadGroupData();
                return View(group);
            }

            var response = await _httpClient.PutAsJsonAsync(
                $"https://localhost:44360/api/Groups/{group.Id}",
                group);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to update the group: {error}");

                await LoadGroupData();

                return View(group);
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // GROUP REPORT
        // =========================

        [HttpGet]
        public async Task<IActionResult> Report(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{id}");

            if (!groupResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var group =
                await groupResponse.Content
                    .ReadFromJsonAsync<GroupManagement>();

            if (group == null)
            {
                return NotFound();
            }

            // Get group members
            var members =
                await _httpClient.GetFromJsonAsync<
                    List<GroupMember>>(
                    $"https://localhost:44360/api/GroupMembers/group/{id}")
                ?? new List<GroupMember>();

            // Get group collections
            var collections =
                await _httpClient.GetFromJsonAsync<
                    List<ChurchCollectionManagement>>(
                    $"https://localhost:44360/api/Collections/group/{id}")
                ?? new List<ChurchCollectionManagement>();

            // Get group events
            var events =
                await _httpClient.GetFromJsonAsync<
                    List<EventManagement>>(
                    $"https://localhost:44360/api/Events/group/{id}")
                ?? new List<EventManagement>();

            ViewBag.Group = group;
            ViewBag.Members = members;
            ViewBag.Collections = collections;
            ViewBag.Events = events;

            return View();
        }

        // EXPORT GROUP REPORT TO EXCEL
        [HttpGet]
        public async Task<IActionResult> ExportReportToExcel(int id)
        {
            if (id <= 0) return NotFound();
            var groupResponse = await _httpClient.GetAsync($"https://localhost:44360/api/Groups/{id}");
            if (!groupResponse.IsSuccessStatusCode) return NotFound();
            var group = await groupResponse.Content.ReadFromJsonAsync<GroupManagement>();
            if (group == null) return NotFound();

            var members = await _httpClient.GetFromJsonAsync<List<GroupMember>>(
                $"https://localhost:44360/api/GroupMembers/group/{id}") ?? new List<GroupMember>();
            var collections = await _httpClient.GetFromJsonAsync<List<ChurchCollectionManagement>>(
                $"https://localhost:44360/api/Collections/group/{id}") ?? new List<ChurchCollectionManagement>();
            var events = await _httpClient.GetFromJsonAsync<List<EventManagement>>(
                $"https://localhost:44360/api/Events/group/{id}") ?? new List<EventManagement>();

            using var workbook = new XLWorkbook();
            var summarySheet = workbook.Worksheets.Add("Group Summary");
            summarySheet.Cell(1, 1).Value = "Group Report";
            summarySheet.Cell(1, 1).Style.Font.Bold = true;
            summarySheet.Cell(1, 1).Style.Font.FontSize = 16;
            summarySheet.Cell(3, 1).Value = "Group Name";
            summarySheet.Cell(3, 2).Value = group.GroupName;
            summarySheet.Cell(4, 1).Value = "Description";
            summarySheet.Cell(4, 2).Value = group.Description ?? string.Empty;
            summarySheet.Cell(5, 1).Value = "Date Created";
            summarySheet.Cell(5, 2).Value = group.DateCreated.ToString("dd/MM/yyyy");
            summarySheet.Cell(7, 1).Value = "Total Members";
            summarySheet.Cell(7, 2).Value = members.Count;
            summarySheet.Cell(8, 1).Value = "Total Leaders";
            summarySheet.Cell(8, 2).Value = members.Count(m => !string.Equals(m.Role, "Member", StringComparison.OrdinalIgnoreCase));
            summarySheet.Cell(9, 1).Value = "Total Collections";
            summarySheet.Cell(9, 2).Value = collections.Count;
            summarySheet.Cell(10, 1).Value = "Total Amount Collected";
            summarySheet.Cell(10, 2).Value = collections.Sum(c => c.Amount);
            summarySheet.Cell(10, 2).Style.NumberFormat.Format = "₦#,##0.00";
            summarySheet.Cell(11, 1).Value = "Total Events";
            summarySheet.Cell(11, 2).Value = events.Count;
            summarySheet.Columns().AdjustToContents();

            var memberSheet = workbook.Worksheets.Add("Members");
            memberSheet.Cell(1, 1).Value = "Member";
            memberSheet.Cell(1, 2).Value = "Role";
            memberSheet.Cell(1, 3).Value = "Date Joined";
            for (var index = 0; index < members.Count; index++)
            {
                var member = members[index];
                memberSheet.Cell(index + 2, 1).Value = $"{member.Member?.FirstName} {member.Member?.LastName} {member.Member?.OtherNames}".Trim();
                memberSheet.Cell(index + 2, 2).Value = member.Role;
                memberSheet.Cell(index + 2, 3).Value = member.DateJoined.ToString("dd/MM/yyyy");
            }
            memberSheet.Row(1).Style.Font.Bold = true;
            memberSheet.Columns().AdjustToContents();

            var eventSheet = workbook.Worksheets.Add("Events");
            var eventHeaders = new[] { "Event", "Date", "Time", "Location", "Status" };
            for (var column = 0; column < eventHeaders.Length; column++) eventSheet.Cell(1, column + 1).Value = eventHeaders[column];
            for (var index = 0; index < events.Count; index++)
            {
                var item = events[index];
                eventSheet.Cell(index + 2, 1).Value = item.EventName;
                eventSheet.Cell(index + 2, 2).Value = item.EventDate.ToString("dd/MM/yyyy");
                eventSheet.Cell(index + 2, 3).Value = item.EventTime.ToString();
                eventSheet.Cell(index + 2, 4).Value = item.Location;
                eventSheet.Cell(index + 2, 5).Value = item.Status;
            }
            eventSheet.Row(1).Style.Font.Bold = true;
            eventSheet.Columns().AdjustToContents();

            var collectionSheet = workbook.Worksheets.Add("Collections");
            var collectionHeaders = new[] { "Date", "Collection Type", "Payment Method", "Amount", "Notes" };
            for (var column = 0; column < collectionHeaders.Length; column++) collectionSheet.Cell(1, column + 1).Value = collectionHeaders[column];
            for (var index = 0; index < collections.Count; index++)
            {
                var collection = collections[index];
                collectionSheet.Cell(index + 2, 1).Value = collection.CollectionDate.ToString("dd/MM/yyyy");
                collectionSheet.Cell(index + 2, 2).Value = collection.CollectionType?.Name ?? string.Empty;
                collectionSheet.Cell(index + 2, 3).Value = collection.PaymentMethod?.Name ?? string.Empty;
                collectionSheet.Cell(index + 2, 4).Value = collection.Amount;
                collectionSheet.Cell(index + 2, 4).Style.NumberFormat.Format = "₦#,##0.00";
                collectionSheet.Cell(index + 2, 5).Value = collection.Notes ?? string.Empty;
            }
            collectionSheet.Row(1).Style.Font.Bold = true;
            collectionSheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var safeGroupName = string.Join("_", group.GroupName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"GroupReport_{safeGroupName}_{DateTime.Today:yyyyMMdd}.xlsx");
        }
        // POST: Groups/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/Groups/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to delete the group.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Load dropdown data
        private async Task LoadGroupData()
        {
            var groupTypes =
                await _httpClient.GetFromJsonAsync<List<GroupType>>(
                    "https://localhost:44360/api/GroupType")
                ?? new List<GroupType>();

            var activities =
                await _httpClient.GetFromJsonAsync<List<GroupActivity>>(
                    "https://localhost:44360/api/GroupActivity")
                ?? new List<GroupActivity>();

            var members =
                await _httpClient.GetFromJsonAsync<List<MembershipManagement>>(
                    "https://localhost:44360/api/Members")
                ?? new List<MembershipManagement>();

            ViewBag.GroupTypes = groupTypes;
            ViewBag.Activities = activities;
            ViewBag.Members = members;
        }
    }
}