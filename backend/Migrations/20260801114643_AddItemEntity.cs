using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddItemEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Items",
                table: "Participants");

            migrationBuilder.AddColumn<int>(
                name: "AfterItemId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BeforeItemId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BuyPrice = table.Column<int>(type: "integer", nullable: false),
                    SellPrice = table.Column<int>(type: "integer", nullable: false),
                    Stats = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantItems",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantItems", x => new { x.ItemsId, x.ParticipantsId });
                    table.ForeignKey(
                        name: "FK_ParticipantItems_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantItems_Participants_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_AfterItemId",
                table: "Events",
                column: "AfterItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_BeforeItemId",
                table: "Events",
                column: "BeforeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ItemId",
                table: "Events",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Key",
                table: "Items",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantItems_ParticipantsId",
                table: "ParticipantItems",
                column: "ParticipantsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Items_AfterItemId",
                table: "Events",
                column: "AfterItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Items_BeforeItemId",
                table: "Events",
                column: "BeforeItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Items_ItemId",
                table: "Events",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Items_AfterItemId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Items_BeforeItemId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Items_ItemId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "ParticipantItems");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Events_AfterItemId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_BeforeItemId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ItemId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AfterItemId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BeforeItemId",
                table: "Events");

            migrationBuilder.AddColumn<List<int>>(
                name: "Items",
                table: "Participants",
                type: "integer[]",
                nullable: false);
        }
    }
}
