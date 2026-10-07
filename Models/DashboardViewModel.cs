namespace ChurchManagementMVC.Models
{
    public class DashboardViewModel
    {
        public int PeopleCount { get; set; }

        public int GroupsCount { get; set; }

        public int CollectionsCount { get; set; }

        public List<EventManagement> UpcomingEvents { get; set; }
            = new List<EventManagement>();
    }
}