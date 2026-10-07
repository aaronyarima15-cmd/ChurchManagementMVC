using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    public class ChurchCommitteeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ApiBaseUrl = "https://localhost:44360/";

        public ChurchCommitteeController(
            IHttpClientFactory httpClientFactory)
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

                var committee =
                    await client.GetFromJsonAsync<List<ChurchCommittee>>(
                        "api/ChurchCommittee");

                return View(
                    committee ?? new List<ChurchCommittee>());
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load church committee officials. " +
                    ex.Message;

                return View(
                    new List<ChurchCommittee>());
            }
        }

        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            return View(new ChurchCommittee());
        }

        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ChurchCommittee committee)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();

                return View(committee);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(ApiBaseUrl);

                var response =
                    await client.PostAsJsonAsync(
                        "api/ChurchCommittee",
                        new
                        {
                            committee.MemberId,
                            committee.PositionId
                        });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Church committee official added successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var errorMessage =
                    await GetErrorMessage(response);

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                await LoadDropdowns();

                return View(committee);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while adding the church committee official: " +
                    ex.Message);

                await LoadDropdowns();

                return View(committee);
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

                var committee =
                    await client.GetFromJsonAsync<ChurchCommittee>(
                        $"api/ChurchCommittee/{id}");

                if (committee == null)
                {
                    TempData["Error"] =
                        "Church committee official was not found.";

                    return RedirectToAction(nameof(Index));
                }

                await LoadDropdowns();

                return View(committee);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load the church committee official. " +
                    ex.Message;

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
            ChurchCommittee committee)
        {
            if (id != committee.Id)
            {
                TempData["Error"] =
                    "Invalid church committee ID.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();

                return View(committee);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(ApiBaseUrl);

                var response =
                    await client.PutAsJsonAsync(
                        $"api/ChurchCommittee/{id}",
                        new
                        {
                            committee.Id,
                            committee.MemberId,
                            committee.PositionId
                        });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Church committee official updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var errorMessage =
                    await GetErrorMessage(response);

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                await LoadDropdowns();

                return View(committee);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while updating the church committee official: " +
                    ex.Message);

                await LoadDropdowns();

                return View(committee);
            }
        }

        // =========================================================
        // DELETE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(ApiBaseUrl);

                var response =
                    await client.DeleteAsync(
                        $"api/ChurchCommittee/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Church committee official removed successfully.";
                }
                else
                {
                    TempData["Error"] =
                        await GetErrorMessage(response);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "An error occurred while removing the church committee official. " +
                    ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // LOAD DROPDOWNS
        // =========================================================
        private async Task LoadDropdowns()
        {
            var client = _httpClientFactory.CreateClient();

            client.BaseAddress = new Uri(ApiBaseUrl);

            // =====================================================
            // LOAD MEMBERS
            // =====================================================
            try
            {
                var response =
                    await client.GetAsync("api/Members");

                if (response.IsSuccessStatusCode)
                {
                    var members =
                        await response.Content
                            .ReadFromJsonAsync<
                                List<MembershipManagement>>();

                    ViewBag.Members =
                        members ??
                        new List<MembershipManagement>();
                }
                else
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ViewBag.Members =
                        new List<MembershipManagement>();

                    ViewBag.MemberLoadError =
                        $"Members API returned " +
                        $"{(int)response.StatusCode} " +
                        $"{response.StatusCode}. {error}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Members =
                    new List<MembershipManagement>();

                ViewBag.MemberLoadError =
                    "Could not connect to the Members API: " +
                    ex.Message;
            }

            // =====================================================
            // LOAD POSITIONS
            // =====================================================
            try
            {
                var response =
                    await client.GetAsync("api/Position");

                if (response.IsSuccessStatusCode)
                {
                    var positions =
                        await response.Content
                            .ReadFromJsonAsync<List<Position>>();

                    ViewBag.Positions =
                        positions ??
                        new List<Position>();
                }
                else
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ViewBag.Positions =
                        new List<Position>();

                    ViewBag.PositionLoadError =
                        $"Positions API returned " +
                        $"{(int)response.StatusCode} " +
                        $"{response.StatusCode}. {error}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Positions =
                    new List<Position>();

                ViewBag.PositionLoadError =
                    "Could not connect to the Positions API: " +
                    ex.Message;
            }
        }

        // =========================================================
        // GET API ERROR MESSAGE
        // =========================================================
        private async Task<string> GetErrorMessage(
            HttpResponseMessage response)
        {
            try
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<ApiError>();

                if (!string.IsNullOrWhiteSpace(
                    error?.Message))
                {
                    return error.Message;
                }
            }
            catch
            {
                // Ignore JSON parsing errors.
            }

            return
                $"The request could not be completed. " +
                $"Server returned status code " +
                $"{(int)response.StatusCode} " +
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