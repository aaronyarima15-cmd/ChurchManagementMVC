using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [AllowAnonymous]
    [Route("Registration")]
    public class MemberRegistrationController : Controller
    {
        private const string ApiUrl = "https://localhost:44360/api/";
        private readonly HttpClient _http;
        private readonly IWebHostEnvironment _environment;

        public MemberRegistrationController(IHttpClientFactory factory, IWebHostEnvironment environment)
        {
            _http = factory.CreateClient();
            _environment = environment;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            await LoadDropdowns();
            return View(new MembershipManagement());
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Index(MembershipManagement member, IFormFile? photo)
        {
            if (string.IsNullOrWhiteSpace(member.FirstName)) ModelState.AddModelError(nameof(member.FirstName), "Please enter your name.");
            if (string.IsNullOrWhiteSpace(member.FatherName)) ModelState.AddModelError(nameof(member.FatherName), "Please enter your father\'s name.");
            if (string.IsNullOrWhiteSpace(member.PhoneNumber)) ModelState.AddModelError(nameof(member.PhoneNumber), "Please enter a phone number.");
            if (member.DateOfBirth == default || member.DateOfBirth.Date > DateTime.Today) ModelState.AddModelError(nameof(member.DateOfBirth), "Please enter a valid date of birth.");
            if (member.RelationshipId <= 0) ModelState.AddModelError(nameof(member.RelationshipId), "Please select your relationship.");
            if (member.StatusId <= 0) ModelState.AddModelError(nameof(member.StatusId), "Please select a status.");
            if (member.IsBaptized && string.IsNullOrWhiteSpace(member.CardNumber)) ModelState.AddModelError(nameof(member.CardNumber), "Card number is required if you are baptized.");

            member.LastName = member.FatherName?.Trim() ?? string.Empty;
            string? savedPhoto = null;
            if (photo != null && photo.Length > 0)
            {
                savedPhoto = await SavePhoto(photo);
                if (savedPhoto == null) ModelState.AddModelError("Photo", "Choose a JPG, PNG, or WEBP image under 5 MB.");
                else member.PhotoPath = savedPhoto;
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(member.RelationshipId, member.StatusId, member.StateId, member.LgaId);
                return View(member);
            }

            var response = await _http.PostAsJsonAsync(ApiUrl + "Members", member);
            if (response.IsSuccessStatusCode)
            {
                return View("Submitted");
            }

            if (savedPhoto != null) DeletePhoto(savedPhoto);
            ModelState.AddModelError(string.Empty, "We could not save your information. Please check the details and try again.");
            await LoadDropdowns(member.RelationshipId, member.StatusId, member.StateId, member.LgaId);
            return View(member);
        }

        [HttpGet("lgas/{stateId:int}")]
        public async Task<IActionResult> Lgas(int stateId)
        {
            var items = await _http.GetFromJsonAsync<List<Lga>>(ApiUrl + $"Lga/state/{stateId}") ?? new();
            return Json(items.Select(x => new { id = x.LgaId, name = x.Name }));
        }

        private async Task LoadDropdowns(int? relationship = null, int? status = null, int? state = null, int? lga = null)
        {
            var relationships = await _http.GetFromJsonAsync<List<Relationship>>(ApiUrl + "Relationship") ?? new();
            var statuses = await _http.GetFromJsonAsync<List<MembershipStatus>>(ApiUrl + "MembershipStatus") ?? new();
            var states = await _http.GetFromJsonAsync<List<State>>(ApiUrl + "State") ?? new();
            var lgas = state > 0 ? await _http.GetFromJsonAsync<List<Lga>>(ApiUrl + $"Lga/state/{state}") ?? new() : new List<Lga>();
            ViewBag.Relationships = new SelectList(relationships, "Id", "Name", relationship);
            ViewBag.Statuses = new SelectList(statuses, "Id", "StatusName", status);
            ViewBag.States = new SelectList(states, "StateId", "Name", state);
            ViewBag.Lgas = new SelectList(lgas, "LgaId", "Name", lga);
        }

        private async Task<string?> SavePhoto(IFormFile photo)
        {
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (photo.Length > 5 * 1024 * 1024 || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext)) return null;
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "members");
            Directory.CreateDirectory(folder);
            var name = Guid.NewGuid().ToString("N") + ext;
            using var stream = System.IO.File.Create(Path.Combine(folder, name));
            await photo.CopyToAsync(stream);
            return "/uploads/members/" + name;
        }

        private void DeletePhoto(string path)
        {
            var full = Path.Combine(_environment.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
        }
    }
}
