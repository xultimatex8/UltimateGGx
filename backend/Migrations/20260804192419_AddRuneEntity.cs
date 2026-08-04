using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRuneEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondaryTree",
                table: "Participants",
                newName: "SecondaryTreeId");

            migrationBuilder.RenameColumn(
                name: "PrimaryRune",
                table: "Participants",
                newName: "PrimaryRuneId");

            migrationBuilder.CreateTable(
                name: "Runes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RiotId = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    IsStyle = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Participants_PrimaryRuneId",
                table: "Participants",
                column: "PrimaryRuneId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SecondaryTreeId",
                table: "Participants",
                column: "SecondaryTreeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Runes_PrimaryRuneId",
                table: "Participants",
                column: "PrimaryRuneId",
                principalTable: "Runes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Runes_SecondaryTreeId",
                table: "Participants",
                column: "SecondaryTreeId",
                principalTable: "Runes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Runes_PrimaryRuneId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Runes_SecondaryTreeId",
                table: "Participants");

            migrationBuilder.DropTable(
                name: "Runes");

            migrationBuilder.DropIndex(
                name: "IX_Participants_PrimaryRuneId",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Participants_SecondaryTreeId",
                table: "Participants");

            migrationBuilder.RenameColumn(
                name: "SecondaryTreeId",
                table: "Participants",
                newName: "SecondaryTree");

            migrationBuilder.RenameColumn(
                name: "PrimaryRuneId",
                table: "Participants",
                newName: "PrimaryRune");
        }
    }
}
