namespace ChurchManagementMVC.Models
{
    public class GroupMonthlyDuePayment
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int MemberId { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public decimal Amount { get; set; }
    }
}