using Microsoft.EntityFrameworkCore;
using NanoLink.Services;

namespace NanoLink.Models
{
    public class ApiDbContext : DbContext
    {
        public virtual DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        public ApiDbContext(DbContextOptions<ApiDbContext> options ) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>(builder =>
            {
                builder
                    .Property(shortenedUrl => shortenedUrl.Code)
                    .HasMaxLength(ShortLinkSettings.Length);

                builder
                    .HasIndex(shortenedUrl => shortenedUrl.Code)
                    .IsUnique();
            });
        }
    }
}
