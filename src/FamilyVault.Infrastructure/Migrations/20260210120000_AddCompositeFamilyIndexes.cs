using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeFamilyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Families_Name",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_FirstName_LastName",
                table: "FamilyMembers");

            migrationBuilder.CreateIndex(
                name: "IX_Families_Name_UserId",
                table: "Families",
                columns: new[] { "Name", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_FirstName_LastName_FamilyId",
                table: "FamilyMembers",
                columns: new[] { "FirstName", "LastName", "FamilyId" },
                unique: true,
                filter: "[LastName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Families_Name_UserId",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_FirstName_LastName_FamilyId",
                table: "FamilyMembers");

            migrationBuilder.CreateIndex(
                name: "IX_Families_Name",
                table: "Families",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_FirstName_LastName",
                table: "FamilyMembers",
                columns: new[] { "FirstName", "LastName" },
                unique: true,
                filter: "[LastName] IS NOT NULL");
        }
    }
}
