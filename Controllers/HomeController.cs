using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();

            _httpClient.BaseAddress =
                new Uri("https://localhost:44360/");
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var events =
                await _httpClient.GetFromJsonAsync<List<EventManagement>>(
                    "api/Events")
                ?? new List<EventManagement>();

            var collections =
                await _httpClient.GetFromJsonAsync<List<ChurchCollectionManagement>>(
                    "api/Collections")
                ?? new List<ChurchCollectionManagement>();

            var peopleCount =
                await _httpClient.GetFromJsonAsync<int>(
                    "api/Members/count");

            var groupsCount =
                await _httpClient.GetFromJsonAsync<int>(
                    "api/Groups/count");

            var model = new DashboardViewModel
            {
                PeopleCount = peopleCount,
                GroupsCount = groupsCount,
                CollectionsCount = collections.Count,

                UpcomingEvents = events
                    .Where(e => e.EventDate >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}