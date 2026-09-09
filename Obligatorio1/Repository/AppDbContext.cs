using System.Diagnostics.CodeAnalysis;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Repository;

[ExcludeFromCodeCoverage]
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ShippingType> ShippingTypes { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductPicture> ProductPictures { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CommercialLine> CommercialLines { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<AuditLogger> AuditLoggers { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureShippingType(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureProductPicture(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureCommercialLine(modelBuilder);
        ConfigurePromotion(modelBuilder);
        ConfigureSession(modelBuilder);
        ConfigureAuditLogger(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name).IsRequired();
            entity.Property(u => u.Surname).IsRequired();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Phone).IsRequired();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.Salt).IsRequired();
            entity.Property(u => u.Role).IsRequired();
            entity.Property(u => u.IsInternal).IsRequired();
            entity.Property(u => u.IsActive).IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasMany(u => u.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderNumber);

            entity.Property(o => o.CreatedAt).IsRequired();
            entity.Property(o => o.CustomerId).IsRequired();
            entity.Property(o => o.Status).IsRequired();
            entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
            entity.Property(o => o.Subtotal).HasPrecision(18, 2);
            entity.Property(o => o.Total).HasPrecision(18, 2);

            entity.Property(o => o.ShippingTypeId).IsRequired();

            entity.HasOne(o => o.ShippingType)
                .WithMany()
                .HasForeignKey(o => o.ShippingTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.OwnsOne(o => o.DeliveryAddress, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("Street")
                    .IsRequired();

                address.Property(a => a.DoorNumber)
                    .HasColumnName("DoorNumber")
                    .IsRequired();

                address.Property(a => a.Apartment)
                    .HasColumnName("Apartment");
            });

            entity.OwnsMany(o => o.Lines, line =>
            {
                line.ToTable("OrderLines");

                line.WithOwner()
                    .HasForeignKey("OrderNumber");

                line.Property(l => l.Id);
                line.HasKey(l => l.Id);

                line.Property(l => l.ProductId).IsRequired();
                line.Property(l => l.Quantity).IsRequired();
                line.Property(l => l.UnitPrice).HasPrecision(18, 2).IsRequired();
                line.Property(l => l.Discount).HasPrecision(18, 2).IsRequired();

                line.HasOne(l => l.Product)
                    .WithMany()
                    .HasForeignKey(l => l.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                line.Ignore(l => l.Subtotal);
            });

            entity.Navigation(o => o.Lines)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    private static void ConfigureShippingType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShippingType>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .IsRequired();

            entity.Property(s => s.Cost)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(s => s.IsDefault)
    .IsRequired();

            entity.HasIndex(s => s.Name)
                .IsUnique();

            entity.HasData(
                new
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Express",
                    Cost = 250m,
                    IsDefault = true
                },
                new
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "24Hours",
                    Cost = 180m,
                    IsDefault = true
                }
            );
        });
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Code).IsRequired();
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.IsActive).IsRequired();

            entity.HasOne(p => p.CommercialLine)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Category)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Pictures)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProductPicture(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductPicture>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.FileName).IsRequired();
            entity.Property(p => p.SizeInKb).IsRequired();
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired();
        });
    }

    private static void ConfigureCommercialLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommercialLine>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired();
        });
    }

    private static void ConfigurePromotion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.StartDate).IsRequired();
            entity.Property(p => p.EndDate).IsRequired();
            entity.Property(p => p.Discount).IsRequired();

            entity.HasOne(p => p.CommercialLine)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Products)
                .WithMany()
                .UsingEntity(j => j.ToTable("PromotionProducts"));
        });
    }

    private static void ConfigureSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Token);

            entity.Property(s => s.UserId).IsRequired();

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAuditLogger(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogger>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Username)
                .IsRequired();

            entity.Property(a => a.Action)
                .IsRequired();

            entity.Property(a => a.Time)
                .IsRequired();

            entity.Property(a => a.EntityName)
                .IsRequired(false);

            entity.Property(a => a.EntityId)
                .IsRequired(false);

            entity.Property(a => a.Description)
                .IsRequired(false);
        });
    }
}