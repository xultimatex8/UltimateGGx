using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Champions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Roles = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Champions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<string>(type: "text", nullable: false),
                    EndOfGameResult = table.Column<string>(type: "text", nullable: false),
                    GameDuration = table.Column<long>(type: "bigint", nullable: false),
                    GameEndTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    QueueType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Summoners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Puuid = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ProfileIconId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Summoners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SummonerSpells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummonerSpells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Win = table.Column<bool>(type: "boolean", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Tier = table.Column<string>(type: "text", nullable: false),
                    Rank = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    SummonerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Queues_Summoners_SummonerId",
                        column: x => x.SummonerId,
                        principalTable: "Summoners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    ChampionLevel = table.Column<int>(type: "integer", nullable: false),
                    Deaths = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    Items = table.Column<List<int>>(type: "integer[]", nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: false),
                    Lane = table.Column<string>(type: "text", nullable: false),
                    PrimaryRune = table.Column<int>(type: "integer", nullable: false),
                    SecondaryTree = table.Column<int>(type: "integer", nullable: false),
                    DamageToChampions = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    SummonerId = table.Column<int>(type: "integer", nullable: false),
                    ChampionId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Champions_ChampionId",
                        column: x => x.ChampionId,
                        principalTable: "Champions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Summoners_SummonerId",
                        column: x => x.SummonerId,
                        principalTable: "Summoners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Bounty = table.Column<int>(type: "integer", nullable: false),
                    ShutdownBounty = table.Column<int>(type: "integer", nullable: false),
                    MonsterType = table.Column<string>(type: "text", nullable: true),
                    MonsterSubType = table.Column<string>(type: "text", nullable: true),
                    BuildingType = table.Column<string>(type: "text", nullable: true),
                    LaneType = table.Column<string>(type: "text", nullable: true),
                    TowerType = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    KillerParticipantId = table.Column<int>(type: "integer", nullable: true),
                    VictimParticipantId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Participants_KillerParticipantId",
                        column: x => x.KillerParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Participants_VictimParticipantId",
                        column: x => x.VictimParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantFrames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantFrames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantFrames_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantSummonerSpells",
                columns: table => new
                {
                    ParticipantsId = table.Column<int>(type: "integer", nullable: false),
                    SummonerSpellsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantSummonerSpells", x => new { x.ParticipantsId, x.SummonerSpellsId });
                    table.ForeignKey(
                        name: "FK_ParticipantSummonerSpells_Participants_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantSummonerSpells_SummonerSpells_SummonerSpellsId",
                        column: x => x.SummonerSpellsId,
                        principalTable: "SummonerSpells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventAssists",
                columns: table => new
                {
                    AssistedId = table.Column<int>(type: "integer", nullable: false),
                    AssistingParticipantsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAssists", x => new { x.AssistedId, x.AssistingParticipantsId });
                    table.ForeignKey(
                        name: "FK_EventAssists_Events_AssistedId",
                        column: x => x.AssistedId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAssists_Participants_AssistingParticipantsId",
                        column: x => x.AssistingParticipantsId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Champions_Key",
                table: "Champions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAssists_AssistingParticipantsId",
                table: "EventAssists",
                column: "AssistingParticipantsId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_KillerParticipantId",
                table: "Events",
                column: "KillerParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MatchId",
                table: "Events",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VictimParticipantId",
                table: "Events",
                column: "VictimParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MatchId",
                table: "Matches",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantFrames_ParticipantId",
                table: "ParticipantFrames",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ChampionId",
                table: "Participants",
                column: "ChampionId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SummonerId",
                table: "Participants",
                column: "SummonerId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_TeamId",
                table: "Participants",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantSummonerSpells_SummonerSpellsId",
                table: "ParticipantSummonerSpells",
                column: "SummonerSpellsId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_SummonerId",
                table: "Queues",
                column: "SummonerId");

            migrationBuilder.CreateIndex(
                name: "IX_Summoners_Puuid",
                table: "Summoners",
                column: "Puuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SummonerSpells_Key",
                table: "SummonerSpells",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_MatchId",
                table: "Teams",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventAssists");

            migrationBuilder.DropTable(
                name: "ParticipantFrames");

            migrationBuilder.DropTable(
                name: "ParticipantSummonerSpells");

            migrationBuilder.DropTable(
                name: "Queues");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "SummonerSpells");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Champions");

            migrationBuilder.DropTable(
                name: "Summoners");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
