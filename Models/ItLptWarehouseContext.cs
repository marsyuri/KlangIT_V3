using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KlangIT_V3.Models;

public partial class ItLptWarehouseContext : DbContext
{
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

    public virtual DbSet<StockLog> StockLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BorrowHistory>(entity =>
        {
            entity.ToTable("BorrowHistory");

            entity.Property(e => e.BorrowDate).HasColumnType("datetime");
            entity.Property(e => e.BorrowItname)
                .HasMaxLength(1000)
                .HasColumnName("BorrowITName");
            entity.Property(e => e.BorrowTel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BorrowerUser).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_BorrowHistory_OrderNo");
            entity.Property(e => e.ReferenceNo).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");
            entity.Property(e => e.ReturnItname)
                .HasMaxLength(1000)
                .HasColumnName("ReturnITName");

            entity.HasOne(d => d.BorrowerDepartment).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.BorrowerDepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BorrowHistory_Department");

            entity.HasOne(d => d.BorrowerSection).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.BorrowerSectionId)
                .HasConstraintName("FK_BorrowHistory_Section");

            entity.HasOne(d => d.Item).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BorrowHistory_Item");
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

            entity.HasIndex(e => e.AssetId, "IX_Item_AssetId").HasFilter("([AssetId] IS NOT NULL AND [IsDeleted]=(0))");

            entity.HasIndex(e => e.ItemBrandId, "IX_Item_ItemBrandId");

            entity.HasIndex(e => new { e.ItemStatus, e.IsDeleted }, "IX_Item_ItemStatus_IsDeleted");

            entity.HasIndex(e => e.ItemTypeId, "IX_Item_ItemTypeId").HasFilter("([ItemTypeId] IS NOT NULL)");

            entity.HasIndex(e => e.SerialNo, "IX_Item_SerialNo").HasFilter("([SerialNo] IS NOT NULL AND [IsDeleted]=(0))");

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
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())", "DF_Item_CreatedDate")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemDescription).HasMaxLength(1000);
            entity.Property(e => e.ItemImageUrl).HasMaxLength(1000);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())", "DF_Item_ModifiedDate")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderNo).HasDefaultValue(1, "DF_Item_OrderNo");
            entity.Property(e => e.OtherAssetId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.SerialNo).HasMaxLength(100);

            entity.HasOne(d => d.ItemBrand).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemBrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_ItemBrand");

            entity.HasOne(d => d.ItemModel).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemModelId)
                .HasConstraintName("FK_Item_ItemModel");

            entity.HasOne(d => d.ItemType).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
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

        modelBuilder.Entity<StockLog>(entity =>
        {
            entity.ToTable("StockLog");

            entity.HasIndex(e => new { e.ItemId, e.CreatedDate }, "IX_StockLog_ItemId_CreatedDate").IsDescending(false, true);

            entity.HasIndex(e => e.LogType, "IX_StockLog_LogType");

            entity.HasIndex(e => e.ReferenceNo, "IX_StockLog_ReferenceNo").HasFilter("([ReferenceNo] IS NOT NULL)");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ReferenceNo).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(d => d.Item).WithMany(p => p.StockLogs)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockLog_Item");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
