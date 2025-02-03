using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clsCms.Migrations
{
    /// <inheritdoc />
    public partial class companytop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginLinks",
                columns: table => new
                {
                    LinkId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Expire = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLinks", x => x.LinkId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginLinks");
        }
    }
}
