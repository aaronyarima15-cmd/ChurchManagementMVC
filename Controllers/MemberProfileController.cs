using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberProfileController : Controller
    {
        private readonly HttpClient _httpClient;

        public MemberProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // =========================================================
        // MY PROFILE
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get the email of the currently logged-in user.
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all church members from the existing API.
            var members =
                await _httpClient.GetFromJsonAsync<List<MembershipManagement>>(
                    "https://localhost:44360/api/Members")
                ?? new List<MembershipManagement>();

            // Find the church member whose email matches
            // the email of the logged-in Member account.
            var member = members.FirstOrDefault(m =>
                !string.IsNullOrWhiteSpace(m.Email) &&
                m.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (member == null)
            {
                return View("ProfileNotFound");
            }

            return View(member);
        }
    }
}