namespace ChurchManagementMVC.Models
{
    public class Position
    {
        public int Id { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
    }
}