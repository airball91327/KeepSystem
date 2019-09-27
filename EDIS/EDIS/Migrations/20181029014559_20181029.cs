using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EDIS.Migrations
{
    public partial class _20181029 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    BuildingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BuildingName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.BuildingId);
                });

            migrationBuilder.CreateTable(
                name: "Floors",
                columns: table => new
                {
                    BuildingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FloorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Flg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FloorName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Floors", x => new { x.BuildingId, x.FloorId });
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    BuildingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FloorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlaceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Flg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => new { x.BuildingId, x.FloorId, x.PlaceId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "Floors");

            migrationBuilder.DropTable(
                name: "Places");
        }
    }
}
