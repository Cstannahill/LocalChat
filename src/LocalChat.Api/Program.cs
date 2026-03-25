using System.Net.Http.Headers;
using LocalChat.Api.Endpoints;
using LocalChat.Api.Middleware;
using LocalChat.Api.Streaming;
using LocalChat.Api.Telemetry;
using LocalChat.Application.Abstractions.ImageGeneration;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Abstractions.Speech;
using LocalChat.Application.Abstractions.Telemetry;
using LocalChat.Application.Authoring;
using LocalChat.Application.Background;
using LocalChat.Application.Chat;
using LocalChat.Application.Features.Commands;
using LocalChat.Application.Features.Memory;
using LocalChat.Application.Features.Summaries;
using LocalChat.Application.ImageGeneration;
using LocalChat.Application.Inspection;
using LocalChat.Application.Memory;
using LocalChat.Application.Options;
using LocalChat.Application.Runtime;
using LocalChat.Application.Speech;
using LocalChat.Infrastructure.BackgroundJobs;
using LocalChat.Infrastructure.ImageGeneration;
using LocalChat.Infrastructure.ImageGeneration.ComfyUi;
using LocalChat.Infrastructure.Inference;
using LocalChat.Infrastructure.Inference.HuggingFace;
using LocalChat.Infrastructure.Inference.LlamaCpp;
using LocalChat.Infrastructure.Inference.Ollama;
using LocalChat.Infrastructure.Inference.OpenRouter;
using LocalChat.Infrastructure.Inference.Tokenization;
using LocalChat.Infrastructure.Options;
using LocalChat.Infrastructure.Persistence;
using LocalChat.Infrastructure.Persistence.Repositories;
using LocalChat.Infrastructure.Persistence.Seed;
using LocalChat.Infrastructure.Retrieval;
using LocalChat.Infrastructure.Retrieval.Admin;
using LocalChat.Infrastructure.Retrieval.Ranking;
using LocalChat.Infrastructure.Retrieval.VectorStores;
using LocalChat.Infrastructure.Speech;
using LocalChat.Infrastructure.Speech.Kokoro;
using LocalChat.Infrastructure.Speech.Qwen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<RequestFlowLoggingOptions>(
    builder.Configuration.GetSection(RequestFlowLoggingOptions.SectionName));
builder.Services.AddSingleton<RequestFlowLogWriter>();
builder.Services.AddScoped<IRequestFlowTiming, RequestFlowTiming>();

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);

var defaultConnectionString = $"Data Source={Path.Combine(dataDirectory, "localchat.db")}";
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ?? defaultConnectionString;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection(OllamaOptions.SectionName)
);

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<OllamaOptions>>().Value);
builder.Services.Configure<OpenRouterOptions>(
    builder.Configuration.GetSection(OpenRouterOptions.SectionName)
);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<OpenRouterOptions>>().Value);
builder.Services.Configure<HuggingFaceOptions>(
    builder.Configuration.GetSection(HuggingFaceOptions.SectionName)
);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<HuggingFaceOptions>>().Value);
builder.Services.Configure<LlamaCppOptions>(
    builder.Configuration.GetSection(LlamaCppOptions.SectionName)
);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<LlamaCppOptions>>().Value);

var summaryOptions =
    builder.Configuration.GetSection(SummaryOptions.SectionName).Get<SummaryOptions>()
    ?? new SummaryOptions();

builder.Services.AddSingleton(summaryOptions);

var retrievalOptions =
    builder.Configuration.GetSection(RetrievalOptions.SectionName).Get<RetrievalOptions>()
    ?? new RetrievalOptions();

builder.Services.AddSingleton(retrievalOptions);

var memoryProposalOptions =
    builder.Configuration.GetSection("MemoryProposals").Get<MemoryProposalOptions>()
    ?? new MemoryProposalOptions();

builder.Services.AddSingleton(memoryProposalOptions);

var sceneStateCleanupOptions =
    builder.Configuration.GetSection("SceneStateCleanup").Get<SceneStateCleanupOptions>()
    ?? new SceneStateCleanupOptions();

builder.Services.AddSingleton(sceneStateCleanupOptions);

var inspectionOptions =
    builder.Configuration.GetSection(InspectionOptions.SectionName).Get<InspectionOptions>()
    ?? new InspectionOptions();

builder.Services.AddSingleton(inspectionOptions);

var ollamaOptions = builder.Configuration
    .GetSection(OllamaOptions.SectionName)
    .Get<OllamaOptions>()
    ?? new OllamaOptions();
var normalizedOllamaBaseUrl = ollamaOptions.BaseUrl.EndsWith('/')
    ? ollamaOptions.BaseUrl
    : $"{ollamaOptions.BaseUrl}/";

var kokoroTtsOptions = builder.Configuration.GetSection("KokoroTts").Get<KokoroTtsOptions>() ?? new KokoroTtsOptions();
builder.Services.AddSingleton(kokoroTtsOptions);
var qwenTtsOptions = builder.Configuration.GetSection("QwenTts").Get<QwenTtsOptions>() ?? new QwenTtsOptions();
builder.Services.AddSingleton(qwenTtsOptions);
var speechProviderOptions =
    builder.Configuration.GetSection(SpeechProviderOptions.SectionName).Get<SpeechProviderOptions>()
    ?? new SpeechProviderOptions();
builder.Services.AddSingleton(speechProviderOptions);

var comfyUiOptions = builder.Configuration.GetSection("ComfyUi").Get<ComfyUiOptions>() ?? new ComfyUiOptions();
builder.Services.AddSingleton(comfyUiOptions);
var backgroundMemoryProposalOptions =
    builder.Configuration.GetSection("BackgroundMemoryProposals").Get<BackgroundMemoryProposalOptions>()
    ?? new BackgroundMemoryProposalOptions();
builder.Services.AddSingleton(backgroundMemoryProposalOptions);

var conversationBackgroundWorkOptions =
    builder.Configuration.GetSection("ConversationBackgroundWork").Get<LocalChat.Infrastructure.Options.ConversationBackgroundWorkOptions>()
    ?? new LocalChat.Infrastructure.Options.ConversationBackgroundWorkOptions();
builder.Services.AddSingleton(conversationBackgroundWorkOptions);
builder.Services.AddSingleton(new LocalChat.Application.Background.ConversationBackgroundWorkOptions
{
    Enabled = conversationBackgroundWorkOptions.Enabled,
    PollIntervalMilliseconds = conversationBackgroundWorkOptions.PollIntervalMilliseconds,
    RetrievalDebounceMilliseconds = conversationBackgroundWorkOptions.RetrievalDebounceMilliseconds,
    MemoryDebounceMilliseconds = conversationBackgroundWorkOptions.MemoryDebounceMilliseconds,
    SummaryDebounceMilliseconds = conversationBackgroundWorkOptions.SummaryDebounceMilliseconds,
    SummaryMinMessagesBeforeRefresh = conversationBackgroundWorkOptions.SummaryMinMessagesBeforeRefresh,
    SummaryRecentMessagesToKeepRaw = conversationBackgroundWorkOptions.SummaryRecentMessagesToKeepRaw,
    SummaryMinNewMessagesSinceLastRefresh = conversationBackgroundWorkOptions.SummaryMinNewMessagesSinceLastRefresh,
    SummaryMaxMessagesInPrompt = conversationBackgroundWorkOptions.SummaryMaxMessagesInPrompt
});

builder.Services.AddHttpClient<OllamaInferenceProvider>(client =>
{
    client.BaseAddress = new Uri(normalizedOllamaBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ollamaOptions.TimeoutSeconds);
});

builder.Services.AddHttpClient<OpenRouterInferenceProvider>();
builder.Services.AddHttpClient<HuggingFaceInferenceProvider>();
builder.Services.AddHttpClient<LlamaCppInferenceProvider>();
builder.Services.AddScoped<OpenRouterModelContextService>();
builder.Services.AddScoped<HuggingFaceModelContextService>();
builder.Services.AddScoped<OllamaModelContextService>();
builder.Services.AddScoped<LlamaCppModelContextService>();

builder.Services.AddHttpClient<OllamaEmbeddingProvider>(client =>
{
    client.BaseAddress = new Uri(normalizedOllamaBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ollamaOptions.TimeoutSeconds);
});

builder.Services.AddHttpClient<OllamaModelInfoClient>(client =>
{
    client.BaseAddress = new Uri(normalizedOllamaBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ollamaOptions.TimeoutSeconds);
});

builder.Services.AddHttpClient<OllamaHttpClient>(client =>
{
    client.BaseAddress = new Uri(normalizedOllamaBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(ollamaOptions.TimeoutSeconds);
});

builder.Services.AddHttpClient<KokoroSpeechSynthesisProvider>(client =>
{
    client.BaseAddress = new Uri(kokoroTtsOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(kokoroTtsOptions.TimeoutSeconds);

    if (!string.IsNullOrWhiteSpace(kokoroTtsOptions.ApiKey))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", kokoroTtsOptions.ApiKey);
    }
});
builder.Services.AddHttpClient<QwenSpeechSynthesisProvider>(client =>
{
    client.BaseAddress = new Uri(qwenTtsOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(qwenTtsOptions.TimeoutSeconds);

    if (!string.IsNullOrWhiteSpace(qwenTtsOptions.ApiKey))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", qwenTtsOptions.ApiKey);
    }
});

builder.Services.AddHttpClient<ComfyUiImageGenerationProvider>(client =>
{
    client.BaseAddress = new Uri(comfyUiOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(comfyUiOptions.TimeoutSeconds);
});

builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IUserPersonaRepository, UserPersonaRepository>();
builder.Services.AddScoped<IModelProfileRepository, ModelProfileRepository>();
builder.Services.AddScoped<IGenerationPresetRepository, GenerationPresetRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IAppRuntimeDefaultsRepository, AppRuntimeDefaultsRepository>();
builder.Services.AddScoped<IGenerationPromptSnapshotRepository, GenerationPromptSnapshotRepository>();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<IMemoryOperationAuditService, MemoryOperationAuditService>();
builder.Services.AddScoped<ISceneStateExtractionEventRepository, SceneStateExtractionEventRepository>();
builder.Services.AddScoped<IMemoryExtractionAuditEventRepository, MemoryExtractionAuditEventRepository>();
builder.Services.AddScoped<ILorebookRepository, LorebookRepository>();
builder.Services.AddScoped<ISpeechClipRepository, SpeechClipRepository>();
builder.Services.AddScoped<IImageGenerationJobRepository, ImageGenerationJobRepository>();
builder.Services.AddScoped<IAuthoringAssistantService, AuthoringAssistantService>();

builder.Services.AddScoped<
    IPromptComposer,
    LocalChat.Application.Prompting.Composition.PromptComposer
>();
builder.Services.AddScoped<IInferenceProvider, RoutedInferenceProvider>();
builder.Services.AddScoped<IEmbeddingProvider, OllamaEmbeddingProvider>();
builder.Services.AddScoped<ITokenEstimator, BasicTokenEstimator>();
builder.Services.AddScoped<IModelContextService, RoutedModelContextService>();
builder.Services.AddScoped<IPromptInspectionService, PromptInspectionService>();
builder.Services.AddScoped<ISceneStateInspectionService, SceneStateInspectionService>();
builder.Services.AddScoped<IMemoryExtractionInspectionService, MemoryExtractionInspectionService>();
builder.Services.AddScoped<ISceneStateCleanupService, SceneStateCleanupService>();
builder.Services.AddScoped<IMemoryPolicyService, MemoryPolicyService>();
builder.Services.AddScoped<MemoryProposalQualityEvaluator>();
builder.Services.AddScoped<MemoryExtractionClassifier>();
builder.Services.AddScoped<IMemoryProposalService, MemoryProposalService>();
builder.Services.AddScoped<IConversationSummaryRefreshService, ConversationSummaryRefreshService>();
builder.Services.AddScoped<IVectorStore, SqliteBruteForceVectorStore>();
builder.Services.AddScoped<RetrievalRanker>();
builder.Services.AddScoped<VectorIndexingService>();
builder.Services.AddScoped<IConversationRetrievalSyncService, ConversationRetrievalSyncService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddScoped<IRetrievalAdminService, RetrievalAdminService>();
builder.Services.AddScoped<IFullRetrievalReindexService, FullRetrievalReindexService>();
builder.Services.AddScoped<IMemoryMaintenanceService, MemoryMaintenanceService>();
builder.Services.AddScoped<IConversationSummaryService, ConversationSummaryService>();
builder.Services.AddScoped<SlashCommandParser>();
builder.Services.AddScoped<CommandOrchestrator>();
builder.Services.AddScoped<IRuntimeSelectionResolver, RuntimeSelectionResolver>();
builder.Services.AddScoped<ISpeechSynthesisProvider>(sp =>
{
    var selectedProvider = sp.GetRequiredService<SpeechProviderOptions>().Provider;
    return string.Equals(selectedProvider, "Kokoro", StringComparison.OrdinalIgnoreCase)
        ? sp.GetRequiredService<KokoroSpeechSynthesisProvider>()
        : sp.GetRequiredService<QwenSpeechSynthesisProvider>();
});
builder.Services.AddScoped<ISpeechFileStore, LocalSpeechFileStore>();
builder.Services.AddScoped<SpeechOrchestrator>();
builder.Services.AddScoped<IImageGenerationProvider>(sp =>
    sp.GetRequiredService<ComfyUiImageGenerationProvider>());
builder.Services.AddScoped<IGeneratedImageFileStore, LocalGeneratedImageFileStore>();
builder.Services.AddScoped<ImageGenerationOrchestrator>();
builder.Services.AddScoped<ConversationVisualPromptService>();
builder.Services.AddScoped<UserMessageSuggestionService>();
builder.Services.AddScoped<ChatOrchestrator>();
builder.Services.AddScoped<IAssistantTurnGenerationService, AssistantTurnGenerationService>();
builder.Services.AddScoped<ConversationMessageMutationService>();
builder.Services.AddScoped<ConversationContinuationService>();
builder.Services.AddSingleton<SseChatStreamWriter>();
builder.Services.AddSingleton<BackgroundMemoryProposalState>();
builder.Services.AddSingleton<BackgroundMemoryProposalCoordinator>();
builder.Services.AddHostedService<BackgroundMemoryProposalWorker>();
builder.Services.AddSingleton<ConversationBackgroundWorkQueue>();
builder.Services.AddScoped<ConversationBackgroundWorkExecutor>();
builder.Services.AddSingleton<ConversationBackgroundWorkProcessor>();
builder.Services.AddSingleton<IConversationBackgroundWorkScheduler>(sp =>
    sp.GetRequiredService<ConversationBackgroundWorkQueue>());
builder.Services.AddHostedService<ConversationBackgroundWorkHostedService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureLegacyBaselineMigrationAsync(dbContext);
    await dbContext.Database.MigrateAsync();

    await DefaultModelProfileSeeder.SeedAsync(dbContext);
    await DefaultGenerationPresetSeeder.SeedAsync(dbContext);
    await DefaultCharacterSeeder.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCharactersEndpoints();
app.MapPersonasEndpoints();
app.MapModelProfilesEndpoints();
app.MapConversationsEndpoints();
app.MapAppRuntimeDefaultsEndpoints();
app.MapGenerationPromptSnapshotEndpoints();
app.MapMemoryEndpoints();
app.MapMemoryAdminEndpoints();
app.MapLorebookEndpoints();
app.MapInspectionEndpoints();
app.MapAuthoringAssistantEndpoints();
app.MapSceneStateInspectionEndpoints();
app.MapMemoryExtractionInspectionEndpoints();
app.MapImportExportEndpoints();
app.MapCommandsEndpoints();
app.MapTtsEndpoints();
app.MapImageGenerationEndpoints();
app.MapAdminEndpoints();
app.MapAdminMaintenanceEndpoints();
app.MapBackgroundWorkEndpoints();
app.MapChatEndpoints();
app.MapSuggestedUserMessageEndpoints();
app.MapContinueChatEndpoints();

app.Run();

static async Task EnsureLegacyBaselineMigrationAsync(ApplicationDbContext dbContext)
{
    const string baselineMigrationId = "20260315174509_Baseline_20260315";
    const string baselineProductVersion = "9.0.0";

    var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
    if (appliedMigrations.Any())
    {
        return;
    }

    var hasGenerationPresetsTable = await TableExistsAsync(dbContext, "GenerationPresets");
    var hasCharactersTable = await TableExistsAsync(dbContext, "Characters");
    if (!hasGenerationPresetsTable || !hasCharactersTable)
    {
        return;
    }

    var hasHistoryTable = await TableExistsAsync(dbContext, "__EFMigrationsHistory");
    if (!hasHistoryTable)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            """
        );
    }

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ({0}, {1});
        """,
        baselineMigrationId,
        baselineProductVersion
    );
}

static async Task<bool> TableExistsAsync(ApplicationDbContext dbContext, string tableName)
{
    var connection = dbContext.Database.GetDbConnection();
    var shouldClose = connection.State != System.Data.ConnectionState.Open;
    if (shouldClose)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var scalar = await command.ExecuteScalarAsync();
        return scalar is long count && count > 0;
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }
}





