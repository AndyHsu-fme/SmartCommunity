using Microsoft.EntityFrameworkCore;
using SmartCommunityApi.Models;
using SmartCommunityApi.Models.Enums;

namespace SmartCommunityApi.Data;

public class SmartCommunityDbContext(DbContextOptions<SmartCommunityDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<VoteTopic> VoteTopics { get; set; }
    public DbSet<VoteStatus> VoteStatuses { get; set; }
    public DbSet<AnonymousBallot> AnonymousBallots { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<FacilityReservation> FacilityReservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UnitNumber).IsRequired().HasMaxLength(20);
            entity.Property(u => u.UserName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.HasIndex(u => u.UnitNumber).IsUnique();
        });

        // ── VoteTopic ─────────────────────────────────────────────────────
        modelBuilder.Entity<VoteTopic>(entity =>
        {
            entity.HasKey(v => v.TopicId);
            entity.Property(v => v.Title).IsRequired().HasMaxLength(200);
            entity.Property(v => v.Description).HasMaxLength(2000);
        });

        // ── VoteStatus ────────────────────────────────────────────────────
        modelBuilder.Entity<VoteStatus>(entity =>
        {
            entity.HasKey(vs => vs.StatusId);

            // Unique constraint: each user can only vote once per topic
            entity.HasIndex(vs => new { vs.TopicId, vs.UserId }).IsUnique();

            entity.HasOne(vs => vs.VoteTopic)
                  .WithMany(t => t.VoteStatuses)
                  .HasForeignKey(vs => vs.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(vs => vs.User)
                  .WithMany(u => u.VoteStatuses)
                  .HasForeignKey(vs => vs.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AnonymousBallot ───────────────────────────────────────────────
        // 刻意不設 UserId 外鍵，確保投票匿名性
        modelBuilder.Entity<AnonymousBallot>(entity =>
        {
            entity.HasKey(b => b.BallotId);
            entity.Property(b => b.OptionSelected).IsRequired().HasMaxLength(200);
            entity.Property(b => b.HashToken).IsRequired().HasMaxLength(256);

            entity.HasOne(b => b.VoteTopic)
                  .WithMany(t => t.AnonymousBallots)
                  .HasForeignKey(b => b.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Package ───────────────────────────────────────────────────────
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(p => p.PackageId);
            entity.Property(p => p.CarrierName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(p => p.User)
                  .WithMany(u => u.Packages)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Facility ──────────────────────────────────────────────────────
        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(f => f.FacilityId);
            entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
        });

        // ── FacilityReservation ───────────────────────────────────────────
        modelBuilder.Entity<FacilityReservation>(entity =>
        {
            entity.HasKey(r => r.ReservationId);
            entity.Property(r => r.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(r => r.Facility)
                  .WithMany(f => f.FacilityReservations)
                  .HasForeignKey(r => r.FacilityId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.FacilityReservations)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
