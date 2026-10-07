using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchManagementMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddChurchProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChurchProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChurchProfiles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ChurchProfiles",
                columns: new[] { "Id", "Address", "Email", "Name", "Phone" },
                values: new object[] { 1, "Enter church address here", "Enter church email here", "ECWA GIGINYU HAUSA SECTION", "Enter church phone here" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChurchProfiles");
        }
    }
}
