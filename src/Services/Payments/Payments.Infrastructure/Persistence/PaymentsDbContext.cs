using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.Currency).HasMaxLength(3).IsRequired();
            entity.Property(p => p.CardToken).HasMaxLength(500).IsRequired();
            entity.Property(p => p.TransactionId).HasMaxLength(200);
            entity.Property(p => p.FailureReason).HasMaxLength(1000);

            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedAt);
        });
    }
}
