using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace SIMTech.APS.Integration.API.DBContext
{
    using SIMTech.APS.Integration.API.Models;
    public  class IntegrationContext : DbContext
    {
        public IntegrationContext()
        {
        }

        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<InvMas> InvMas { get; set; }
        public virtual DbSet<Pporder> Pporders { get; set; }
        public virtual DbSet<PporderDetail> PporderDetails { get; set; }
        public virtual DbSet<PporderRoute> PporderRoutes { get; set; }
        public virtual DbSet<WorkorderMac> WorkorderMacs { get; set; }

         

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<InvMas>(entity =>
            {
                entity.Property(e => e.CenterId).IsUnicode(false);

                entity.Property(e => e.CreatedBy).IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Customer).IsUnicode(false);

                entity.Property(e => e.InvType)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Location).IsUnicode(false);

                entity.Property(e => e.ReservedWorkOrder).IsUnicode(false);

                entity.Property(e => e.Status).IsUnicode(false);

                entity.Property(e => e.SubWorkOrder).IsUnicode(false);

                entity.Property(e => e.WorkOrder).IsUnicode(false);
            });

            modelBuilder.Entity<Pporder>(entity =>
            {
                entity.Property(e => e.BasicChainType).IsUnicode(false);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.GenDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GoldContent).IsUnicode(false);

                entity.Property(e => e.Size).IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Type)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<PporderDetail>(entity =>
            {
                entity.Property(e => e.Remark).IsUnicode(false);

                entity.Property(e => e.SublotId).IsUnicode(false);

                entity.HasOne(d => d.Pp)
                    .WithMany(p => p.PporderDetails)
                    .HasForeignKey(d => d.Ppid)
                    .HasConstraintName("FK_PPOrderDetail_PPOrder");
            });

            modelBuilder.Entity<PporderRoute>(entity =>
            {
                entity.Property(e => e.AttributeGroup)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MacGroup).HasDefaultValueSql("((0))");

                entity.Property(e => e.MacType)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.MaterialIdList).IsUnicode(false);

                entity.HasOne(d => d.Pp)
                    .WithMany(p => p.PporderRoutes)
                    .HasForeignKey(d => d.Ppid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PPOrderRoute_PPOrder");
            });

            modelBuilder.Entity<WorkorderMac>(entity =>
            {
                entity.Property(e => e.AttributeGroup).IsUnicode(false);

                entity.Property(e => e.MacGroup).HasDefaultValueSql("((0))");

                entity.Property(e => e.MacType)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

        }

    }
}
