using System.ComponentModel.DataAnnotations;

namespace ChurchManagementMVC.Models
{
    public class ExpenditureManagement
    {
        public int ExpenditureId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Expenditure Date")]
        public DateTime ExpenditureDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Expenditure Type")]
        public int ExpenditureTypeId { get; set; }

        [Display(Name = "Expenditure Type")]
        public string? ExpenditureTypeName { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }

        [Display(Name = "Payment Method")]
        public string? PaymentMethodName { get; set; }

        [StringLength(200)]
        [Display(Name = "Payee Name")]
        public string? PayeeName { get; set; }

        [StringLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "Requested By")]
        public string? RequestedBy { get; set; }

        [StringLength(200)]
        [Display(Name = "Approved By")]
        public string? ApprovedBy { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime DateCreated { get; set; }

        [StringLength(200)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }
    }
}