using ChurchManagementMVC.Models;

namespace ChurchManagementMVC.Models    
{
    public class GroupMember
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public GroupManagement? Group { get; set; }

        public int MemberId { get; set; }

        public MembershipManagement? Member { get; set; }

        public string Role { get; set; } = "Member";

        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    }
}