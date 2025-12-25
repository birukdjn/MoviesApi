namespace Backend.DTOs.Admin
{
    public class AdminStatsDto
    {
        
            public UserStats Users { get; set; }
            public ContentStats Content { get; set; }
            public EngagementStats Engagement { get; set; }
            public SubscriptionStats Subscriptions { get; set; }
        

    }

    public class UserStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalProfiles { get; set; }
    }
    public class ContentStats
    {
        public int TotalMovies { get; set; }
        public int TotalCategories { get; set; }
        public int TotalGenres { get; set; }
    }

    public class EngagementStats
    {
        public int TotalRatings { get; set; }
        public int TotalFavorites { get; set; }
        public double AverageRating {  get; set; }

    }
    public class SubscriptionStats
    {
        public int TotalSubscriptions { get;set; }

    }
}
