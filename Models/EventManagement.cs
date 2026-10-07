using System.ComponentModel.DataAnnotations;

namespace ChurchManagementMVC.Models
{
    public class EventManagement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Event type is required")]
        public int EventTypeId { get; set; }

        public EventType? EventType { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Event time is required")]
        [DataType(DataType.Time)]
        public TimeSpan EventTime { get; set; }

        public int? CoordinatorId { get; set; }

        public int? GroupId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        public string? EventItem { get; set; }
    }

    public class EventType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event type name is required")]
        public string Name { get; set; } = string.Empty;
    }
}