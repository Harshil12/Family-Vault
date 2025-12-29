using FamilyVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Family> Families { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }
    public DbSet<DocumentDetails> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentDetails>(entity =>
        {
            entity.Property(e => e.DocumentType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");
        });

        modelBuilder.Entity<FamilyMember>(entity =>
        {
            entity.Property(e => e.BloodGroup)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.RelationshipType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");
        });
    }
}
