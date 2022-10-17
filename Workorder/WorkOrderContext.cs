using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace SIMTech.APS.WorkOrder.API.DBContext
{
    using SIMTech.APS.WorkOrder.API.Models;
    public  class WorkOrderContext : DbContext
    {
        
        public WorkOrderContext()
        {
        }

        public WorkOrderContext(DbContextOptions<WorkOrderContext> options)
            : base(options)
        {
        }

        public virtual DbSet<WorkOrder> WorkOrders { get; set; }
        public virtual DbSet<WorkOrderAdjustment> WorkOrderAdjustments { get; set; }
        public virtual DbSet<WorkOrderCost> WorkOrderCosts { get; set; }
        public virtual DbSet<WorkOrderDetail> WorkOrderDetails { get; set; }
        public virtual DbSet<WorkOrderMaterial> WorkOrderMaterials { get; set; }
        public virtual DbSet<WIP1> WIPs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<WorkOrder>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            });

            modelBuilder.Entity<WorkOrderAdjustment>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .IsFixedLength(true);

            });

            modelBuilder.Entity<WorkOrderCost>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");


            });

            modelBuilder.Entity<WorkOrderDetail>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.WorkOrder)
                    .WithMany(p => p.WorkOrderDetails)
                    .HasForeignKey(d => d.WorkOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__WorkOrder__WorkO__09746778");
            });

            modelBuilder.Entity<WorkOrderMaterial>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.WorkOrder)
                    .WithMany(p => p.WorkOrderMaterials)
                    .HasForeignKey(d => d.WorkOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WorkOrderMaterial_WorkOrder");
            });

            modelBuilder.Entity<WIP1>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            });

        }


    }
}
