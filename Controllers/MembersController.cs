using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Pastor,Secretary")]
    public class MembersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public MembersController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _httpClient = httpClientFactory.CreateClient();
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var members =
                await _httpClient.GetFromJsonAsync<List<MembershipManagement>>(
                    "https://localhost:44360/api/Members")
                ?? new List<MembershipManagement>();

            return View(members);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var members = await _httpClient.GetFromJsonAsync<List<MembershipManagement>>(
                "https://localhost:44360/api/Members") ?? new List<MembershipManagement>();

            var model = new PdfExportVM
            {
                ReportTitle = "Church Members",
                Sections = new List<PdfExportSectionVM>
                {
                    new PdfExportSectionVM
                    {
                        Headers = new List<string> { "Name", "Phone", "Email", "Gender", "Card Number", "Baptized" },
                        Rows = members.OrderBy(m => m.FirstName).ThenBy(m => m.LastName)
                            .Select(m => new List<string>
                            {
                                $"{m.FirstName} {m.LastName} {m.OtherNames}".Trim(), m.PhoneNumber,
                                m.Email ?? string.Empty, m.Gender ?? string.Empty,
                                m.CardNumber ?? string.Empty, m.IsBaptized ? "Yes" : "No"
                            }).ToList()
                    }
                }
            };

            return View("~/Views/Shared/ExportPreview.cshtml", model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MembershipManagement member,
            IFormFile? photo)
        {
            if (photo != null &&
                photo.Length > 0)
            {
                var photoPath =
                    await SaveMemberPhotoAsync(photo);

                if (photoPath == null)
                {
                    ModelState.AddModelError(
                        "Photo",
                        "Please upload a valid image file (JPG, JPEG, PNG, or WEBP).");
                }
                else
                {
                    member.PhotoPath =
                        photoPath;
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    member.RelationshipId,
                    member.StatusId,
                    member.StateId,
                    member.LgaId);

                return View(member);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/Members",
                    member);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(
                    nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            // If the API failed, remove the uploaded photo
            // because the member was not saved.
            if (!string.IsNullOrWhiteSpace(
                member.PhotoPath))
            {
                DeleteMemberPhoto(
                    member.PhotoPath);
            }

            ModelState.AddModelError(
                string.Empty,
                "Unable to create member. API response: "
                + errorMessage);

            await LoadDropdownsAsync(
                member.RelationshipId,
                member.StatusId,
                member.StateId,
                member.LgaId);

            return View(member);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var response =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Members/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var member =
                await response.Content
                    .ReadFromJsonAsync<MembershipManagement>();

            if (member == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync(
                member.RelationshipId,
                member.StatusId,
                member.StateId,
                member.LgaId);

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MembershipManagement member,
            IFormFile? photo)
        {
            var currentResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Members/{member.Id}");

            if (!currentResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var currentMember =
                await currentResponse.Content
                    .ReadFromJsonAsync<MembershipManagement>();

            if (currentMember == null)
            {
                return NotFound();
            }

            // Keep the existing photo by default.
            member.PhotoPath =
                currentMember.PhotoPath;

            string? newPhotoPath = null;

            if (photo != null &&
                photo.Length > 0)
            {
                newPhotoPath =
                    await SaveMemberPhotoAsync(photo);

                if (newPhotoPath == null)
                {
                    ModelState.AddModelError(
                        "Photo",
                        "Please upload a valid image file (JPG, JPEG, PNG, or WEBP).");
                }
                else
                {
                    member.PhotoPath =
                        newPhotoPath;
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    member.RelationshipId,
                    member.StatusId,
                    member.StateId,
                    member.LgaId);

                return View(member);
            }

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/Members/{member.Id}",
                    member);

            if (response.IsSuccessStatusCode)
            {
                // If a new photo was uploaded successfully,
                // remove the old photo from wwwroot.
                if (!string.IsNullOrWhiteSpace(
                        newPhotoPath) &&
                    !string.IsNullOrWhiteSpace(
                        currentMember.PhotoPath))
                {
                    DeleteMemberPhoto(
                        currentMember.PhotoPath);
                }

                return RedirectToAction(
                    nameof(Index));
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            // The new photo was uploaded but the API failed.
            // Delete the new photo so we don't leave an unused file.
            if (!string.IsNullOrWhiteSpace(
                    newPhotoPath))
            {
                DeleteMemberPhoto(
                    newPhotoPath);
            }

            ModelState.AddModelError(
                string.Empty,
                "Unable to update member. API response: "
                + errorMessage);

            // Restore the old photo path for the view.
            member.PhotoPath =
                currentMember.PhotoPath;

            await LoadDropdownsAsync(
                member.RelationshipId,
                member.StatusId,
                member.StateId,
                member.LgaId);

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            // Get the member first so we know the photo path.
            var memberResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Members/{id}");

            MembershipManagement? member =
                null;

            if (memberResponse.IsSuccessStatusCode)
            {
                member =
                    await memberResponse.Content
                        .ReadFromJsonAsync<MembershipManagement>();
            }

            // Delete member from API.
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/Members/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Delete the member photo from wwwroot.
                if (member != null &&
                    !string.IsNullOrWhiteSpace(
                        member.PhotoPath))
                {
                    DeleteMemberPhoto(
                        member.PhotoPath);
                }

                return RedirectToAction(
                    nameof(Index));
            }

            var errorMessage =
                await response.Content
                    .ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                "Unable to delete member. API response: "
                + errorMessage);

            return RedirectToAction(
                nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetLgasByState(
            int stateId)
        {
            if (stateId <= 0)
            {
                return Json(
                    new List<Lga>());
            }

            var lgas =
                await _httpClient.GetFromJsonAsync<List<Lga>>(
                    $"https://localhost:44360/api/Lga/state/{stateId}")
                ?? new List<Lga>();

            return Json(lgas);
        }

        private async Task LoadDropdownsAsync(
            int? selectedRelationshipId = null,
            int? selectedStatusId = null,
            int? selectedStateId = null,
            int? selectedLgaId = null)
        {
            var relationships =
                await _httpClient.GetFromJsonAsync<List<Relationship>>(
                    "https://localhost:44360/api/Relationship")
                ?? new List<Relationship>();

            var statuses =
                await _httpClient.GetFromJsonAsync<List<MembershipStatus>>(
                    "https://localhost:44360/api/MembershipStatus")
                ?? new List<MembershipStatus>();

            var states =
                await _httpClient.GetFromJsonAsync<List<State>>(
                    "https://localhost:44360/api/State")
                ?? new List<State>();

            var lgas =
                new List<Lga>();

            if (selectedStateId.HasValue &&
                selectedStateId.Value > 0)
            {
                lgas =
                    await _httpClient.GetFromJsonAsync<List<Lga>>(
                        $"https://localhost:44360/api/Lga/state/{selectedStateId.Value}")
                    ?? new List<Lga>();
            }

            ViewBag.Relationships =
                new SelectList(
                    relationships,
                    "Id",
                    "Name",
                    selectedRelationshipId);

            ViewBag.Statuses =
                new SelectList(
                    statuses,
                    "Id",
                    "StatusName",
                    selectedStatusId);

            ViewBag.States =
                states;

            ViewBag.Lgas =
                lgas;

            ViewBag.SelectedRelationshipId =
                selectedRelationshipId;

            ViewBag.SelectedStatusId =
                selectedStatusId;

            ViewBag.SelectedStateId =
                selectedStateId;

            ViewBag.SelectedLgaId =
                selectedLgaId;
        }

        private async Task<string?> SaveMemberPhotoAsync(
            IFormFile photo)
        {
            // Allowed image extensions.
            var allowedExtensions =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

            var extension =
                Path.GetExtension(
                    photo.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(
                extension))
            {
                return null;
            }

            // Maximum file size: 5 MB.
            const long maxFileSize =
                5 * 1024 * 1024;

            if (photo.Length > maxFileSize)
            {
                return null;
            }

            // Create:
            // wwwroot/uploads/members
            var uploadFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "members");

            if (!Directory.Exists(
                uploadFolder))
            {
                Directory.CreateDirectory(
                    uploadFolder);
            }

            // Generate a unique filename.
            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath =
                Path.Combine(
                    uploadFolder,
                    fileName);

            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await photo.CopyToAsync(
                    stream);
            }

            // This is what will be saved
            // in the database.
            return
                $"/uploads/members/{fileName}";
        }

        private void DeleteMemberPhoto(
            string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(
                photoPath))
            {
                return;
            }

            var cleanPath =
                photoPath.TrimStart(
                    '/',
                    '\\');

            var filePath =
                Path.Combine(
                    _environment.WebRootPath,
                    cleanPath.Replace(
                        '/',
                        Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(
                filePath))
            {
                System.IO.File.Delete(
                    filePath);
            }
        }
    }
}