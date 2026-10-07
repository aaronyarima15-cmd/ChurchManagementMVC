using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Treasurer")]
    public class PaymentMethodsController : Controller
    {
        private readonly HttpClient _httpClient;

        public PaymentMethodsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: PaymentMethods
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var methods =
                await _httpClient.GetFromJsonAsync<
                    List<PaymentMethod>>(
                    "https://localhost:44360/api/PaymentMethod")
                ?? new List<PaymentMethod>();

            return View(methods);
        }

        // GET: PaymentMethods/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentMethods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PaymentMethod method)
        {
            if (!ModelState.IsValid)
            {
                return View(method);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/PaymentMethod",
                    method);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create payment method.");

                return View(method);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: PaymentMethods/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var method =
                await _httpClient.GetFromJsonAsync<
                    PaymentMethod>(
                    $"https://localhost:44360/api/PaymentMethod/{id}");

            if (method == null)
            {
                return NotFound();
            }

            return View(method);
        }

        // POST: PaymentMethods/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PaymentMethod method)
        {
            if (!ModelState.IsValid)
            {
                return View(method);
            }

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/PaymentMethod/{method.Id}",
                    method);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update payment method.");

                return View(method);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PaymentMethods/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/PaymentMethod/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to delete payment method.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}