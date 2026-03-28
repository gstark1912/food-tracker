using Microsoft.EntityFrameworkCore;
using App.Api.Models;

namespace App.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DayEntry> DayEntries => Set<DayEntry>();
    public DbSet<MomentEntry> MomentEntries => Set<MomentEntry>();
    public DbSet<WeeklySummary> WeeklySummaries => Set<WeeklySummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DayEntry>(entity =>
        {
            entity.ToTable("day_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<MomentEntry>(entity =>
        {
            entity.ToTable("moment_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasOne(e => e.DayEntry)
                  .WithMany(d => d.Moments)
                  .HasForeignKey(e => e.DayEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeeklySummary>(entity =>
        {
            entity.ToTable("weekly_summaries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => new { e.Year, e.WeekNumber }).IsUnique();
            entity.Property(e => e.CalculatedAt).HasDefaultValueSql("now()");
        });
    }
}
