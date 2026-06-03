using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SSQHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "主键ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicalNO = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "开奖期号"),
                    DrawDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "开奖日期"),
                    OutBallOrder = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "出球顺序（按摇出顺序排列的号码）"),
                    RedBalls = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "红球号码（逗号分隔，按大小顺序）"),
                    BlueBall = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "蓝球号码"),
                    CreatedTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSQHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SSQHistories_CreatedTime",
                table: "SSQHistories",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_SSQHistories_DrawDate",
                table: "SSQHistories",
                column: "DrawDate");

            migrationBuilder.CreateIndex(
                name: "IX_SSQHistories_PeriodicalNO",
                table: "SSQHistories",
                column: "PeriodicalNO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SSQHistories");
        }
    }
}
