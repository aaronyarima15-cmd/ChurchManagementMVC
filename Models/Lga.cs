using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchManagementMVC.Models
{
    public class Lga
    {
        [Key]
        public int LgaId { get; set; }

        [Required]
        public int StateId { get; set; }

        [ForeignKey(nameof(StateId))]
        public State? State { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}