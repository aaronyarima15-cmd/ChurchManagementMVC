using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin,Treasurer")]
    public class ReportsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ApiBaseUrl =
            "https://localhost:44360/api/";

        public ReportsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // =========================================================
        // REPORTS HOME
        // =========================================================
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =========================================================
        // INCOME REPORT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Income(
            DateTime? startDate,
            DateTime? endDate,
            int? collectionTypeId)
        {
            var client = _httpClientFactory.CreateClient();

            var collectionsResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}Collections");

            var collectionTypesResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}CollectionType");

            var collections =
                new List<ChurchCollectionManagement>();

            var collectionTypes =
                new List<CollectionType>();

            if (collectionsResponse.IsSuccessStatusCode)
            {
                collections =
                    await collectionsResponse.Content
                        .ReadFromJsonAsync<
                            List<ChurchCollectionManagement>>()
                    ?? new List<ChurchCollectionManagement>();
            }

            if (collectionTypesResponse.IsSuccessStatusCode)
            {
                collectionTypes =
                    await collectionTypesResponse.Content
                        .ReadFromJsonAsync<
                            List<CollectionType>>()
                    ?? new List<CollectionType>();
            }

            IEnumerable<ChurchCollectionManagement> filtered =
                collections;

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;

                filtered = filtered.Where(
                    x => x.CollectionDate.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;

                filtered = filtered.Where(
                    x => x.CollectionDate.Date <= end);
            }

            if (collectionTypeId.HasValue)
            {
                filtered = filtered.Where(
                    x => x.CollectionTypeId ==
                         collectionTypeId.Value);
            }

            var results =
                filtered
                    .OrderByDescending(x => x.CollectionDate)
                    .ToList();

            var model = new IncomeReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                CollectionTypeId = collectionTypeId,
                CollectionTypes = collectionTypes,
                Results = results,
                TotalAmount = results.Sum(x => x.Amount),

                HasSearched =
                    startDate.HasValue ||
                    endDate.HasValue ||
                    collectionTypeId.HasValue
            };

            return View(model);
        }

        // =========================================================
        // EXPENSE REPORT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Expense(
            DateTime? startDate,
            DateTime? endDate,
            int? expenditureTypeId,
            string? payeeName)
        {
            var client =
                _httpClientFactory.CreateClient();

            var expendituresResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}Expenditure");

            var expenditureTypesResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}ExpenditureType");

            var expenditures =
                new List<ExpenditureManagement>();

            var expenditureTypes =
                new List<ExpenditureType>();

            if (expendituresResponse.IsSuccessStatusCode)
            {
                expenditures =
                    await expendituresResponse.Content
                        .ReadFromJsonAsync<
                            List<ExpenditureManagement>>()
                    ?? new List<ExpenditureManagement>();
            }

            if (expenditureTypesResponse.IsSuccessStatusCode)
            {
                expenditureTypes =
                    await expenditureTypesResponse.Content
                        .ReadFromJsonAsync<
                            List<ExpenditureType>>()
                    ?? new List<ExpenditureType>();
            }

            IEnumerable<ExpenditureManagement> filtered =
                expenditures;

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;

                filtered = filtered.Where(
                    x => x.ExpenditureDate.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;

                filtered = filtered.Where(
                    x => x.ExpenditureDate.Date <= end);
            }

            if (expenditureTypeId.HasValue)
            {
                filtered = filtered.Where(
                    x => x.ExpenditureTypeId ==
                         expenditureTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(payeeName))
            {
                var search = payeeName.Trim();

                filtered = filtered.Where(
                    x =>
                        !string.IsNullOrWhiteSpace(x.PayeeName) &&
                        x.PayeeName.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase));
            }

            var results =
                filtered
                    .OrderByDescending(x => x.ExpenditureDate)
                    .ToList();

            var model = new ExpenseReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                ExpenditureTypeId = expenditureTypeId,
                PayeeName = payeeName,
                ExpenditureTypes = expenditureTypes,
                Results = results,
                TotalAmount = results.Sum(x => x.Amount),

                HasSearched =
                    startDate.HasValue ||
                    endDate.HasValue ||
                    expenditureTypeId.HasValue ||
                    !string.IsNullOrWhiteSpace(payeeName)
            };

            return View(model);
        }

        // =========================================================
        // EXPORT INCOME REPORT
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportIncome(
            DateTime? startDate,
            DateTime? endDate,
            int? collectionTypeId)
        {
            var client =
                _httpClientFactory.CreateClient();

            var collectionsResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}Collections");

            var collectionTypesResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}CollectionType");

            var collections =
                new List<ChurchCollectionManagement>();

            var collectionTypes =
                new List<CollectionType>();

            if (collectionsResponse.IsSuccessStatusCode)
            {
                collections =
                    await collectionsResponse.Content
                        .ReadFromJsonAsync<
                            List<ChurchCollectionManagement>>()
                    ?? new List<ChurchCollectionManagement>();
            }

            if (collectionTypesResponse.IsSuccessStatusCode)
            {
                collectionTypes =
                    await collectionTypesResponse.Content
                        .ReadFromJsonAsync<
                            List<CollectionType>>()
                    ?? new List<CollectionType>();
            }

            IEnumerable<ChurchCollectionManagement> filtered =
                collections;

            if (startDate.HasValue)
            {
                filtered = filtered.Where(
                    x => x.CollectionDate.Date >=
                         startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                filtered = filtered.Where(
                    x => x.CollectionDate.Date <=
                         endDate.Value.Date);
            }

            if (collectionTypeId.HasValue)
            {
                filtered = filtered.Where(
                    x => x.CollectionTypeId ==
                         collectionTypeId.Value);
            }

            var results =
                filtered
                    .OrderByDescending(x => x.CollectionDate)
                    .ToList();

            return ExportIncomeExcel(results, collectionTypes);
        }

        // =========================================================
        // EXPORT EXPENSE REPORT
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportExpense(
            DateTime? startDate,
            DateTime? endDate,
            int? expenditureTypeId,
            string? payeeName)
        {
            var client =
                _httpClientFactory.CreateClient();

            var expendituresResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}Expenditure");

            var expenditureTypesResponse =
                await client.GetAsync(
                    $"{ApiBaseUrl}ExpenditureType");

            var expenditures =
                new List<ExpenditureManagement>();

            var expenditureTypes =
                new List<ExpenditureType>();

            if (expendituresResponse.IsSuccessStatusCode)
            {
                expenditures =
                    await expendituresResponse.Content
                        .ReadFromJsonAsync<
                            List<ExpenditureManagement>>()
                    ?? new List<ExpenditureManagement>();
            }

            if (expenditureTypesResponse.IsSuccessStatusCode)
            {
                expenditureTypes =
                    await expenditureTypesResponse.Content
                        .ReadFromJsonAsync<
                            List<ExpenditureType>>()
                    ?? new List<ExpenditureType>();
            }

            IEnumerable<ExpenditureManagement> filtered =
                expenditures;

            if (startDate.HasValue)
            {
                filtered = filtered.Where(
                    x => x.ExpenditureDate.Date >=
                         startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                filtered = filtered.Where(
                    x => x.ExpenditureDate.Date <=
                         endDate.Value.Date);
            }

            if (expenditureTypeId.HasValue)
            {
                filtered = filtered.Where(
                    x => x.ExpenditureTypeId ==
                         expenditureTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(payeeName))
            {
                var search = payeeName.Trim();

                filtered = filtered.Where(
                    x =>
                        !string.IsNullOrWhiteSpace(x.PayeeName) &&
                        x.PayeeName.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase));
            }

            var results =
                filtered
                    .OrderByDescending(x => x.ExpenditureDate)
                    .ToList();

            return ExportExpenseExcel(results, expenditureTypes);
        }

        // =========================================================
        // EXCEL - INCOME
        // =========================================================
        private IActionResult ExportIncomeExcel(
            IEnumerable<ChurchCollectionManagement> data,
            IEnumerable<CollectionType> collectionTypes)
        {
            // EPPlus 8+
            ExcelPackage.License.SetNonCommercialPersonal(
                "Church Management System");

            using var package =
                new ExcelPackage();

            var worksheet =
                package.Workbook.Worksheets.Add(
                    "Income Report");

            // -----------------------------------------------------
            // TITLE
            // -----------------------------------------------------
            worksheet.Cells["A1"].Value =
                "CHURCH INCOME REPORT";

            worksheet.Cells["A1:E1"].Merge = true;

            worksheet.Cells["A1"].Style.Font.Bold =
                true;

            worksheet.Cells["A1"].Style.Font.Size =
                16;

            worksheet.Cells["A1:E1"].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            // -----------------------------------------------------
            // HEADERS
            // -----------------------------------------------------
            worksheet.Cells[3, 1].Value =
                "Date";

            worksheet.Cells[3, 2].Value =
                "Collection Type";

            worksheet.Cells[3, 3].Value =
                "Payment Method";

            worksheet.Cells[3, 4].Value =
                "Notes";

            worksheet.Cells[3, 5].Value =
                "Amount";

            using (var header =
                   worksheet.Cells["A3:E3"])
            {
                header.Style.Font.Bold = true;

                header.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
            }

            // -----------------------------------------------------
            // DATA
            // -----------------------------------------------------
            int row = 4;

            foreach (var income in data)
            {
                var collectionType =
                    collectionTypes.FirstOrDefault(
                        x => x.Id ==
                             income.CollectionTypeId);

                worksheet.Cells[row, 1].Value =
                    income.CollectionDate;

                worksheet.Cells[row, 1].Style.Numberformat.Format =
                    "dd/MM/yyyy";

                worksheet.Cells[row, 2].Value =
                    collectionType?.Name
                    ?? income.CollectionTypeId.ToString();

                worksheet.Cells[row, 3].Value =
                    income.PaymentMethod?.Name
                    ?? income.PaymentMethodId.ToString();

                worksheet.Cells[row, 4].Value =
                    income.Notes;

                worksheet.Cells[row, 5].Value =
                    income.Amount;

                worksheet.Cells[row, 5].Style.Numberformat.Format =
                    "₦#,##0.00";

                row++;
            }

            // -----------------------------------------------------
            // TOTAL
            // -----------------------------------------------------
            worksheet.Cells[row, 4].Value =
                "TOTAL";

            worksheet.Cells[row, 4].Style.Font.Bold =
                true;

            worksheet.Cells[row, 5].Formula =
                $"SUM(E4:E{row - 1})";

            worksheet.Cells[row, 5].Style.Font.Bold =
                true;

            worksheet.Cells[row, 5].Style.Numberformat.Format =
                "₦#,##0.00";

            // -----------------------------------------------------
            // FORMATTING
            // -----------------------------------------------------
            worksheet.Cells[
                worksheet.Dimension.Address
            ].AutoFitColumns();

            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 20;
            worksheet.Column(4).Width = 40;
            worksheet.Column(5).Width = 18;

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "IncomeReport.xlsx");
        }

        // =========================================================
        // EXCEL - EXPENSE
        // =========================================================
        private IActionResult ExportExpenseExcel(
            IEnumerable<ExpenditureManagement> data,
            IEnumerable<ExpenditureType> expenditureTypes)
        {
            // EPPlus 8+
            ExcelPackage.License.SetNonCommercialPersonal(
                "Church Management System");

            using var package =
                new ExcelPackage();

            var worksheet =
                package.Workbook.Worksheets.Add(
                    "Expense Report");

            // -----------------------------------------------------
            // TITLE
            // -----------------------------------------------------
            worksheet.Cells["A1:G1"].Merge = true;

            worksheet.Cells["A1"].Value =
                "CHURCH EXPENSE REPORT";

            worksheet.Cells["A1"].Style.Font.Bold =
                true;

            worksheet.Cells["A1"].Style.Font.Size =
                16;

            worksheet.Cells["A1:G1"].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            // -----------------------------------------------------
            // HEADERS
            // -----------------------------------------------------
            worksheet.Cells[3, 1].Value =
                "Date";

            worksheet.Cells[3, 2].Value =
                "Expenditure Type";

            worksheet.Cells[3, 3].Value =
                "Paid To";

            worksheet.Cells[3, 4].Value =
                "Description";

            worksheet.Cells[3, 5].Value =
                "Payment Method";

            worksheet.Cells[3, 6].Value =
                "Status";

            worksheet.Cells[3, 7].Value =
                "Amount";

            using (var header =
                   worksheet.Cells["A3:G3"])
            {
                header.Style.Font.Bold = true;

                header.Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
            }

            // -----------------------------------------------------
            // DATA
            // -----------------------------------------------------
            int row = 4;

            foreach (var expense in data)
            {
                var expenditureType =
                    expenditureTypes.FirstOrDefault(
                        x => x.ExpenditureTypeId ==
                             expense.ExpenditureTypeId);

                worksheet.Cells[row, 1].Value =
                    expense.ExpenditureDate;

                worksheet.Cells[row, 1].Style.Numberformat.Format =
                    "dd/MM/yyyy";

                worksheet.Cells[row, 2].Value =
                    expenditureType?.TypeName
                    ?? expense.ExpenditureTypeName
                    ?? expense.ExpenditureTypeId.ToString();

                worksheet.Cells[row, 3].Value =
                    expense.PayeeName;

                worksheet.Cells[row, 4].Value =
                    expense.Description;

                worksheet.Cells[row, 5].Value =
                    expense.PaymentMethodName
                    ?? expense.PaymentMethodId.ToString();

                worksheet.Cells[row, 6].Value =
                    expense.Status;

                worksheet.Cells[row, 7].Value =
                    expense.Amount;

                worksheet.Cells[row, 7].Style.Numberformat.Format =
                    "₦#,##0.00";

                row++;
            }

            // -----------------------------------------------------
            // TOTAL
            // -----------------------------------------------------
            worksheet.Cells[row, 6].Value =
                "TOTAL";

            worksheet.Cells[row, 6].Style.Font.Bold =
                true;

            worksheet.Cells[row, 7].Formula =
                $"SUM(G4:G{row - 1})";

            worksheet.Cells[row, 7].Style.Font.Bold =
                true;

            worksheet.Cells[row, 7].Style.Numberformat.Format =
                "₦#,##0.00";

            // -----------------------------------------------------
            // FORMATTING
            // -----------------------------------------------------
            worksheet.Cells[
                worksheet.Dimension.Address
            ].AutoFitColumns();

            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 40;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 18;

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ExpenseReport.xlsx");
        }
    }
}