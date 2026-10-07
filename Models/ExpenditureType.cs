using System.ComponentModel.DataAnnotations;

namespace ChurchManagementMVC.Models
{
    public class ExpenditureType
    {
        public int ExpenditureTypeId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Expenditure Type")]
        public string TypeName { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
    }
}