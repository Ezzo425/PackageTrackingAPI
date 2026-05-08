using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PackageTracking.API.Models;

namespace PackageTracking.API.Configurations
{
    public class PackageConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable("Packages");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.TrackingNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.TrackingNumber)
                .IsUnique();

            builder.Property(p => p.SenderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ReceiverName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.CurrentLocation)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.EstimatedDeliveryDate)
                .IsRequired(false);
        }
    }
}
