using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Models;

namespace FabrikaTakipPaneli.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockEntry> StockEntries => Set<StockEntry>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentSequence> ShipmentSequences => Set<ShipmentSequence>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StockEntry>()
            .HasOne(s => s.Product)
            .WithMany(p => p.StockEntries)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StockEntry>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ShipmentSequence>()
            .HasKey(s => s.Year);

        builder.Entity<ShipmentSequence>()
            .Property(s => s.Year)
            .ValueGeneratedNever();

        builder.Entity<Shipment>()
            .HasIndex(s => s.OrderNumber)
            .IsUnique();

        builder.Entity<Shipment>()
            .HasOne(s => s.StockEntry)
            .WithOne(e => e.Shipment)
            .HasForeignKey<Shipment>(s => s.StockEntryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
