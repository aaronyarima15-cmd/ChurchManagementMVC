using System.ComponentModel.DataAnnotations;

namespace ChurchManagementMVC.Models
{
    public class State
    {
        [Key]
        public int StateId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<Lga> Lgas { get; set; } = new List<Lga>();
    }
}