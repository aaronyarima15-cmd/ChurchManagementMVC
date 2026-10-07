using System;
using System.ComponentModel.DataAnnotations;

namespace ChurchManagementMVC.Models
{
    public class AttendanceSummary
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "Service Type is required.")]
        public string ServiceType { get; set; } = string.Empty;

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Men cannot be negative.")]
        public int Men { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Women cannot be negative.")]
        public int Women { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Children cannot be negative.")]
        public int Children { get; set; }

        public int Total { get; set; }

        public string? Remarks { get; set; }

        public DateTime DateRecorded { get; set; }
    }
}