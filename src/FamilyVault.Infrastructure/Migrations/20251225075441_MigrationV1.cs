using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyVault.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MigrationV1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Mobile = table.Column<long>(type: "bigint", maxLength: 10, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Families",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Families", x => x.Id);
                table.ForeignKey(
                    name: "FK_Families_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FamilyMembers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Mobile = table.Column<long>(type: "bigint", maxLength: 10, nullable: true),
                RelationshipType = table.Column<int>(type: "int", nullable: false),
                DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                BloodGroup = table.Column<int>(type: "int", nullable: true),
                PAN = table.Column<long>(type: "bigint", maxLength: 10, nullable: true),
                Aadhar = table.Column<long>(type: "bigint", maxLength: 12, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                table.ForeignKey(
                    name: "FK_FamilyMembers_Families_FamilyId",
                    column: x => x.FamilyId,
                    principalTable: "Families",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SavedLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Documents", x => x.Id);
                table.ForeignKey(
                    name: "FK_Documents_FamilyMembers_FamilyMemberId",
                    column: x => x.FamilyMemberId,
                    principalTable: "FamilyMembers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Documents_FamilyMemberId",
            table: "Documents",
            column: "FamilyMemberId");

        migrationBuilder.CreateIndex(
            name: "IX_Families_UserId",
            table: "Families",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_FamilyMembers_FamilyId",
            table: "FamilyMembers",
            column: "FamilyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Documents");

        migrationBuilder.DropTable(
            name: "FamilyMembers");

        migrationBuilder.DropTable(
            name: "Families");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
