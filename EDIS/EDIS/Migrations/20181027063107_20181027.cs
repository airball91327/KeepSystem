using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EDIS.Migrations
{
    public partial class _20181027 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocIdStores",
                columns: table => new
                {
                    DocType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocIdStores", x => new { x.DocType, x.DocId });
                    table.UniqueConstraint("AK_DocIdStores_DocId_DocType", x => new { x.DocId, x.DocType });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocIdStores");
        }
    }
}
