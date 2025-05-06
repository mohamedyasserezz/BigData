using BigData.Apis.Entities;
using BigData.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigData.Persistance.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<InvertedIndexEntry> InvertedIndexEntries { get; set; }
        public DbSet<Document> Documents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add your model configurations here
        }
    }
}
