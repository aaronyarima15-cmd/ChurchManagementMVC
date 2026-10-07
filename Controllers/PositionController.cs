using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    public class PositionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ApiBaseUrl = "https://localhost:44360/";

        public PositionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                client.BaseAddress = new Uri(ApiBaseUrl);

                var positions = await client.GetFromJsonAsync<List<Position>>(
                    "api/Position");

                return View(positions ?? new List<Position>());
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load positions. " + ex.Message;

                return View(new List<Position>());
            }
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Position());
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Position position)
        {
            if (!ModelState.IsValid)
            {
                return View(position);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                client.BaseAddress = new Uri(ApiBaseUrl);

                var response = await client.PostAsJsonAsync(
                    "api/Position",
                    new
                    {
                        position.PositionName
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Position added successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var errorMessage = await GetErrorMessage(response);

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(position);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while adding the position. "
                    + ex.Message);

                return View(position);
            }
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                client.BaseAddress = new Uri(ApiBaseUrl);

                var position = await client.GetFromJsonAsync<Position>(
                    $"api/Position/{id}");

                if (position == null)
                {
                    TempData["Error"] =
                        "Position was not found.";

                    return RedirectToAction(nameof(Index));
                }

                return View(position);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load the position. " + ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Position position)
        {
            if (id != position.Id)
            {
                TempData["Error"] =
                    "Invalid position ID.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(position);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                client.BaseAddress = new Uri(ApiBaseUrl);

                var response = await client.PutAsJsonAsync(
                    $"api/Position/{id}",
                    new
                    {
                        position.Id,
                        position.PositionName
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Position updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var errorMessage = await GetErrorMessage(response);

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(position);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while updating the position. "
                    + ex.Message);

                return View(position);
            }
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                client.BaseAddress = new Uri(ApiBaseUrl);

                var response = await client.DeleteAsync(
                    $"api/Position/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Position deleted successfully.";
                }
                else
                {
                    var errorMessage = await GetErrorMessage(response);

                    TempData["Error"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "An error occurred while deleting the position. "
                    + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // ERROR MESSAGE
        // =========================================================

        private async Task<string> GetErrorMessage(
            HttpResponseMessage response)
        {
            try
            {
                var error =
                    await response.Content.ReadFromJsonAsync<ApiError>();

                if (!string.IsNullOrWhiteSpace(error?.Message))
                {
                    return error.Message;
                }
            }
            catch
            {
                // Ignore JSON parsing errors.
            }

            return $"The request could not be completed. " +
                   $"Server returned status code {(int)response.StatusCode} " +
                   $"({response.StatusCode}).";
        }


        // =========================================================
        // API ERROR MODEL
        // =========================================================

        private class ApiError
        {
            public string? Message { get; set; }
        }
    }
}