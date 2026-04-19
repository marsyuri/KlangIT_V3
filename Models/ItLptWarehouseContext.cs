using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Models;

public partial class ItLptWarehouseContext : DbContext
{
    public ItLptWarehouseContext()
    {
    }

    public ItLptWarehouseContext(DbContextOptions<ItLptWarehouseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BorrowHistory> BorrowHistories { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemBrand> ItemBrands { get; set; }

    public virtual DbSet<ItemModel> ItemModels { get; set; }

    public virtual DbSet<ItemType> ItemTypes { get; set; }

    public virtual DbSet<ItemTypeToBrand> ItemTypeToBrands { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NATTLWATTF086\\SQLEXPRESS;Database=IT_LPT_Warehouse;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BorrowHistory>(entity =>
        {
            entity.ToTable("BorrowHistory");

            entity.Property(e => e.BorrowDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ExpectedReturnDate).HasColumnType("datetime");
            entity.Property(e => e.Itstaff)
                .HasMaxLength(1000)
                .HasColumnName("ITStaff");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_BorrowHistory_OrderNo");
            entity.Property(e => e.RequestUser).HasMaxLength(1000);
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");

            entity.HasOne(d => d.Item).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BorrowHistory_Item");

            entity.HasOne(d => d.RequestDepartment).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.RequestDepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BorrowHistory_Department");

            entity.HasOne(d => d.RequestSection).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.RequestSectionId)
                .HasConstraintName("FK_BorrowHistory_Section");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_Department_OrderNo");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");

            entity.Property(e => e.AssetId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AssetId1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AssetId2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AssetId3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AssetId4)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ItemDescription).HasMaxLength(1000);
            entity.Property(e => e.ItemImageUrl).HasMaxLength(1000);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_Item_OrderNo");
            entity.Property(e => e.OtherAssetId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.SerialNo).HasMaxLength(100);

            entity.HasOne(d => d.ItemBrand).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemBrandId)
                .HasConstraintName("FK_Item_ItemBrand");

            entity.HasOne(d => d.ItemModel).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemModelId)
                .HasConstraintName("FK_Item_ItemModel");

            entity.HasOne(d => d.ItemType).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemTypeId)
                .HasConstraintName("FK_Item_ItemType");
        });

        modelBuilder.Entity<ItemBrand>(entity =>
        {
            entity.ToTable("ItemBrand");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_ItemBrand_OrderNo");
        });

        modelBuilder.Entity<ItemModel>(entity =>
        {
            entity.ToTable("ItemModel");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_ItemModel_OrderNo");

            entity.HasOne(d => d.ItemBrand).WithMany(p => p.ItemModels)
                .HasForeignKey(d => d.ItemBrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemModel_ItemBrand");
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.ToTable("ItemType");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_ItemType_OrderNo");
        });

        modelBuilder.Entity<ItemTypeToBrand>(entity =>
        {
            entity.ToTable("ItemTypeToBrand");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("Tont", "DF_ItemTypeToBrand_CreatedBy");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValue(new DateTime(2026, 4, 7, 9, 17, 53, 920, DateTimeKind.Unspecified), "DF_ItemTypeToBrand_CreatedDate")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("Tont", "DF_ItemTypeToBrand_ModifiedBy");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValue(new DateTime(2026, 4, 7, 9, 17, 53, 920, DateTimeKind.Unspecified), "DF_ItemTypeToBrand_ModifiedDate")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ItemBrand).WithMany(p => p.ItemTypeToBrands)
                .HasForeignKey(d => d.ItemBrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemTypeToBrand_ItemBrand");

            entity.HasOne(d => d.ItemType).WithMany(p => p.ItemTypeToBrands)
                .HasForeignKey(d => d.ItemTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemTypeToBrand_ItemType");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.ToTable("Section");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_Section_OrderNo");

            entity.HasOne(d => d.Department).WithMany(p => p.Sections)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Section_Department");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
