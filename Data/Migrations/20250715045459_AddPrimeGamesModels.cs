using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PrimeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimeGamesModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "#3B82F6"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "#6B7280"),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvatarPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NewsletterSubscription = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FeaturedImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FeaturedImageAlt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    FavoriteCount = table.Column<int>(type: "int", nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArticleMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleMedia_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTags",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTags", x => new { x.ArticleId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ArticleTags_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ColorCode", "CreatedDate", "Description", "IsActive", "Name", "Slug", "SortOrder" },
                values: new object[,]
                {
                    { 1, "#22C55E", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9289), "ข่าวสารเกมล่าสุด", true, "ข่าวเกม", "news", 1 },
                    { 2, "#3B82F6", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9290), "รีวิวเกมใหม่และเกมน่าสนใจ", true, "รีวิวเกม", "review", 2 },
                    { 3, "#F59E0B", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9291), "ข้อมูลเกมลดราคาและโปรโมชั่น", true, "ดีล/ลดราคา", "deal", 3 },
                    { 4, "#8B5CF6", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9292), "Mods และการดัดแปลงเกม", true, "Mods", "mods", 4 }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "ColorCode", "CreatedDate", "Name", "Slug", "UsageCount" },
                values: new object[,]
                {
                    { 1, "#22C55E", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9403), "Gaming", "gaming", 0 },
                    { 2, "#3B82F6", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9404), "News", "news", 0 },
                    { 3, "#8B5CF6", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9405), "Review", "review", 0 },
                    { 4, "#F59E0B", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9406), "Deal", "deal", 0 },
                    { 5, "#1E40AF", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9407), "Steam", "steam", 0 },
                    { 6, "#1E3A8A", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9408), "PlayStation", "playstation", 0 },
                    { 7, "#16A34A", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9409), "Xbox", "xbox", 0 },
                    { 8, "#DC2626", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9410), "Nintendo", "nintendo", 0 },
                    { 9, "#7C3AED", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9410), "RPG", "rpg", 0 },
                    { 10, "#B45309", new DateTime(2025, 7, 15, 4, 54, 59, 150, DateTimeKind.Utc).AddTicks(9411), "FPS", "fps", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleMedia_ArticleId",
                table: "ArticleMedia",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryId",
                table: "Articles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CreatedDate",
                table: "Articles",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PublishedDate",
                table: "Articles",
                column: "PublishedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug",
                table: "Articles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Status",
                table: "Articles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTags_TagId",
                table: "ArticleTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_ArticleId",
                table: "Favorites",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserProfileId_ArticleId",
                table: "Favorites",
                columns: new[] { "UserProfileId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleMedia");

            migrationBuilder.DropTable(
                name: "ArticleTags");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
