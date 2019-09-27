using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EDIS.Data.Migrations
{
    public partial class _126 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_UsersInRolesModel_RoleId_UserId",
                table: "UsersInRolesModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersInRolesModel",
                table: "UsersInRolesModel");

            migrationBuilder.RenameTable(
                name: "UsersInRolesModel",
                newName: "UsersInRoles");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UsersInRoles_RoleId_UserId",
                table: "UsersInRoles",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersInRoles",
                table: "UsersInRoles",
                columns: new[] { "UserId", "RoleId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_UsersInRoles_RoleId_UserId",
                table: "UsersInRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersInRoles",
                table: "UsersInRoles");

            migrationBuilder.RenameTable(
                name: "UsersInRoles",
                newName: "UsersInRolesModel");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UsersInRolesModel_RoleId_UserId",
                table: "UsersInRolesModel",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersInRolesModel",
                table: "UsersInRolesModel",
                columns: new[] { "UserId", "RoleId" });
        }
    }
}
