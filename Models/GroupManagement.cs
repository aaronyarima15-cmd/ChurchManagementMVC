using ChurchManagementMVC.Models;

namespace ChurchManagementMVC.Models
{
    public class GroupManagement
    {
        public int Id { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int GroupTypeId { get; set; }
        public GroupType? GroupType { get; set; }

        public int? CoordinatorId { get; set; }

        public int? ActivityId { get; set; }
        public GroupActivity? Activity { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
