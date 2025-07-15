// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrimeGames.Models;

namespace PrimeGames.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }
        public DbSet<ArticleMedia> ArticleMedia { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ColorCode).HasMaxLength(20).HasDefaultValue("#3B82F6");
            });

            builder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ColorCode).HasMaxLength(20).HasDefaultValue("#6B7280");
            });

            builder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PublishedDate);
                entity.HasIndex(e => e.CreatedDate);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Summary).HasMaxLength(1000);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AuthorId).HasMaxLength(450);
                entity.Property(e => e.FeaturedImagePath).HasMaxLength(500);
                entity.Property(e => e.FeaturedImageAlt).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedBy).HasMaxLength(450);
                entity.Property(e => e.MetaDescription).HasMaxLength(200);
                entity.Property(e => e.MetaKeywords).HasMaxLength(500);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Articles)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ArticleTag>(entity =>
            {
                entity.HasKey(e => new { e.ArticleId, e.TagId });

                entity.HasOne(e => e.Article)
                      .WithMany(a => a.ArticleTags)
                      .HasForeignKey(e => e.ArticleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                      .WithMany(t => t.ArticleTags)
                      .HasForeignKey(e => e.TagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ArticleMedia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ArticleId);

                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.AltText).HasMaxLength(200);
                entity.Property(e => e.Caption).HasMaxLength(500);
                entity.Property(e => e.MimeType).HasMaxLength(50);
                entity.Property(e => e.UploadedBy).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.Article)
                      .WithMany(a => a.Media)
                      .HasForeignKey(e => e.ArticleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Bio).HasMaxLength(500);
                entity.Property(e => e.AvatarPath).HasMaxLength(500);

                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<UserProfile>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserProfileId, e.ArticleId }).IsUnique();

                entity.HasOne(e => e.UserProfile)
                      .WithMany(u => u.Favorites)
                      .HasForeignKey(e => e.UserProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Article)
                      .WithMany(a => a.Favorites)
                      .HasForeignKey(e => e.ArticleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "ข่าวเกม",
                    Slug = "news",
                    Description = "ข่าวสารเกมล่าสุด",
                    ColorCode = "#22C55E",
                    SortOrder = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = 2,
                    Name = "รีวิวเกม",
                    Slug = "review",
                    Description = "รีวิวเกมใหม่และเกมน่าสนใจ",
                    ColorCode = "#3B82F6",
                    SortOrder = 2,
                    CreatedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = 3,
                    Name = "ดีล/ลดราคา",
                    Slug = "deal",
                    Description = "ข้อมูลเกมลดราคาและโปรโมชั่น",
                    ColorCode = "#F59E0B",
                    SortOrder = 3,
                    CreatedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = 4,
                    Name = "Mods",
                    Slug = "mods",
                    Description = "Mods และการดัดแปลงเกม",
                    ColorCode = "#8B5CF6",
                    SortOrder = 4,
                    CreatedDate = DateTime.UtcNow
                }
            );

            builder.Entity<Tag>().HasData(
                new Tag { Id = 1, Name = "Gaming", Slug = "gaming", ColorCode = "#22C55E", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 2, Name = "News", Slug = "news", ColorCode = "#3B82F6", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 3, Name = "Review", Slug = "review", ColorCode = "#8B5CF6", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 4, Name = "Deal", Slug = "deal", ColorCode = "#F59E0B", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 5, Name = "Steam", Slug = "steam", ColorCode = "#1E40AF", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 6, Name = "PlayStation", Slug = "playstation", ColorCode = "#1E3A8A", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 7, Name = "Xbox", Slug = "xbox", ColorCode = "#16A34A", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 8, Name = "Nintendo", Slug = "nintendo", ColorCode = "#DC2626", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 9, Name = "RPG", Slug = "rpg", ColorCode = "#7C3AED", CreatedDate = DateTime.UtcNow },
                new Tag { Id = 10, Name = "FPS", Slug = "fps", ColorCode = "#B45309", CreatedDate = DateTime.UtcNow }
            );
        }
    }
}