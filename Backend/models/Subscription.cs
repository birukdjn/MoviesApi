using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Enums;

namespace Backend.Models
{

    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]

        public User User { get; set; } = null!;

        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Basic;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public string TxRef { get; set; } = string.Empty; 

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

        [NotMapped]
        public bool IsActive => Status == SubscriptionStatus.Active
                         && (EndDate == null || EndDate > DateTime.UtcNow);
    }
}