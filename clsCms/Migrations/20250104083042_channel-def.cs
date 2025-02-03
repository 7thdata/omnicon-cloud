using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clsCms.Migrations
{
    /// <inheritdoc />
    public partial class channeldef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsTopPageStaticPage",
                table: "Channels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopPagePermaName",
                table: "Channels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTopPageStaticPage",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "TopPagePermaName",
                table: "Channels");
        }
    }
}
