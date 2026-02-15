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
    /// <summary>
    /// Gets or sets FixedDeposits.
    /// </summary>
    public DbSet<FixedDepositDetails> FixedDeposits { get; set; }
    /// <summary>
    /// Gets or sets LifeInsurancePolicies.
    /// </summary>
    public DbSet<LifeInsurancePolicyDetails> LifeInsurancePolicies { get; set; }
    /// <summary>
    /// Gets or sets MediclaimPolicies.
    /// </summary>
    public DbSet<MediclaimPolicyDetails> MediclaimPolicies { get; set; }
    /// <summary>
    /// Gets or sets MediclaimPolicyMembers.
    /// </summary>
    public DbSet<MediclaimPolicyMember> MediclaimPolicyMembers { get; set; }
    /// <summary>
    /// Gets or sets DematAccounts.
    /// </summary>
    public DbSet<DematAccountDetails> DematAccounts { get; set; }
    /// <summary>
    /// Gets or sets MutualFundHoldings.
    /// </summary>
    public DbSet<MutualFundHoldingDetails> MutualFundHoldings { get; set; }

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

            entity.Property(e => e.NomineeName)
                  .HasMaxLength(150);
        });

        modelBuilder.Entity<FixedDepositDetails>(entity =>
        {
            entity.Property(e => e.DepositType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.InstitutionName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.DepositNumber)
                  .IsRequired();

            entity.Property(e => e.DepositNumberLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.PrincipalAmount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.InterestRate)
                  .HasColumnType("decimal(5,2)");

            entity.Property(e => e.MaturityAmount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.NomineeName)
                  .HasMaxLength(150);
        });

        modelBuilder.Entity<LifeInsurancePolicyDetails>(entity =>
        {
            entity.Property(e => e.PolicyType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.PremiumFrequency)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.Status)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.InsurerName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.PolicyNumber)
                  .IsRequired();

            entity.Property(e => e.PolicyNumberLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.PlanName)
                  .HasMaxLength(200);

            entity.Property(e => e.CoverAmount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.PremiumAmount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.NomineeName)
                  .HasMaxLength(150);

            entity.Property(e => e.AgentName)
                  .HasMaxLength(150);
        });

        modelBuilder.Entity<MediclaimPolicyDetails>(entity =>
        {
            entity.Property(e => e.PolicyType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.Status)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.InsurerName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.PolicyNumber)
                  .IsRequired();

            entity.Property(e => e.PolicyNumberLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.PlanName)
                  .HasMaxLength(200);

            entity.Property(e => e.SumInsured)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.PremiumAmount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TPAName)
                  .HasMaxLength(150);

            entity.Property(e => e.HospitalNetworkUrl)
                  .HasMaxLength(500);
        });

        modelBuilder.Entity<MediclaimPolicyMember>(entity =>
        {
            entity.Property(e => e.RelationshipLabel)
                  .HasMaxLength(50);

            entity.HasOne(e => e.MediclaimPolicy)
                  .WithMany(p => p.CoveredMembers)
                  .HasForeignKey(e => e.MediclaimPolicyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FamilyMember)
                  .WithMany(m => m.MediclaimPolicyMembers)
                  .HasForeignKey(e => e.FamilyMemberId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DematAccountDetails>(entity =>
        {
            entity.Property(e => e.Depository)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.HoldingPattern)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.BrokerName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.DPId)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(e => e.ClientId)
                  .IsRequired();

            entity.Property(e => e.ClientIdLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.BOIdLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.NomineeName)
                  .HasMaxLength(150);
        });

        modelBuilder.Entity<MutualFundHoldingDetails>(entity =>
        {
            entity.Property(e => e.SchemeType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.PlanType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.OptionType)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.InvestmentMode)
                  .HasConversion<byte>()
                  .HasColumnType("TINYINT");

            entity.Property(e => e.AMCName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(e => e.FolioNumber)
                  .IsRequired();

            entity.Property(e => e.FolioNumberLast4)
                  .HasMaxLength(4);

            entity.Property(e => e.SchemeName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Units)
                  .HasColumnType("decimal(18,4)");

            entity.Property(e => e.InvestedAmount)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.CurrentValue)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.NomineeName)
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
        modelBuilder.Entity<FixedDepositDetails>().HasIndex(f => f.FamilyMemberId);
        modelBuilder.Entity<FixedDepositDetails>().HasIndex(f => f.CreatedAt);
        modelBuilder.Entity<LifeInsurancePolicyDetails>().HasIndex(l => l.FamilyMemberId);
        modelBuilder.Entity<LifeInsurancePolicyDetails>().HasIndex(l => l.CreatedAt);
        modelBuilder.Entity<MediclaimPolicyDetails>().HasIndex(m => m.FamilyMemberId);
        modelBuilder.Entity<MediclaimPolicyDetails>().HasIndex(m => m.CreatedAt);
        modelBuilder.Entity<MediclaimPolicyMember>().HasIndex(m => m.FamilyMemberId);
        modelBuilder.Entity<MediclaimPolicyMember>()
            .HasIndex(m => new { m.MediclaimPolicyId, m.FamilyMemberId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        modelBuilder.Entity<DematAccountDetails>().HasIndex(d => d.FamilyMemberId);
        modelBuilder.Entity<DematAccountDetails>().HasIndex(d => d.CreatedAt);
        modelBuilder.Entity<MutualFundHoldingDetails>().HasIndex(m => m.FamilyMemberId);
        modelBuilder.Entity<MutualFundHoldingDetails>().HasIndex(m => m.CreatedAt);
        modelBuilder.Entity<AuditEvent>().HasIndex(a => new { a.UserId, a.CreatedAt });
        modelBuilder.Entity<AuditEvent>().HasIndex(a => new { a.Action, a.EntityType, a.CreatedAt });

        modelBuilder.Entity<User>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Family>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<FamilyMember>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<DocumentDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<BankAccountDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<FixedDepositDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<LifeInsurancePolicyDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<MediclaimPolicyDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<MediclaimPolicyMember>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<DematAccountDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<MutualFundHoldingDetails>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<AuditEvent>().HasQueryFilter(d => !d.IsDeleted);
    }
}
