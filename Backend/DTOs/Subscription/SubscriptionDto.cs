using Backend.Enums;


namespace Backend.DTOs.Subscription
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public SubscriptionPlan Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TxRef { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; }
        public bool IsActive { get; set; }
    }
}