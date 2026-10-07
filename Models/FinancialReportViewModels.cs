using System;
using System.Collections.Generic;

namespace ChurchManagementMVC.Models
{
    // =========================================================
    // INCOME / COLLECTION REPORT
    // =========================================================
    public class IncomeReportViewModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? CollectionTypeId { get; set; }

        public List<CollectionType> CollectionTypes { get; set; }
            = new List<CollectionType>();

        public List<ChurchCollectionManagement> Results { get; set; }
            = new List<ChurchCollectionManagement>();

        public decimal TotalAmount { get; set; }

        public bool HasSearched { get; set; }
    }


    // =========================================================
    // EXPENSE REPORT
    // =========================================================
    public class ExpenseReportViewModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ExpenditureTypeId { get; set; }

        public string? PayeeName { get; set; }

        public List<ExpenditureType> ExpenditureTypes { get; set; }
            = new List<ExpenditureType>();

        public List<ExpenditureManagement> Results { get; set; }
            = new List<ExpenditureManagement>();

        public decimal TotalAmount { get; set; }

        public bool HasSearched { get; set; }
    }
}