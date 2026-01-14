namespace Backend.DTOs.Admin
{
    public class AdminStatsDto
    {
        
            public UserStats? Users { get; set; }
            public ContentStats? Content { get; set; }
            public EngagementStats? Engagement { get; set; }
            public SubscriptionStats? Subscriptions { get; set; }
            public RevenueStats? Revenue { get; set; }


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
        public int TotalSeries { get; set; }
        public int TotalEpisodes { get; set; }
        public int TotalDirectors { get; set; }
        public int TotalLanguages { get; set; }
        public int TotalCountries { get; set; }
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
        public int BasicSubscriptions { get; set; }
        public int StandardSubscriptions { get; set; }
        public int PremiumSubscriptions { get; set; }
    }

    public class RevenueStats
    {
        public decimal TotalRevenue { get; set; }
        public decimal BasicRevenue { get; set; }
        public decimal StandardRevenue { get; set; }
        public decimal PremiumRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }




}
