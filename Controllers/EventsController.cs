using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary")]
    public class EventsController : Controller
    {
        private readonly HttpClient _httpClient;

        public EventsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index()
        {
            var events =
                await _httpClient.GetFromJsonAsync<
                    List<EventManagement>>(
                    "https://localhost:44360/api/Events")
                ?? new List<EventManagement>();

            return View(events);
        }

        private async Task<List<EventType>> GetEventTypes()
        {
            var types =
                await _httpClient.GetFromJsonAsync<List<EventType>>(
                    "https://localhost:44360/api/EventType");

            return types?
                .GroupBy(t => t.Name)
                .Select(g => g.First())
                .ToList()
                ?? new List<EventType>();
        }

        private async Task<List<MembershipManagement>> GetMembers()
        {
            var members =
                await _httpClient.GetFromJsonAsync<
                    List<MembershipManagement>>(
                    "https://localhost:44360/api/Members");

            return members?
                .Select(m => new MembershipManagement
                {
                    Id = m.Id,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                })
                .ToList()
                ?? new List<MembershipManagement>();
        }

        private async Task<List<GroupManagement>> GetGroups()
        {
            var groups =
                await _httpClient.GetFromJsonAsync<
                    List<GroupManagement>>(
                    "https://localhost:44360/api/Groups");

            return groups ?? new List<GroupManagement>();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.EventTypes = await GetEventTypes();
            ViewBag.Members = await GetMembers();
            ViewBag.Groups = await GetGroups();

            return View();
        }

        // =========================
        // GROUP EVENTS
        // =========================

        [HttpGet]
        public async Task<IActionResult> GroupEvents(int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
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

            // Get events for this group
            var events =
                await _httpClient.GetFromJsonAsync<
                    List<EventManagement>>(
                    $"https://localhost:44360/api/Events/group/{groupId}")
                ?? new List<EventManagement>();

            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            return View(events);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EventManagement ev)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Groups = await GetGroups();

                return View(ev);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/Events",
                    ev);

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"The API could not save the event: {apiError}");

                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Groups = await GetGroups();

                return View(ev);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ev =
                await _httpClient.GetFromJsonAsync<EventManagement>(
                    $"https://localhost:44360/api/Events/{id}");

            if (ev == null)
            {
                return NotFound();
            }

            ViewBag.EventTypes = await GetEventTypes();
            ViewBag.Members = await GetMembers();
            ViewBag.Groups = await GetGroups();

            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EventManagement ev)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Groups = await GetGroups();

                return View(ev);
            }

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/Events/{ev.Id}",
                    ev);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update the event.");

                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Groups = await GetGroups();

                return View(ev);
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // GROUP EVENTS - CREATE
        // =========================

        [HttpGet]
        public async Task<IActionResult> CreateGroupEvent(int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
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

            // Load event types and members
            ViewBag.EventTypes = await GetEventTypes();
            ViewBag.Members = await GetMembers();
            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            return View();
        }

        // =========================
        // GROUP EVENTS - EDIT
        // =========================

        [HttpGet]
        public async Task<IActionResult> EditGroupEvent(int id)
        {
            var response =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var ev =
                await response.Content
                    .ReadFromJsonAsync<EventManagement>();

            if (ev == null || ev.GroupId == null)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{ev.GroupId}");

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

            ViewBag.EventTypes = await GetEventTypes();
            ViewBag.Members = await GetMembers();
            ViewBag.Group = group;
            ViewBag.GroupId = ev.GroupId;

            return View(ev);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupEvent(
            EventManagement ev)
        {
            if (ev.GroupId == null || ev.GroupId <= 0)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{ev.GroupId}");

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

            if (!ModelState.IsValid)
            {
                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Group = group;
                ViewBag.GroupId = ev.GroupId;

                return View(ev);
            }

            // Keep the event attached to the current group
            ev.GroupId = group.Id;

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/Events/{ev.Id}",
                    ev);

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"The API could not update the event: {apiError}");

                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Group = group;
                ViewBag.GroupId = ev.GroupId;

                return View(ev);
            }

            return RedirectToAction(
                nameof(GroupEvents),
                new { groupId = ev.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupEvent(
            EventManagement ev)
        {
            if (ev.GroupId == null || ev.GroupId <= 0)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{ev.GroupId}");

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

            if (!ModelState.IsValid)
            {
                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Group = group;
                ViewBag.GroupId = ev.GroupId;

                return View(ev);
            }

            // Make sure the event belongs to this group
            ev.GroupId = group.Id;

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/Events",
                    ev);

            if (!response.IsSuccessStatusCode)
            {
                var apiError =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"The API could not save the event: {apiError}");

                ViewBag.EventTypes = await GetEventTypes();
                ViewBag.Members = await GetMembers();
                ViewBag.Group = group;
                ViewBag.GroupId = ev.GroupId;

                return View(ev);
            }

            return RedirectToAction(
                nameof(GroupEvents),
                new { groupId = ev.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to delete the event.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}