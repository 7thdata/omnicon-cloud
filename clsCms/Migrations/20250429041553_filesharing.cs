using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clsCms.Migrations
{
    /// <inheritdoc />
    public partial class filesharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileShares",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FileSize = table.Column<int>(type: "int", nullable: false),
                    FileOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileShares", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileShares");
        }
    }
}
