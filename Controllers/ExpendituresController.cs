using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Treasurer")]
    public class ExpendituresController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ApiBaseUrl = "https://localhost:44360/api/";

        public ExpendituresController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================================================
        // INDEX
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"{ApiBaseUrl}Expenditure");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load expenditures.";

                return View(new List<ExpenditureManagement>());
            }

            var expenditures =
                await response.Content
                    .ReadFromJsonAsync<List<ExpenditureManagement>>();

            return View(
                expenditures ?? new List<ExpenditureManagement>());
        }

        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            return View(new ExpenditureManagement
            {
                ExpenditureDate = DateTime.Today,
                ReferenceNumber = GenerateReferenceNumber(),
                Status = "Pending"
            });
        }

        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ExpenditureManagement expenditure)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();

                return View(expenditure);
            }

            var client = _httpClientFactory.CreateClient();

            expenditure.CreatedBy =
                User.Identity?.Name;

            var response = await client.PostAsJsonAsync(
                $"{ApiBaseUrl}Expenditure",
                expenditure);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Expenditure recorded successfully.";

                return RedirectToAction(nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            TempData["Error"] =
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to record expenditure."
                    : errorMessage;

            await LoadDropdowns();

            return View(expenditure);
        }

        // =========================================================
        // EDIT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"{ApiBaseUrl}Expenditure/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var expenditure =
                await response.Content
                    .ReadFromJsonAsync<ExpenditureManagement>();

            if (expenditure == null)
            {
                return NotFound();
            }

            await LoadDropdowns();

            return View(expenditure);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ExpenditureManagement expenditure)
        {
            if (id != expenditure.ExpenditureId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();

                return View(expenditure);
            }

            var client = _httpClientFactory.CreateClient();

            var response = await client.PutAsJsonAsync(
                $"{ApiBaseUrl}Expenditure/{id}",
                expenditure);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Expenditure updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            TempData["Error"] =
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to update expenditure."
                    : errorMessage;

            await LoadDropdowns();

            return View(expenditure);
        }

        // =========================================================
        // DETAILS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(
                $"{ApiBaseUrl}Expenditure/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var expenditure =
                await response.Content
                    .ReadFromJsonAsync<ExpenditureManagement>();

            if (expenditure == null)
            {
                return NotFound();
            }

            return View(expenditure);
        }

        // =========================================================
        // DELETE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.DeleteAsync(
                $"{ApiBaseUrl}Expenditure/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Expenditure deleted successfully.";
            }
            else
            {
                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Unable to delete expenditure."
                        : errorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GENERATE REFERENCE NUMBER
        // =========================================================
        private string GenerateReferenceNumber()
        {
            int randomNumber = RandomNumberGenerator.GetInt32(
                1000000000,
                int.MaxValue);

            return randomNumber.ToString();
        }

        // =========================================================
        // LOAD ALL DROPDOWNS
        // =========================================================
        private async Task LoadDropdowns()
        {
            var client = _httpClientFactory.CreateClient();

            // -----------------------------------------------------
            // Expenditure Types
            // -----------------------------------------------------
            var expenditureTypeResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}ExpenditureType");

            if (expenditureTypeResponse.IsSuccessStatusCode)
            {
                var expenditureTypes =
                    await expenditureTypeResponse
                        .Content
                        .ReadFromJsonAsync<List<ExpenditureType>>();

                ViewBag.ExpenditureTypes =
                    expenditureTypes
                    ?? new List<ExpenditureType>();
            }
            else
            {
                ViewBag.ExpenditureTypes =
                    new List<ExpenditureType>();
            }

            // -----------------------------------------------------
            // Payment Methods
            // -----------------------------------------------------
            var paymentMethodResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}PaymentMethod");

            if (paymentMethodResponse.IsSuccessStatusCode)
            {
                var paymentMethods =
                    await paymentMethodResponse
                        .Content
                        .ReadFromJsonAsync<List<PaymentMethod>>();

                ViewBag.PaymentMethods =
                    paymentMethods
                    ?? new List<PaymentMethod>();
            }
            else
            {
                ViewBag.PaymentMethods =
                    new List<PaymentMethod>();
            }

            // -----------------------------------------------------
            // Church Committee Officials
            // -----------------------------------------------------
            var churchOfficialsResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}ChurchCommittee");

            if (churchOfficialsResponse.IsSuccessStatusCode)
            {
                var churchOfficials =
                    await churchOfficialsResponse
                        .Content
                        .ReadFromJsonAsync<List<ChurchCommittee>>();

                ViewBag.ChurchOfficials =
                    churchOfficials
                    ?? new List<ChurchCommittee>();
            }
            else
            {
                ViewBag.ChurchOfficials =
                    new List<ChurchCommittee>();
            }
        }
    }
}