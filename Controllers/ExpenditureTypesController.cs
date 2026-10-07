using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Treasurer")]
    public class ExpenditureTypesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExpenditureTypesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                "https://localhost:44360/api/ExpenditureType");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load expenditure types.";
                return View(new List<ExpenditureType>());
            }

            var expenditureTypes =
                await response.Content.ReadFromJsonAsync<List<ExpenditureType>>();

            return View(expenditureTypes ?? new List<ExpenditureType>());
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
        public async Task<IActionResult> Create(ExpenditureType expenditureType)
        {
            if (!ModelState.IsValid)
            {
                return View(expenditureType);
            }

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "https://localhost:44360/api/ExpenditureType",
                expenditureType);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Expenditure type created successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] =
                "Unable to create expenditure type.";

            return View(expenditureType);
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"https://localhost:44360/api/ExpenditureType/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var expenditureType =
                await response.Content.ReadFromJsonAsync<ExpenditureType>();

            if (expenditureType == null)
            {
                return NotFound();
            }

            return View(expenditureType);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ExpenditureType expenditureType)
        {
            if (id != expenditureType.ExpenditureTypeId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(expenditureType);
            }

            var client = _httpClientFactory.CreateClient();

            var response = await client.PutAsJsonAsync(
                $"https://localhost:44360/api/ExpenditureType/{id}",
                expenditureType);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Expenditure type updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] =
                "Unable to update expenditure type.";

            return View(expenditureType);
        }

        // =========================
        // DELETE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"https://localhost:44360/api/ExpenditureType/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var expenditureType =
                await response.Content.ReadFromJsonAsync<ExpenditureType>();

            if (expenditureType == null)
            {
                return NotFound();
            }

            return View(expenditureType);
        }

        // =========================
        // DELETE - POST
        // =========================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.DeleteAsync(
                $"https://localhost:44360/api/ExpenditureType/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Expenditure type deleted successfully.";
            }
            else
            {
                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Unable to delete expenditure type."
                        : errorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}