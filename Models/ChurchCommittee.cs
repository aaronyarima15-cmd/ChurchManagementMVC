namespace ChurchManagementMVC.Models
{
    public class ChurchCommittee
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public int PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public DateTime DateRegistered { get; set; }
    }
}