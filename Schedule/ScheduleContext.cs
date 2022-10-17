using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace SIMTech.APS.Scheduling.API.DBContext
{
    using SIMTech.APS.Scheduling.API.Models;
    public  class ScheduleContext : DbContext
    {
        public ScheduleContext()
        {
        }

        public ScheduleContext(DbContextOptions<ScheduleContext> options)
            : base(options)
        {
        }

        public virtual DbSet<RescheduleAlert> RescheduleAlerts { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<ScheduleDetail> ScheduleDetails { get; set; }      


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<RescheduleAlert>(entity =>
            {
                entity.Property(e => e.AlertType)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .IsFixedLength(true);

            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedOn).HasDefaultValueSql("(getdate())");

            });

            modelBuilder.Entity<ScheduleDetail>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ScheduleType)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.Schedule)
                    .WithMany(p => p.ScheduleDetails)
                    .HasForeignKey(d => d.ScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ScheduleDetail_Schedule");
            });
         
        }

    }
}
