using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // --- DbSet Properties (Clean and Complete) ---
        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; }

        // Join Tables
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<PlaybackPosition> PlaybackPositions { get; set; } = null!;
        public DbSet<MovieGenre> MovieGenres { get; set; } = null!;
        public DbSet<MovieCategory> MovieCategories { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- 1. User Configuration ---
            // If you changed Username to Email, this should be updated:
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // User to Profile (One User has Many Profiles)
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithMany(u => u.Profiles)
                .HasForeignKey(p => p.UserId);


            // --- 2. MovieGenre (Many-to-Many) ---
            modelBuilder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            // --- 3. MovieCategory (Many-to-Many) ---
            modelBuilder.Entity<MovieCategory>()
                .HasKey(mc => new { mc.MovieId, mc.CategoryId });


            // --- 4. Favorite (Composite Key & Relationships) ---
            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.ProfileId, f.MovieId });

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Profile)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Movie)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MovieId);

           

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);



            // --- 5. Rating (Composite Key & Relationships) ---
            modelBuilder.Entity<Rating>()
                .HasKey(r => new { r.ProfileId, r.MovieId }); 

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Profile)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Movie)
                .WithMany(m => m.Ratings)
                .HasForeignKey(r => r.MovieId);


            // --- 6. PlaybackPosition ---
            // Profile to PlaybackPosition
            modelBuilder.Entity<PlaybackPosition>()
                .HasOne(pp => pp.Profile)
                .WithMany(p => p.PlaybackPositions)
                .HasForeignKey(pp => pp.ProfileId);

            // Movie to PlaybackPosition
            modelBuilder.Entity<PlaybackPosition>()
                .HasOne(pp => pp.Movie)
                .WithMany(m => m.PlaybackPositions)
                .HasForeignKey(pp => pp.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 4)") 
                .HasPrecision(18, 4);

            base.OnModelCreating(modelBuilder);
        }
    }
}