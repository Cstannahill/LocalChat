using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_20260315 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenerationPresets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Temperature = table.Column<double>(type: "REAL", nullable: false),
                    TopP = table.Column<double>(type: "REAL", nullable: false),
                    RepeatPenalty = table.Column<double>(type: "REAL", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    StopSequencesText = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lorebooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lorebooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProviderType = table.Column<string>(type: "TEXT", nullable: false),
                    ModelIdentifier = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ContextWindow = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetrievalChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetrievalChunks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPersonas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Traits = table.Column<string>(type: "TEXT", nullable: false),
                    Preferences = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalInstructions = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPersonas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoreEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LorebookId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoreEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoreEntries_Lorebooks_LorebookId",
                        column: x => x.LorebookId,
                        principalTable: "Lorebooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Greeting = table.Column<string>(type: "TEXT", nullable: false),
                    PersonalityDefinition = table.Column<string>(type: "TEXT", nullable: false),
                    Scenario = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultModelProfileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultGenerationPresetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultTtsVoice = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_GenerationPresets_DefaultGenerationPresetId",
                        column: x => x.DefaultGenerationPresetId,
                        principalTable: "GenerationPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Characters_ModelProfiles_DefaultModelProfileId",
                        column: x => x.DefaultModelProfileId,
                        principalTable: "ModelProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSampleDialogues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserMessage = table.Column<string>(type: "TEXT", nullable: false),
                    AssistantMessage = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSampleDialogues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSampleDialogues_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserPersonaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BranchedFromMessageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    DirectorInstructions = table.Column<string>(type: "TEXT", nullable: true),
                    DirectorInstructionsUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SceneContext = table.Column<string>(type: "TEXT", nullable: true),
                    SceneContextUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsOocModeEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_Conversations_ParentConversationId",
                        column: x => x.ParentConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Conversations_UserPersonas_UserPersonaId",
                        column: x => x.UserPersonaId,
                        principalTable: "UserPersonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ImageGenerationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PromptText = table.Column<string>(type: "TEXT", nullable: false),
                    NegativePromptText = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Steps = table.Column<int>(type: "INTEGER", nullable: false),
                    Cfg = table.Column<double>(type: "REAL", nullable: false),
                    Seed = table.Column<long>(type: "INTEGER", nullable: false),
                    ProviderJobId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageGenerationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageGenerationJobs_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageGenerationJobs_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    IsPinned = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDerived = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "REAL", nullable: true),
                    ReviewStatus = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemoryItems_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemoryItems_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    SequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SelectedVariantIndex = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SummaryCheckpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartSequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    EndSequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SummaryText = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReplacedByCheckpointId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryCheckpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummaryCheckpoints_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedImageAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImageGenerationJobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelativeUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedImageAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedImageAssets_ImageGenerationJobs_ImageGenerationJobId",
                        column: x => x.ImageGenerationJobId,
                        principalTable: "ImageGenerationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VariantIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageVariants_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeechClips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Voice = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModelIdentifier = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ResponseFormat = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RelativeUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SourceText = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeechClips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeechClips_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeechClips_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeechClips_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_DefaultGenerationPresetId",
                table: "Characters",
                column: "DefaultGenerationPresetId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_DefaultModelProfileId",
                table: "Characters",
                column: "DefaultModelProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Name",
                table: "Characters",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSampleDialogues_CharacterId_SortOrder",
                table: "CharacterSampleDialogues",
                columns: new[] { "CharacterId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_BranchedFromMessageId",
                table: "Conversations",
                column: "BranchedFromMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CharacterId",
                table: "Conversations",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ParentConversationId",
                table: "Conversations",
                column: "ParentConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UpdatedAt",
                table: "Conversations",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserPersonaId",
                table: "Conversations",
                column: "UserPersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedImageAssets_ImageGenerationJobId",
                table: "GeneratedImageAssets",
                column: "ImageGenerationJobId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationPresets_Name",
                table: "GenerationPresets",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGenerationJobs_CharacterId",
                table: "ImageGenerationJobs",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGenerationJobs_ConversationId",
                table: "ImageGenerationJobs",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGenerationJobs_Status",
                table: "ImageGenerationJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Lorebooks_CharacterId",
                table: "Lorebooks",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_LoreEntries_IsEnabled",
                table: "LoreEntries",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_LoreEntries_LorebookId",
                table: "LoreEntries",
                column: "LorebookId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_CharacterId",
                table: "MemoryItems",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_ConversationId",
                table: "MemoryItems",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_IsPinned",
                table: "MemoryItems",
                column: "IsPinned");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryItems_ReviewStatus",
                table: "MemoryItems",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_SequenceNumber",
                table: "Messages",
                columns: new[] { "ConversationId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageVariants_MessageId_VariantIndex",
                table: "MessageVariants",
                columns: new[] { "MessageId", "VariantIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelProfiles_Name",
                table: "ModelProfiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RetrievalChunks_CharacterId",
                table: "RetrievalChunks",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_RetrievalChunks_ConversationId",
                table: "RetrievalChunks",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_RetrievalChunks_SourceType_SourceEntityId",
                table: "RetrievalChunks",
                columns: new[] { "SourceType", "SourceEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeechClips_CharacterId",
                table: "SpeechClips",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechClips_ConversationId",
                table: "SpeechClips",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechClips_MessageId",
                table: "SpeechClips",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryCheckpoints_ConversationId_EndSequenceNumber",
                table: "SummaryCheckpoints",
                columns: new[] { "ConversationId", "EndSequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPersonas_Name",
                table: "UserPersonas",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterSampleDialogues");

            migrationBuilder.DropTable(
                name: "GeneratedImageAssets");

            migrationBuilder.DropTable(
                name: "LoreEntries");

            migrationBuilder.DropTable(
                name: "MemoryItems");

            migrationBuilder.DropTable(
                name: "MessageVariants");

            migrationBuilder.DropTable(
                name: "RetrievalChunks");

            migrationBuilder.DropTable(
                name: "SpeechClips");

            migrationBuilder.DropTable(
                name: "SummaryCheckpoints");

            migrationBuilder.DropTable(
                name: "ImageGenerationJobs");

            migrationBuilder.DropTable(
                name: "Lorebooks");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "UserPersonas");

            migrationBuilder.DropTable(
                name: "GenerationPresets");

            migrationBuilder.DropTable(
                name: "ModelProfiles");
        }
    }
}
