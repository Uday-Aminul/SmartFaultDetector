using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public partial class SmartFaultDetectorDbContext : DbContext
{
    public SmartFaultDetectorDbContext(DbContextOptions<SmartFaultDetectorDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Reading> Readings { get; set; }

    public virtual DbSet<TrainingSample> TrainingSamples { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reading>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Readings__3214EC07805A0498");

            entity.ToTable("Readings", "Prediction");

            entity.Property(e => e.PredictedLabel)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<TrainingSample>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Training__3214EC07BA0C8661");

            entity.ToTable("TrainingSamples", "Training");

            entity.Property(e => e.Label)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
