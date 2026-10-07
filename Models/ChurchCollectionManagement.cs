namespace ChurchManagementMVC.Models
{
    public class ChurchCollectionManagement
    {
        public int Id { get; set; }

        public DateTime CollectionDate { get; set; }

        public int CollectionTypeId { get; set; }

        public CollectionType? CollectionType { get; set; }

        public decimal Amount { get; set; }

        public int PaymentMethodId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public string? Notes { get; set; }

        // =========================
        // GROUP
        // =========================

        public int? GroupId { get; set; }

        public GroupManagement? Group { get; set; }
    }
}