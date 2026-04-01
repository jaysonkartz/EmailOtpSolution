using EmailOtpCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailOtpInfrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OtpRecord>(entity =>
        {
            entity.ToTable("OtpRecords");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.OtpCode)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(x => x.CreatedUtc)
                .IsRequired();

            entity.Property(x => x.ExpiryUtc)
                .IsRequired();

            entity.HasIndex(x => x.Email);
        });
    }
}