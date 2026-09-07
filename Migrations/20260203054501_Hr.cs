using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class Hr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId1",
                table: "PayRolls",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HRModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResumePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HRModels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayRolls_EmployeeId1",
                table: "PayRolls",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_HRModels_UserId",
                table: "HRModels",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayRolls_Employees_EmployeeId1",
                table: "PayRolls",
                column: "EmployeeId1",
                principalTable: "Employees",
                principalColumn: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayRolls_Employees_EmployeeId1",
                table: "PayRolls");

            migrationBuilder.DropTable(
                name: "HRModels");

            migrationBuilder.DropIndex(
                name: "IX_PayRolls_EmployeeId1",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "PayRolls");
        }
    }
}
