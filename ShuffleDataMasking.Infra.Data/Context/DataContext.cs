using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Infra.Data.Mappings.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ShuffleDataMasking.Infra.Data.Contexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DataCollectorMapping());
            base.OnModelCreating(modelBuilder);
        }
    }
}