using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary")]
    public class GroupMembersController : Controller
    {
        private readonly HttpClient _httpClient;

        public GroupMembersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }


        // =========================================================
        // GET: GroupMembers/Index?groupId=1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            var groupResponse = await _httpClient.GetAsync(
                $"https://localhost:44360/api/Groups/{groupId}");

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

            var members =
                await _httpClient.GetFromJsonAsync<List<GroupMember>>(
                    $"https://localhost:44360/api/GroupMembers/group/{groupId}")
                ?? new List<GroupMember>();

            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            return View(members);
        }


        // =========================================================
        // GET: GroupMembers/Leaders?groupId=1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Leaders(int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            var groupResponse = await _httpClient.GetAsync(
                $"https://localhost:44360/api/Groups/{groupId}");

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

            var members =
                await _httpClient.GetFromJsonAsync<List<GroupMember>>(
                    $"https://localhost:44360/api/GroupMembers/group/{groupId}")
                ?? new List<GroupMember>();

            var leaders = members
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Role) &&
                    !x.Role.Equals(
                        "Member",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            return View(leaders);
        }


        // =========================================================
        // GET: GroupMembers/Create?groupId=1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            var groupResponse = await _httpClient.GetAsync(
                $"https://localhost:44360/api/Groups/{groupId}");

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

            await LoadMembersAsync();

            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            var groupMember = new GroupMember
            {
                GroupId = groupId,
                Role = "Member",
                DateJoined = DateTime.UtcNow
            };

            return View(groupMember);
        }


        // =========================================================
        // POST: GroupMembers/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GroupMember groupMember)
        {
            if (!ModelState.IsValid)
            {
                await LoadMembersAsync();

                var groupResponse = await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupMember.GroupId}");

                if (groupResponse.IsSuccessStatusCode)
                {
                    ViewBag.Group =
                        await groupResponse.Content
                            .ReadFromJsonAsync<GroupManagement>();
                }

                ViewBag.GroupId = groupMember.GroupId;

                return View(groupMember);
            }

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:44360/api/GroupMembers",
                groupMember);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Member added to the group successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        groupId = groupMember.GroupId
                    });
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                errorMessage);

            await LoadMembersAsync();

            var groupResponseAfterError =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupMember.GroupId}");

            if (groupResponseAfterError.IsSuccessStatusCode)
            {
                ViewBag.Group =
                    await groupResponseAfterError.Content
                        .ReadFromJsonAsync<GroupManagement>();
            }

            ViewBag.GroupId = groupMember.GroupId;

            return View(groupMember);
        }


        // =========================================================
        // GET: GroupMembers/Edit/5
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync(
                $"https://localhost:44360/api/GroupMembers/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var groupMember =
                await response.Content
                    .ReadFromJsonAsync<GroupMember>();

            if (groupMember == null)
            {
                return NotFound();
            }

            var groupResponse = await _httpClient.GetAsync(
                $"https://localhost:44360/api/Groups/{groupMember.GroupId}");

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

            await LoadMembersAsync();

            ViewBag.Group = group;
            ViewBag.GroupId = groupMember.GroupId;

            return View(groupMember);
        }


        // =========================================================
        // POST: GroupMembers/Edit
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            GroupMember groupMember)
        {
            if (!ModelState.IsValid)
            {
                await LoadMembersAsync();

                var groupResponse = await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupMember.GroupId}");

                if (groupResponse.IsSuccessStatusCode)
                {
                    ViewBag.Group =
                        await groupResponse.Content
                            .ReadFromJsonAsync<GroupManagement>();
                }

                ViewBag.GroupId = groupMember.GroupId;

                return View(groupMember);
            }

            var response = await _httpClient.PutAsJsonAsync(
                $"https://localhost:44360/api/GroupMembers/{groupMember.Id}",
                groupMember);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Group member updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        groupId = groupMember.GroupId
                    });
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                errorMessage);

            await LoadMembersAsync();

            var groupResponseAfterError =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupMember.GroupId}");

            if (groupResponseAfterError.IsSuccessStatusCode)
            {
                ViewBag.Group =
                    await groupResponseAfterError.Content
                        .ReadFromJsonAsync<GroupManagement>();
            }

            ViewBag.GroupId = groupMember.GroupId;

            return View(groupMember);
        }


        // =========================================================
        // POST: GroupMembers/Delete
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            int groupId)
        {
            if (id <= 0 || groupId <= 0)
            {
                return NotFound();
            }

            var response = await _httpClient.DeleteAsync(
                $"https://localhost:44360/api/GroupMembers/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Member removed from the group successfully.";
            }
            else
            {
                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Unable to remove the member from the group."
                        : errorMessage;
            }

            return RedirectToAction(
                nameof(Index),
                new
                {
                    groupId = groupId
                });
        }


        // =========================================================
        // LOAD MEMBERS
        // =========================================================

        private async Task LoadMembersAsync()
        {
            var members =
                await _httpClient.GetFromJsonAsync<
                    List<MembershipManagement>>(
                    "https://localhost:44360/api/Members")
                ?? new List<MembershipManagement>();

            ViewBag.Members = members
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToList();
        }
    }
}