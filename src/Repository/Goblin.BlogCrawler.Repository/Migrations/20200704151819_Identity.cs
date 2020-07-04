using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Goblin.BlogCrawler.Repository.Migrations
{
    public partial class Identity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Post",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedTime = table.Column<DateTimeOffset>(nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(nullable: true),
                    CreatedBy = table.Column<long>(nullable: true),
                    LastUpdatedBy = table.Column<long>(nullable: true),
                    DeletedBy = table.Column<long>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    SiteName = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    AuthorAvatarUrl = table.Column<string>(nullable: true),
                    PublishTime = table.Column<DateTimeOffset>(nullable: false),
                    LastCrawledTime = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Source",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedTime = table.Column<DateTimeOffset>(nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(nullable: true),
                    CreatedBy = table.Column<long>(nullable: true),
                    LastUpdatedBy = table.Column<long>(nullable: true),
                    DeletedBy = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    LastCrawledPostUrl = table.Column<string>(nullable: true),
                    LastCrawlStartTime = table.Column<DateTimeOffset>(nullable: false),
                    LastCrawlEndTime = table.Column<DateTimeOffset>(nullable: false),
                    TotalPostCrawled = table.Column<long>(nullable: false),
                    TimeSpent = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Source", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Post_CreatedTime",
                table: "Post",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Post_DeletedTime",
                table: "Post",
                column: "DeletedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Id",
                table: "Post",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_LastCrawledTime",
                table: "Post",
                column: "LastCrawledTime");

            migrationBuilder.CreateIndex(
                name: "IX_Post_LastUpdatedTime",
                table: "Post",
                column: "LastUpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Post_PublishTime",
                table: "Post",
                column: "PublishTime");

            migrationBuilder.CreateIndex(
                name: "IX_Post_SiteName",
                table: "Post",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Title",
                table: "Post",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Url",
                table: "Post",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_Source_CreatedTime",
                table: "Source",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Source_DeletedTime",
                table: "Source",
                column: "DeletedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Source_Id",
                table: "Source",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Source_LastCrawledPostUrl",
                table: "Source",
                column: "LastCrawledPostUrl");

            migrationBuilder.CreateIndex(
                name: "IX_Source_LastUpdatedTime",
                table: "Source",
                column: "LastUpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Source_Name",
                table: "Source",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Source_Url",
                table: "Source",
                column: "Url");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Post");

            migrationBuilder.DropTable(
                name: "Source");
        }
    }
}
