using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary")]
    public class GroupTypesController : Controller
    {
        private readonly HttpClient _httpClient;

        public GroupTypesController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // =========================
        // GROUP TYPES LIST
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var groupTypes =
                await _httpClient.GetFromJsonAsync<
                    List<GroupType>>(
                    "https://localhost:44360/api/GroupType")
                ?? new List<GroupType>();

            return View(groupTypes);
        }

        // =========================
        // CREATE - GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREATE - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GroupType groupType)
        {
            if (!ModelState.IsValid)
            {
                return View(groupType);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/GroupType",
                    groupType);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                "Unable to create group type. API response: "
                + errorMessage);

            return View(groupType);
        }

        // =========================
        // EDIT - GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/GroupType/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var groupType =
                await response.Content
                    .ReadFromJsonAsync<GroupType>();

            if (groupType == null)
            {
                return NotFound();
            }

            return View(groupType);
        }

        // =========================
        // EDIT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            GroupType groupType)
        {
            if (!ModelState.IsValid)
            {
                return View(groupType);
            }

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/GroupType/{groupType.Id}",
                    groupType);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                "Unable to update group type. API response: "
                + errorMessage);

            return View(groupType);
        }

        // =========================
        // DELETE - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/GroupType/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            TempData["Error"] =
                "Unable to delete group type. API response: "
                + errorMessage;

            return RedirectToAction(nameof(Index));
        }
    }
}