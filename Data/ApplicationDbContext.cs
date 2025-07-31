using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrimeGames.Data;
using cleanNETCoreMVC.Models;

namespace PrimeGames.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Content> Contents { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ContentTag> ContentTags { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ContentTag>()
                .HasKey(ct => new { ct.ContentId, ct.TagId });
            builder.Entity<ContentTag>()
                .HasOne(ct => ct.Content)
                .WithMany(c => c.ContentTags)
                .HasForeignKey(ct => ct.ContentId);
            builder.Entity<ContentTag>()
                .HasOne(ct => ct.Tag)
                .WithMany(t => t.ContentTags)
                .HasForeignKey(ct => ct.TagId);

            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Favorite>()
                .HasOne(f => f.Content)
                .WithMany(c => c.Favorites)
                .HasForeignKey(f => f.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
