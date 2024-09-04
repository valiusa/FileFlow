using Entities.DataModels;
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
        public virtual DbSet<FileStorage> FileStorages { get; set; }

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
            modelBuilder.Entity<FileStorage>(entity =>
            {
                entity.ToTable("FileStorages");

                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();

                entity.Property(t => t.Name);

                entity.Property(t => t.Extension);

                entity.Property(t => t.Path);

                entity.Property(t => t.CreatedOn)
                      .IsRequired(true);
            });
        }
    }
}
