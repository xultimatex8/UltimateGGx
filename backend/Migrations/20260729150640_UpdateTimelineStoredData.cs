using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimelineStoredData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gold",
                table: "ParticipantFrames",
                newName: "TotalGold");

            migrationBuilder.AddColumn<int>(
                name: "CurrentGold",
                table: "ParticipantFrames",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minions",
                table: "ParticipantFrames",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ShutdownBounty",
                table: "Events",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Bounty",
                table: "Events",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParticipantId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ParticipantId",
                table: "Events",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Participants_ParticipantId",
                table: "Events",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Participants_ParticipantId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ParticipantId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CurrentGold",
                table: "ParticipantFrames");

            migrationBuilder.DropColumn(
                name: "Minions",
                table: "ParticipantFrames");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "TotalGold",
                table: "ParticipantFrames",
                newName: "Gold");

            migrationBuilder.AlterColumn<int>(
                name: "ShutdownBounty",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Bounty",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
