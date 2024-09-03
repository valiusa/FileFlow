using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class FileFlowDbContext : DbContext
    {
        public FileFlowDbContext(DbContextOptions<FileFlowDbContext> options)
            : base(options)
        {
            
        }

        // Add DbSet here

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=VALIUSA\\SQLEXPRESS;Initial Catalog=FileFlow;User Id=sa;Password=pass226;TrustServerCertificate=True;Trusted_Connection=True;Encrypt=True;", o => o.UseCompatibilityLevel(120));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add Entities here
        }
    }
}
