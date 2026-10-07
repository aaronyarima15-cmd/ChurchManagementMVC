namespace ChurchManagementMVC.Models
{
    public class MembershipStatus
    {
        public int Id { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
    }
}
