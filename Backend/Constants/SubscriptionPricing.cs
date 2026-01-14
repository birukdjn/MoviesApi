using Backend.Enums;

namespace Backend.Constants
{
    public static class SubscriptionPricing
    {
        public static decimal GetPrice(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Basic => 9.99m,
                SubscriptionPlan.Standard => 14.99m,
                SubscriptionPlan.Premium => 19.99m,
                _ => 0m
            };
        }
    }
}