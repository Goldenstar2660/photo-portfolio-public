using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoExifFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Aperture",
                table: "AppPhotos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CameraModel",
                table: "AppPhotos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FocalLength",
                table: "AppPhotos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Iso",
                table: "AppPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShutterSpeed",
                table: "AppPhotos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aperture",
                table: "AppPhotos");

            migrationBuilder.DropColumn(
                name: "CameraModel",
                table: "AppPhotos");

            migrationBuilder.DropColumn(
                name: "FocalLength",
                table: "AppPhotos");

            migrationBuilder.DropColumn(
                name: "Iso",
                table: "AppPhotos");

            migrationBuilder.DropColumn(
                name: "ShutterSpeed",
                table: "AppPhotos");
        }
    }
}
