namespace ChurchManagementMVC.ViewModels
{
    public class GroupMonthlyDuesVM
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int Year { get; set; }

        public List<GroupMonthlyDuesMemberVM> Members { get; set; }
            = new List<GroupMonthlyDuesMemberVM>();
    }

    public class GroupMonthlyDuesMemberVM
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public decimal?[] MonthlyAmounts { get; set; }
            = new decimal?[12];
    }
}