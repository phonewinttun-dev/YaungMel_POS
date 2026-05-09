using Microsoft.EntityFrameworkCore;
using YaungMel_POS.Database.Models;

namespace YaungMel_POS.Database.Data;

public class POSDbContext: DbContext
{
    public POSDbContext(DbContextOptions<POSDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tbl_Product> Products { get; set; }
    public DbSet<Tbl_Category> Categories { get; set; }
    public DbSet<Tbl_Sale> Sales { get; set; }
    public DbSet<Tbl_SaleItem> SaleItems { get; set; }
    public DbSet<Tbl_Summary> Summaries { get; set; }
    public DbSet<Tbl_User> Users { get; set; }
    public DbSet<Tbl_User_Token> UserToken { get; set; }
    public DbSet<Tbl_AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Tbl_Product>().ToTable("Tbl_Product");
        modelBuilder.Entity<Tbl_Category>().ToTable("Tbl_Category");
        modelBuilder.Entity<Tbl_Sale>().ToTable("Tbl_Sale");
        modelBuilder.Entity<Tbl_SaleItem>().ToTable("Tbl_SaleItem");
        modelBuilder.Entity<Tbl_Summary>().ToTable("Tbl_Summary");
        modelBuilder.Entity<Tbl_User_Token>().ToTable("Tbl_User_Token");
        modelBuilder.Entity<Tbl_User>().ToTable("Tbl_User");
        modelBuilder.Entity<Tbl_AuditLog>().ToTable("Tbl_AuditLog");


        // map Tbl_product to Tbl_category
        modelBuilder.Entity<Tbl_Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        //map Tbl_product to Tbl_user
        modelBuilder.Entity<Tbl_Product>()
            .HasOne(p => p.CreatedUser)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.CreatedBy);

        modelBuilder.Entity<Tbl_Product>()
            .HasOne(p => p.UpdatedUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedBy);

        modelBuilder.Entity<Tbl_Product>()
            .Property(p => p.xmin)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        //map Tbl_category to Tbl_user
        modelBuilder.Entity<Tbl_Category>()
            .HasOne(p => p.CreatedUser)
            .WithMany(u => u.Categories)
            .HasForeignKey(p => p.CreatedBy);

        modelBuilder.Entity<Tbl_Category>()
            .HasOne(p => p.UpdatedUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedBy);

        //map Tbl_SaleItem to Tbl_Product
        modelBuilder.Entity<Tbl_SaleItem>()
           .HasOne(s => s.Product)
           .WithMany(p => p.SaleItems)
           .HasForeignKey(s => s.ProductId);

        // map Tbl_User_Token to Tbl_User
        modelBuilder.Entity<Tbl_User_Token>()
            .HasOne(t => t.User)
            .WithMany(u => u.UserToken)
            .HasForeignKey(t => t.UserId);

        //map Tbl_Sale to Tbl_User
        modelBuilder.Entity<Tbl_Sale>()
            .HasOne(s => s.CreatedUser)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.CreatedBy);

        modelBuilder.Entity<Tbl_Sale>()
            .HasOne(s => s.UpdatedUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy);

        modelBuilder.Entity<Tbl_User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        //map Tbl_Summarry to Tbl_Product
        modelBuilder.Entity<Tbl_Summary>()
            .HasOne(s => s.TopSaleProduct)
            .WithMany()
            .HasForeignKey(s => s.TopSaleProductId);

        base.OnModelCreating(modelBuilder);
    }
}
