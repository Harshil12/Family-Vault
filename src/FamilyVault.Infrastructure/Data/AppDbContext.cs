using FamilyVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Data;

/// <summary>
/// Represents AppDbContext.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of AppDbContext.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    /// <summary>
    /// Gets or sets Users.
    /// </summary>
    public DbSet<User> Users { get; set; }
    /// <summary>
    /// Gets or sets Families.
    /// </summary>
    public DbSet<Family> Families { get; set; }
    /// <summary>
    /// Gets or sets FamilyMembers.
    /// </summary>
    public DbSet<FamilyMember> FamilyMembers { get; set; }
    /// <summary>
    /// Gets or sets Documents.
    /// </summary>
    public DbSet<DocumentDetails> Documents { get; set; }
    /// <summary>
    /// Gets or sets BankAccounts.
    /// </summary>
    public DbSet<BankAccountDetails> BankAccounts { get; set; }
    /// <summary>
    /// Gets or sets AuditEvents.
    /// </summary>
    public DbSet<AuditEvent> AuditEvents { get; set; }

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

        modelBuilder.Entity<BankAccountDetails>(entity =>
        {
            entity.Property(e => e.AccountType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.BankName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.AccountNumber)
                  .IsRequired();

            entity.Property(e => e.AccountNumberLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.AccountHolderName)
                  .HasMaxLength(150);

            entity.Property(e => e.IFSC)
                  .HasMaxLength(20);

            entity.Property(e => e.Branch)
                  .HasMaxLength(150);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.Property(e => e.Action)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.EntityType)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Description)
                  .HasMaxLength(500);

            entity.Property(e => e.IpAddress)
                  .HasMaxLength(64);
        });

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Family>().HasIndex(f => new { f.Name, f.UserId }).IsUnique();

        modelBuilder.Entity<FamilyMember>().HasIndex(f => f.PAN).IsUnique();
        modelBuilder.Entity<FamilyMember>().HasIndex(f => f.Aadhar).IsUnique();
        modelBuilder.Entity<FamilyMember>().HasIndex(f => new { f.FirstName, f.LastName, f.FamilyId }).IsUnique();
        modelBuilder.Entity<BankAccountDetails>().HasIndex(b => b.FamilyMemberId);
        modelBuilder.Entity<AuditEvent>().HasIndex(a => new { a.UserId, a.CreatedAt });
        modelBuilder.Entity<AuditEvent>().HasIndex(a => new { a.Action, a.EntityType, a.CreatedAt });

        modelBuilder.Entity<User>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Family>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<FamilyMember>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<DocumentDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<BankAccountDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<AuditEvent>().HasQueryFilter(d => !d.IsDeleted);
    }
}
