
using Microsoft.EntityFrameworkCore;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Infrastructure.Data;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureLedgerIsImmutable();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnsureLedgerIsImmutable();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureLedgerIsImmutable()
    {
        if (ChangeTracker.Entries<InventoryTransaction>().Any(e => e.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Inventory transactions are immutable and cannot be updated or deleted.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.ProductId);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SKU).HasMaxLength(80);
            entity.Property(x => x.Barcode).HasMaxLength(80);
            entity.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.SellingPrice).HasPrecision(18, 2);
            entity.Property(x => x.ReorderLevel).HasPrecision(18, 3);
            entity.Property(x => x.ReorderQuantity).HasPrecision(18, 3);
            entity.Property(x => x.CurrentStock).HasPrecision(18, 3);
            // Use provider-agnostic filter expression (SQL fragment) for SQL Server
            entity.HasIndex(x => x.SKU).IsUnique().HasFilter("SKU IS NOT NULL");
            entity.HasIndex(x => x.Barcode).IsUnique().HasFilter("Barcode IS NOT NULL");
            entity.HasIndex(x => new { x.Name, x.IsActive });
            entity.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(x => x.SupplierId);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.TaxRegistrationNumber).HasMaxLength(50);
            entity.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.HasIndex(x => x.Phone);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(x => x.PurchaseId);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.PurchaseDate);
            entity.HasOne(x => x.Supplier).WithMany(x => x.Purchases).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseItem>(entity =>
        {
            entity.HasKey(x => x.PurchaseItemId);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Purchase).WithMany(x => x.Items).HasForeignKey(x => x.PurchaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany(x => x.PurchaseItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Batch).WithMany(x => x.PurchaseItems).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.ProductId);
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(x => x.BatchId);
            entity.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.AvailableQuantity).HasPrecision(18, 3);
            entity.HasIndex(x => new { x.ProductId, x.ExpiryDate });
            entity.HasIndex(x => new { x.BatchNumber, x.ProductId }).IsUnique();
            entity.HasOne(x => x.Product).WithMany(x => x.Batches).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(x => x.SaleId);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.SaleDate);
            entity.HasOne(x => x.Customer).WithMany(x => x.Sales).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(x => x.SaleItemId);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Sale).WithMany(x => x.Items).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany(x => x.SaleItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ProductId);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(x => x.TransactionId);
            entity.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.QuantityDelta).HasPrecision(18, 3);
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.CreatedBy).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => new { x.ProductId, x.CreatedAt });
            entity.HasIndex(x => new { x.BatchId, x.CreatedAt });
            entity.HasIndex(x => x.ReferenceId);
            entity.HasOne(x => x.Product).WithMany(x => x.InventoryTransactions).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Batch).WithMany(x => x.InventoryTransactions).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(x => x.AdjustmentId);
            entity.Property(x => x.SystemQuantity).HasPrecision(18, 3);
            entity.Property(x => x.PhysicalQuantity).HasPrecision(18, 3);
            entity.Property(x => x.Difference).HasPrecision(18, 3);
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ApprovedBy).HasMaxLength(150);
            entity.HasIndex(x => new { x.ProductId, x.CreatedAt });
            entity.HasOne(x => x.Product).WithMany(x => x.StockAdjustments).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Demo Rice 5kg",
                SKU = "RICE-5KG",
                Unit = "bag",
                PurchasePrice = 250,
                SellingPrice = 300,
                ReorderLevel = 10,
                ReorderQuantity = 50,
                CurrentStock = 0,
                RequiresBatchTracking = true,
                IsActive = true
            });

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                CategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Grocery",
                Description = "Default grocery category",
                IsActive = true
            });
    }
}
