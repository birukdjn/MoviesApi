using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services.Interfaces;

namespace Backend.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context, IPasswordService PasswordService)
        {
            try
            {
                context.Database.Migrate();
            }
            catch
            {
            }

            User admin;

            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                admin = new User
                {
                    Username = "Birukdjn",
                    Email = "Birukedjn@gmail.com",
                    PasswordHash = PasswordService.HashPassword("Birukdjn@8325"),
                    Role = "Admin",
                    Phone ="+251908574808",
                    CreatedAt = DateTime.UtcNow,
                    IsEmailVerified = true,
                    IsSubscribed = true,
                };
                context.Users.Add(admin);
                context.SaveChanges();
            }
            else
            {
                admin = context.Users.First(u => u.Role == "Admin");
            }

            // --- 2. Seed Categories ---
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Movies"},
                    new() { Name = "Series"},
                    new() { Name = "Documentaries"},
                    new() { Name = "Shorts"},
                    new() { Name = "Specials"},
                    new() { Name = "Live TV"},
                    new() { Name = "News"},
                    new() { Name = "Kids & Family"}
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // --- 3. Seed Genres ---
            if (!context.Genres.Any())
            {
                var genres = new List<Genre>
                {
                    new () { Name = "Action" },
                    new () { Name = "Anime" },
                    new () { Name = "Children & Family" },
                    new () { Name = "Classic" },
                    new () { Name = "Comedies" },
                    new () { Name = "Crime" },
                    new () { Name = "Cult" },
                    new () { Name = "Documentaries" },
                    new () { Name = "Dramas" },
                    new () { Name = "Faith & Spirituality" },
                    new () { Name = "Horror" },
                    new () { Name = "Independent" },
                    new () { Name ="International" },
                    new () { Name = "Music" },
                    new () { Name = "Musicals" },
                    new () { Name = "Reality TV" },
                    new () { Name = "Romance" },
                    new () { Name = "Sci-Fi & Fantasy" },
                    new () { Name = "Sports" },
                    new () { Name = "Stand-Up Comedy" },
                    new () { Name = "Thrillers" },
                    new () { Name = "TV Shows" }
                };

                context.Genres.AddRange(genres);
                context.SaveChanges();
            }

            if (!context.Movies.Any())
            {
                var adminUser = context.Users.First(u => u.Role == "Admin");

                var moviesCategory = context.Categories.First(c => c.Name == "Movies");
                var seriesCategory = context.Categories.First(c => c.Name == "Series");
                var documentariesCategory = context.Categories.First(c => c.Name == "Documentaries");
                var specialsCategory = context.Categories.First(c => c.Name == "Specials");

                var actionGenre = context.Genres.First(g => g.Name == "Action");
                var comedyGenre = context.Genres.First(g => g.Name == "Comedies");
                var dramaGenre = context.Genres.First(g => g.Name == "Dramas");
                var sciFiGenre = context.Genres.First(g => g.Name == "Sci-Fi & Fantasy");
                var familyGenre = context.Genres.First(g => g.Name == "Children & Family");
                var crimeGenre = context.Genres.First(g => g.Name == "Crime");
                var horrorGenre = context.Genres.First(g => g.Name == "Horror");
                var independentGenre = context.Genres.First(g => g.Name == "Independent");
                var animeGenre = context.Genres.First(g => g.Name == "Anime");
                var standUpGenre = context.Genres.First(g => g.Name == "Stand-Up Comedy");
                var romanceGenre = context.Genres.First(g => g.Name == "Romance");
                var sportsGenre = context.Genres.First(g => g.Name == "Sports");
                var thrillerGenre = context.Genres.First(g => g.Name == "Thrillers");
                var cultGenre = context.Genres.First(g => g.Name == "Cult");
                var classicGenre = context.Genres.First(g => g.Name == "Classic");
                var musicGenre = context.Genres.First(g => g.Name == "Music");

                var basicUser = new User
                {
                    Username = "user",
                    Email = "user@example.com",
                    PasswordHash = PasswordService.HashPassword("user123"),
                    Role = "User",
                    Phone ="+251777888325",
                    CreatedAt = DateTime.UtcNow,
                    IsEmailVerified = true,
                    IsSubscribed = true,

                };
                context.Users.Add(basicUser);
                context.SaveChanges();

                var mainProfile = new Profile
                {
                    Name = "User Profile",
                    Avatar = "/avatars/user.png",
                    IsKidsProfile = false,
                    UserId = basicUser.Id
                };
                context.Profiles.Add(mainProfile);

                var DadProfile = new Profile
                {
                    Name = "Dad Profile",
                    Avatar = "/avatars/dad.png",
                    IsKidsProfile = false,
                    UserId = basicUser.Id
                };
                context.Profiles.Add(DadProfile);

                var kidsProfile = new Profile
                {
                    Name = "Kids Zone",
                    Avatar = "/avatars/kid.png",
                    IsKidsProfile = true,
                    UserId = basicUser.Id
                };
                context.Profiles.Add(kidsProfile);
                context.SaveChanges();

                var movies = new List<Movie>
                {
                    // 1. Music/Spiritual Film - Using actual Ethiopian music video
                    new() {
                        Title = "Lidia Anteneh - Tayelegn",
                        ReleaseYear = 2025,
                        Director = "Bereket Tesfaye",
                        Description = "Lidia Anteneh @ Kingdom Sound Worship Night 2025 \"Tayelegn\" Original Song by Bereket Tesfaye & \"Ante kebreh Tay\" Original Song by Meskerem Getu",
                        RuntimeMinutes = 19,
                        AgeRating = "All Ages",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=RKp3pjPHo64",
                        YoutubeId = "RKp3pjPHo64",
                        IsOriginal = true,
                        MovieCategories = [new MovieCategory { CategoryId = moviesCategory.Id }],
                        MovieGenres =
                        [
                            new() { GenreId = musicGenre.Id },
                            new () { GenreId = dramaGenre.Id }
                        ]
                    },

                    // 2. Family Comedy - Using official movie trailer
                    new() {
                        Title = "Dog's Day Out",
                        ReleaseYear = 2023,
                        Director = "Paul King",
                        Description = "A talking golden retriever gets lost on a hilarious journey home.",
                        RuntimeMinutes = 98,
                        AgeRating = "TV-PG",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=T0Qk7fdzb_c",
                        YoutubeId = "T0Qk7fdzb_c",
                        IsOriginal = false,
                        MovieCategories = [new MovieCategory { CategoryId = moviesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = comedyGenre.Id },
                            new MovieGenre { GenreId = familyGenre.Id }
                        ]
                    },

                    // 3. Historical Drama Series - Using actual series trailer
                    new() {
                        Title = "The Crown of Fire (S1 E1)",
                        ReleaseYear = 2022,
                        Director = "Sofia Bell",
                        Description = "The dramatic start of a new royal dynasty.",
                        RuntimeMinutes = 55,
                        AgeRating = "TV-MA",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=JfVOs4VSpmA",
                        YoutubeId = "JfVOs4VSpmA",
                        IsOriginal = true,
                        MovieCategories = [new MovieCategory { CategoryId = seriesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = dramaGenre.Id },
                            new MovieGenre { GenreId = classicGenre.Id }
                        ]
                    },

                    // 4. True Crime Documentary - Using actual documentary
                    new() {
                        Title = "Shadows in the System",
                        ReleaseYear = 2024,
                        Director = "Dan Evans",
                        Description = "A deep dive into a notorious, unsolved cold case.",
                        RuntimeMinutes = 110,
                        AgeRating = "TV-MA",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=6k6JO0X2AHE",
                        YoutubeId = "6k6JO0X2AHE",
                        IsOriginal = false,
                        MovieCategories = [new MovieCategory { CategoryId = documentariesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = dramaGenre.Id },
                            new MovieGenre { GenreId = crimeGenre.Id }
                        ]
                    },

                    // 5. Indie Horror Film - Using actual horror short film
                    new() {
                        Title = "The Whispering Woods",
                        ReleaseYear = 2021,
                        Director = "Liam O'Connell",
                        Description = "Teenagers unleash an ancient evil in a remote forest cabin.",
                        RuntimeMinutes = 88,
                        AgeRating = "R",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=4H5I1bBFt_c",
                        YoutubeId = "4H5I1bBFt_c",
                        IsOriginal = false,
                        MovieCategories = [new MovieCategory { CategoryId = moviesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = horrorGenre.Id },
                            new MovieGenre { GenreId = independentGenre.Id }
                        ]
                    },

                    // 6. Animated Anime Series - Using actual anime trailer
                    new() {
                        Title = "Cyber Samurai (Pilot)",
                        ReleaseYear = 2024,
                        Director = "Kenji Sato",
                        Description = "In a neon city, a disgraced warrior seeks redemption.",
                        RuntimeMinutes = 24,
                        AgeRating = "TV-14",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=f9v4ALp-1pI",
                        YoutubeId = "f9v4ALp-1pI",
                        IsOriginal = true,
                        MovieCategories = [new MovieCategory { CategoryId = seriesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = actionGenre.Id },
                            new MovieGenre { GenreId = animeGenre.Id }
                        ]
                    },

                    // 7. Stand-Up Special - Using actual comedy special
                    new() {
                        Title = "Mark Z. Live in Vegas",
                        ReleaseYear = 2023,
                        Director = "Self",
                        Description = "Comedian Mark Z. delivers an hour of razor-sharp social commentary.",
                        RuntimeMinutes = 60,
                        AgeRating = "TV-MA",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=Xm2Nc6Cpl_k",
                        YoutubeId = "Xm2Nc6Cpl_k",
                        IsOriginal = true,
                        MovieCategories = [new MovieCategory { CategoryId = specialsCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = standUpGenre.Id }
                        ]
                    },

                    // 8. Romantic Comedy - Using actual romance film trailer
                    new() {
                        Title = "Coffee Shop Connection",
                        ReleaseYear = 2020,
                        Director = "Mia T.",
                        Description = "Two strangers meet every morning, leading to an unexpected romance.",
                        RuntimeMinutes = 105,
                        AgeRating = "PG",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=7TavVZMewpY",
                        YoutubeId = "7TavVZMewpY",
                        IsOriginal = false,
                        MovieCategories = [new MovieCategory { CategoryId = moviesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = comedyGenre.Id },
                            new MovieGenre { GenreId = romanceGenre.Id }
                        ]
                    },

                    // 9. Sports Documentary - Using actual sports documentary
                    new() {
                        Title = "The Unbroken Record",
                        ReleaseYear = 2019,
                        Director = "Robert L.",
                        Description = "Chronicling the career of a legendary but controversial marathon runner.",
                        RuntimeMinutes = 95,
                        AgeRating = "TV-14",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=Y66jBbUjXOQ",
                        YoutubeId = "Y66jBbUjXOQ",
                        IsOriginal = false,
                        MovieCategories = [new MovieCategory { CategoryId = documentariesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = sportsGenre.Id }
                        ]
                    },

                    // 10. Cult Thriller - Using actual thriller trailer
                    new() {
                        Title = "Midnight Drive",
                        ReleaseYear = 2005,
                        Director = "J. J. Curtis",
                        Description = "A man picks up a mysterious passenger, leading to a night of terror.",
                        RuntimeMinutes = 92,
                        AgeRating = "R",
                        ThumbnailUrl = "",
                        BackdropUrl = "",
                        VideoUrl = "https://www.youtube.com/watch?v=WMz3X5t7nC0",
                        YoutubeId = "WMz3X5t7nC0",
                        IsOriginal = false,
                        MovieCategories = [new MovieCategory { CategoryId = moviesCategory.Id }],
                        MovieGenres =
                        [
                            new MovieGenre { GenreId = thrillerGenre.Id },
                            new MovieGenre { GenreId = cultGenre.Id }
                        ]
                    }
                };

                context.Movies.AddRange(movies);
                context.SaveChanges();

                // --- D. Seed Relationships (Ratings, Favorites, Playback) ---

                // Rating (Main Profile rates Movie 1)
                context.Ratings.Add(new Rating
                {
                    MovieId = movies[0].Id,
                    ProfileId = mainProfile.Id,
                    Score = 5,
                    RatedDate = DateTime.UtcNow
                });

                // Favorite (Kids Profile favorites Movie 4)
                context.Favorites.Add(new Favorite
                {
                    MovieId = movies[3].Id,
                    ProfileId = kidsProfile.Id,
                    AddedDate = DateTime.UtcNow.AddHours(-1)
                });

                // Playback Position (Main Profile watched Movie 2)
                context.PlaybackPositions.Add(new PlaybackPosition
                {
                    MovieId = movies[1].Id,
                    ProfileId = mainProfile.Id,
                    PositionInSeconds = 600,
                    TotalDurationInSeconds = 1500,
                    LastWatchedDate = DateTime.UtcNow.AddMinutes(-30)
                });

                // Final save for all relationships
                context.SaveChanges();
            }
        }
    }
}