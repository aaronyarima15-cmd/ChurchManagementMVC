using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Treasurer,Pastor,Secretary")]
    public class CollectionsController : Controller
    {
        private readonly HttpClient _httpClient;

        public CollectionsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }


        // =========================
        // COLLECTIONS LIST
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collections =
                await _httpClient.GetFromJsonAsync<
                    List<ChurchCollectionManagement>>(
                    "https://localhost:44360/api/Collections")
                ?? new List<ChurchCollectionManagement>();

            return View(collections);
        }


        // =========================
        // GROUP COLLECTIONS
        // =========================

        [HttpGet]
        public async Task<IActionResult> GroupCollections(int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            // Get group information
            var groupResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupId}");

            if (!groupResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var group =
                await groupResponse.Content
                    .ReadFromJsonAsync<GroupManagement>();

            if (group == null)
            {
                return NotFound();
            }

            // Get collections for this group
            var collections =
                await _httpClient.GetFromJsonAsync<
                    List<ChurchCollectionManagement>>(
                    $"https://localhost:44360/api/Collections/group/{groupId}")
                ?? new List<ChurchCollectionManagement>();

            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            return View(collections);
        }


        // =========================
        // GROUP MONTHLY DUES
        // =========================

        [HttpGet]
        public async Task<IActionResult> GroupMonthlyDues(
            int groupId,
            int? year)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            var selectedYear = year ?? DateTime.Today.Year;

            var groupResponse = await _httpClient.GetAsync(
                $"https://localhost:44360/api/Groups/{groupId}");

            if (!groupResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var group = await groupResponse.Content
                .ReadFromJsonAsync<GroupManagement>();

            if (group == null)
            {
                return NotFound();
            }

            var groupMembers = await _httpClient.GetFromJsonAsync<List<GroupMember>>(
                $"https://localhost:44360/api/GroupMembers/group/{groupId}")
                ?? new List<GroupMember>();

            var duesResponse = await _httpClient.GetAsync(
                $"https://localhost:44360/api/GroupMonthlyDues/group/{groupId}/{selectedYear}");

            var savedPayments = duesResponse.IsSuccessStatusCode
                ? await duesResponse.Content
                    .ReadFromJsonAsync<List<GroupMonthlyDuePayment>>()
                    ?? new List<GroupMonthlyDuePayment>()
                : new List<GroupMonthlyDuePayment>();

            if (!duesResponse.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load saved monthly dues. Make sure the API database migration has been applied.";
            }

            var model = new GroupMonthlyDuesVM
            {
                GroupId = groupId,
                GroupName = group.GroupName,
                Year = selectedYear,
                Members = groupMembers
                    .Where(gm => gm.Member != null)
                    .Select(gm => new GroupMonthlyDuesMemberVM
                    {
                        MemberId = gm.MemberId,
                        MemberName =
                            $"{gm.Member!.FirstName} {gm.Member.LastName} {gm.Member.OtherNames}".Trim()
                    })
                    .ToList()
            };

            foreach (var member in model.Members)
            {
                foreach (var payment in savedPayments.Where(p =>
                    p.MemberId == member.MemberId &&
                    p.Month >= 1 && p.Month <= 12))
                {
                    member.MonthlyAmounts[payment.Month - 1] = payment.Amount;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroupMonthlyDues(
            GroupMonthlyDuesVM model)
        {
            if (model.GroupId <= 0 || model.Year < 1900 || model.Year > 9999)
            {
                return BadRequest("A valid group and year are required.");
            }

            var payments = model.Members
                .SelectMany(member => Enumerable.Range(1, 12)
                    .Select(month => new GroupMonthlyDuePayment
                    {
                        GroupId = model.GroupId,
                        Year = model.Year,
                        MemberId = member.MemberId,
                        Month = month,
                        Amount = member.MonthlyAmounts != null &&
                                 member.MonthlyAmounts.Length >= month
                            ? member.MonthlyAmounts[month - 1] ?? 0m
                            : 0m
                    }))
                .ToList();

            var response = await _httpClient.PostAsJsonAsync(
                $"https://localhost:44360/api/GroupMonthlyDues/group/{model.GroupId}/{model.Year}",
                payments);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["Error"] =
                    "Unable to save monthly dues. " + errorMessage;
            }
            else
            {
                TempData["Success"] =
                    "Monthly member payments saved successfully.";
            }

            return RedirectToAction(
                nameof(GroupMonthlyDues),
                new { groupId = model.GroupId, year = model.Year });
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();

            return View();
        }


        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ChurchCollectionManagement collection)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    collection.CollectionTypeId,
                    collection.PaymentMethodId);

                return View(collection);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/Collections",
                    collection);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create the collection. " +
                    errorMessage);

                await LoadDropdownsAsync(
                    collection.CollectionTypeId,
                    collection.PaymentMethodId);

                return View(collection);
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // CREATE GROUP COLLECTION - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> CreateGroupCollection(
            int groupId)
        {
            if (groupId <= 0)
            {
                return NotFound();
            }

            var groupResponse =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupId}");

            if (!groupResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var group =
                await groupResponse.Content
                    .ReadFromJsonAsync<GroupManagement>();

            if (group == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync();

            ViewBag.Group = group;
            ViewBag.GroupId = groupId;

            var collection =
                new ChurchCollectionManagement
                {
                    GroupId = groupId,
                    CollectionDate = DateTime.Today
                };

            return View(collection);
        }


        // =========================
        // CREATE GROUP COLLECTION - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupCollection(
            ChurchCollectionManagement collection)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    collection.CollectionTypeId,
                    collection.PaymentMethodId);

                await LoadGroupAsync(collection.GroupId);

                return View(collection);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:44360/api/Collections",
                    collection);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Group collection recorded successfully.";

                return RedirectToAction(
                    nameof(GroupCollections),
                    new { groupId = collection.GroupId });
            }

            var errorMessage =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                "Unable to create the group collection. " +
                errorMessage);

            await LoadDropdownsAsync(
                collection.CollectionTypeId,
                collection.PaymentMethodId);

            await LoadGroupAsync(collection.GroupId);

            return View(collection);
        }


        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var collection =
                await _httpClient.GetFromJsonAsync<
                    ChurchCollectionManagement>(
                    $"https://localhost:44360/api/Collections/{id}");

            if (collection == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync(
                collection.CollectionTypeId,
                collection.PaymentMethodId);

            return View(collection);
        }


        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ChurchCollectionManagement collection)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(
                    collection.CollectionTypeId,
                    collection.PaymentMethodId);

                return View(collection);
            }

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:44360/api/Collections/{collection.Id}",
                    collection);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update the collection. " +
                    errorMessage);

                await LoadDropdownsAsync(
                    collection.CollectionTypeId,
                    collection.PaymentMethodId);

                return View(collection);
            }

            // If it was a group collection,
            // return to that group's collections.
            if (collection.GroupId.HasValue)
            {
                return RedirectToAction(
                    nameof(GroupCollections),
                    new { groupId = collection.GroupId.Value });
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // DELETE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"https://localhost:44360/api/Collections/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to delete the collection.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // LOAD DROPDOWNS
        // =========================

        private async Task LoadDropdownsAsync(
            int? selectedCollectionTypeId = null,
            int? selectedPaymentMethodId = null)
        {
            var collectionTypes =
                await _httpClient.GetFromJsonAsync<
                    List<CollectionType>>(
                    "https://localhost:44360/api/CollectionType")
                ?? new List<CollectionType>();

            var paymentMethods =
                await _httpClient.GetFromJsonAsync<
                    List<PaymentMethod>>(
                    "https://localhost:44360/api/PaymentMethod")
                ?? new List<PaymentMethod>();

            ViewBag.CollectionTypes = collectionTypes;

            ViewBag.PaymentMethods = paymentMethods;

            ViewBag.SelectedCollectionTypeId =
                selectedCollectionTypeId;

            ViewBag.SelectedPaymentMethodId =
                selectedPaymentMethodId;
        }


        // =========================
        // LOAD GROUP
        // =========================

        private async Task LoadGroupAsync(int? groupId)
        {
            if (!groupId.HasValue ||
                groupId.Value <= 0)
            {
                ViewBag.Group = null;
                ViewBag.GroupId = null;
                return;
            }

            var response =
                await _httpClient.GetAsync(
                    $"https://localhost:44360/api/Groups/{groupId.Value}");

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Group =
                    await response.Content
                        .ReadFromJsonAsync<GroupManagement>();
            }

            ViewBag.GroupId = groupId.Value;
        }
    }
}