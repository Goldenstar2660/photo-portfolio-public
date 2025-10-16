using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumsAndPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAlbums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAlbums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    DateTaken = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPhotos_AppAlbums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "AppAlbums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAlbums_DisplayOrder",
                table: "AppAlbums",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlbums_Name",
                table: "AppAlbums",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAlbums_Topic",
                table: "AppAlbums",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_AppPhotos_AlbumId",
                table: "AppPhotos",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPhotos_AlbumId_DisplayOrder",
                table: "AppPhotos",
                columns: new[] { "AlbumId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhotos_DateTaken",
                table: "AppPhotos",
                column: "DateTaken");

            migrationBuilder.CreateIndex(
                name: "IX_AppPhotos_DisplayOrder",
                table: "AppPhotos",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AppPhotos_Location",
                table: "AppPhotos",
                column: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPhotos");

            migrationBuilder.DropTable(
                name: "AppAlbums");
        }
    }
}
