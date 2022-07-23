using ShuffleDataMasking.Domain.Masking.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShuffleDataMasking.Infra.Data.Mappings.EntityFramework
{
    public class DataCollectorMapping : IEntityTypeConfiguration<DataCollector>
    {
        public void Configure(EntityTypeBuilder<DataCollector> builder)
        {
            builder.ToTable("DATA_COLLECTOR");

            builder.Property(d => d.DataCollectorId)
               .HasColumnName("DATA_COLLECTOR_ID")
               .IsRequired();

            builder.Property(d => d.OriginalData)
               .HasColumnName("ORIGINAL_DATA")
               .IsRequired();

            builder.Property(d => d.MaskedTypeId)
               .HasColumnName("MASK_TYPE_ID")
               .IsRequired();

            builder.Property(d => d.MaskedData)
               .HasColumnName("MASKED_DATA")
               .IsRequired();

            builder.Property(p => p.CreatedAt)
               .HasColumnName("CREATED_AT_DT")
               .HasColumnType("datetime2")
               .IsRequired();

            builder.Property(p => p.CreatedBy)
               .HasColumnName("CREATED_BY_DS")
               .IsRequired();
        }
    }
}