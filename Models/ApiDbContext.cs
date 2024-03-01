using Microsoft.EntityFrameworkCore;

namespace NanoLink.Models
{
    public class ApiDbContext : DbContext
    {
        public virtual DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        public ApiDbContext(DbContextOptions<ApiDbContext> options ) : base(options)
        {
            
        }
    }
}
