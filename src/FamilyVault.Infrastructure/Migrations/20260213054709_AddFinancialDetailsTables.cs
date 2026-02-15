using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialDetailsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomineeName",
                table: "BankAccounts",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DematAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrokerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Depository = table.Column<byte>(type: "TINYINT", nullable: false),
                    DPId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientIdLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    BOId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BOIdLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    HoldingPattern = table.Column<byte>(type: "TINYINT", nullable: false),
                    NomineeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DematAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DematAccounts_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FixedDeposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DepositNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepositNumberLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    DepositType = table.Column<byte>(type: "TINYINT", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MaturityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MaturityAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsAutoRenewal = table.Column<bool>(type: "bit", nullable: false),
                    NomineeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedDeposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedDeposits_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LifeInsurancePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsurerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PolicyNumberLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    PolicyType = table.Column<byte>(type: "TINYINT", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CoverAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremiumFrequency = table.Column<byte>(type: "TINYINT", nullable: false),
                    PolicyStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PolicyEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MaturityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NomineeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Status = table.Column<byte>(type: "TINYINT", nullable: false),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeInsurancePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifeInsurancePolicies_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediclaimPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsurerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PolicyNumberLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    PolicyType = table.Column<byte>(type: "TINYINT", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SumInsured = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PolicyStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PolicyEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TPAName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    HospitalNetworkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "TINYINT", nullable: false),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediclaimPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediclaimPolicies_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MutualFundHoldings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AMCName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FolioNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FolioNumberLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    SchemeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemeType = table.Column<byte>(type: "TINYINT", nullable: false),
                    PlanType = table.Column<byte>(type: "TINYINT", nullable: false),
                    OptionType = table.Column<byte>(type: "TINYINT", nullable: false),
                    InvestmentMode = table.Column<byte>(type: "TINYINT", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    InvestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NomineeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutualFundHoldings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MutualFundHoldings_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediclaimPolicyMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediclaimPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelationshipLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediclaimPolicyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediclaimPolicyMembers_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediclaimPolicyMembers_MediclaimPolicies_MediclaimPolicyId",
                        column: x => x.MediclaimPolicyId,
                        principalTable: "MediclaimPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DematAccounts_CreatedAt",
                table: "DematAccounts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DematAccounts_FamilyMemberId",
                table: "DematAccounts",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedDeposits_CreatedAt",
                table: "FixedDeposits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FixedDeposits_FamilyMemberId",
                table: "FixedDeposits",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeInsurancePolicies_CreatedAt",
                table: "LifeInsurancePolicies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LifeInsurancePolicies_FamilyMemberId",
                table: "LifeInsurancePolicies",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MediclaimPolicies_CreatedAt",
                table: "MediclaimPolicies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MediclaimPolicies_FamilyMemberId",
                table: "MediclaimPolicies",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MediclaimPolicyMembers_FamilyMemberId",
                table: "MediclaimPolicyMembers",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MediclaimPolicyMembers_MediclaimPolicyId_FamilyMemberId",
                table: "MediclaimPolicyMembers",
                columns: new[] { "MediclaimPolicyId", "FamilyMemberId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MutualFundHoldings_CreatedAt",
                table: "MutualFundHoldings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MutualFundHoldings_FamilyMemberId",
                table: "MutualFundHoldings",
                column: "FamilyMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DematAccounts");

            migrationBuilder.DropTable(
                name: "FixedDeposits");

            migrationBuilder.DropTable(
                name: "LifeInsurancePolicies");

            migrationBuilder.DropTable(
                name: "MediclaimPolicyMembers");

            migrationBuilder.DropTable(
                name: "MutualFundHoldings");

            migrationBuilder.DropTable(
                name: "MediclaimPolicies");

            migrationBuilder.DropColumn(
                name: "NomineeName",
                table: "BankAccounts");
        }
    }
}
