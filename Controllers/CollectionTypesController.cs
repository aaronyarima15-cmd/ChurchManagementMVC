using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Treasurer")]
    public class CollectionTypesController : Controller
    {
        private readonly HttpClient _httpClient;

        public CollectionTypesController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: CollectionTypes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var types =
                await _httpClient.GetFromJsonAsync<
                    List<CollectionType>>(
                    "https://localhost:44360/api/CollectionType")
                ?? new List<CollectionType>();

            return View(types);
        }

        // GET: CollectionTypes/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CollectionTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CollectionType type)
        {
            if (!ModelState.IsValid)
            {
                return View(type);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/CollectionType",
                    type);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create collection type.");

                return View(type);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: CollectionTypes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var type =
                await _httpClient.GetFromJsonAsync<
                    CollectionType>(
                    $"https://localhost:44360/api/CollectionType/{id}");

            if (type == null)
            {
                return NotFound();
            }

            return View(type);
        }

        // POST: CollectionTypes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CollectionType type)
        {
            if (!ModelState.IsValid)
            {
                return View(type);
            }

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/CollectionType/{type.Id}",
                    type);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update collection type.");

                return View(type);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: CollectionTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/CollectionType/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to delete collection type.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}