namespace ChurchManagementMVC.Models
{
    public class CommunionParticipant
    {
        public int Id { get; set; }

        public int CommunionServiceId { get; set; }

        public CommunionService? CommunionService { get; set; }

        public int MemberId { get; set; }

        public MembershipManagement? Member { get; set; }

        public string? Remarks { get; set; }
    }
}