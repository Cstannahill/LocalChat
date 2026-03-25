using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Runtime;

public sealed class RuntimeSelectionResolver : IRuntimeSelectionResolver
{
    private readonly IAppRuntimeDefaultsRepository _appRuntimeDefaultsRepository;
    private readonly IModelProfileRepository _modelProfileRepository;
    private readonly IGenerationPresetRepository _generationPresetRepository;

    public RuntimeSelectionResolver(
        IAppRuntimeDefaultsRepository appRuntimeDefaultsRepository,
        IModelProfileRepository modelProfileRepository,
        IGenerationPresetRepository generationPresetRepository)
    {
        _appRuntimeDefaultsRepository = appRuntimeDefaultsRepository;
        _modelProfileRepository = modelProfileRepository;
        _generationPresetRepository = generationPresetRepository;
    }

    public async Task<ResolvedRuntimeSelection> ResolveAsync(
        Agent agent,
        Conversation conversation,
        string? oneTurnOverrideProvider,
        string? oneTurnOverrideModelIdentifier,
        CancellationToken cancellationToken = default)
    {
        var appDefaults = await _appRuntimeDefaultsRepository.GetOrCreateAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(oneTurnOverrideModelIdentifier))
        {
            var defaultProvider = ProviderType.Ollama;

            if (!string.IsNullOrWhiteSpace(oneTurnOverrideProvider)
                && ModelRoute.TryParseProvider(oneTurnOverrideProvider, out var parsedProvider))
            {
                defaultProvider = parsedProvider;
            }

            var normalizedModelIdentifier = ModelRoute.NormalizeForStorage(
                defaultProvider,
                oneTurnOverrideModelIdentifier);

            var parsed = ModelRoute.Parse(normalizedModelIdentifier, defaultProvider);

            var resolvedGenerationPreset = await ResolveGenerationPresetAsync(
                conversation.RuntimeGenerationPresetOverrideId,
                agent.DefaultGenerationPresetId,
                appDefaults.DefaultGenerationPresetId,
                cancellationToken);

            return new ResolvedRuntimeSelection
            {
                SourceType = RuntimeSourceType.OneTurnOverride,
                ProviderType = parsed.Provider,
                ModelIdentifier = normalizedModelIdentifier,
                ModelProfile = null,
                GenerationPreset = resolvedGenerationPreset
            };
        }

        var conversationModelProfile = await ResolveModelProfileAsync(
            conversation.RuntimeModelProfileOverrideId,
            cancellationToken);

        var agentModelProfile = await ResolveModelProfileAsync(
            agent.DefaultModelProfileId,
            cancellationToken);

        var appDefaultModelProfile = await ResolveModelProfileAsync(
            appDefaults.DefaultModelProfileId,
            cancellationToken);

        var conversationGenerationPreset = await ResolveGenerationPresetAsync(
            conversation.RuntimeGenerationPresetOverrideId,
            null,
            null,
            cancellationToken);

        var agentGenerationPreset = await ResolveGenerationPresetAsync(
            agent.DefaultGenerationPresetId,
            null,
            null,
            cancellationToken);

        var appDefaultGenerationPreset = await ResolveGenerationPresetAsync(
            appDefaults.DefaultGenerationPresetId,
            null,
            null,
            cancellationToken);

        if (conversationModelProfile is not null || conversationGenerationPreset is not null)
        {
            return FromProfileAndPreset(
                RuntimeSourceType.ConversationStickyOverride,
                conversationModelProfile,
                conversationGenerationPreset);
        }

        if (agentModelProfile is not null || agentGenerationPreset is not null)
        {
            return FromProfileAndPreset(
                RuntimeSourceType.AgentDefault,
                agentModelProfile,
                agentGenerationPreset);
        }

        if (appDefaultModelProfile is not null || appDefaultGenerationPreset is not null)
        {
            return FromProfileAndPreset(
                RuntimeSourceType.AppDefault,
                appDefaultModelProfile,
                appDefaultGenerationPreset);
        }

        return new ResolvedRuntimeSelection
        {
            SourceType = RuntimeSourceType.ProviderDefault,
            ProviderType = null,
            ModelIdentifier = null,
            ModelProfile = null,
            GenerationPreset = null
        };
    }

    private async Task<ModelProfile?> ResolveModelProfileAsync(
        Guid? modelProfileId,
        CancellationToken cancellationToken)
    {
        if (!modelProfileId.HasValue)
        {
            return null;
        }

        return await _modelProfileRepository.GetByIdAsync(modelProfileId.Value, cancellationToken);
    }

    private async Task<GenerationPreset?> ResolveGenerationPresetAsync(
        Guid? firstChoice,
        Guid? secondChoice,
        Guid? thirdChoice,
        CancellationToken cancellationToken)
    {
        var id = firstChoice ?? secondChoice ?? thirdChoice;
        if (!id.HasValue)
        {
            return null;
        }

        return await _generationPresetRepository.GetByIdAsync(id.Value, cancellationToken);
    }

    private static ResolvedRuntimeSelection FromProfileAndPreset(
        RuntimeSourceType sourceType,
        ModelProfile? modelProfile,
        GenerationPreset? generationPreset)
    {
        return new ResolvedRuntimeSelection
        {
            SourceType = sourceType,
            ProviderType = modelProfile?.ProviderType,
            ModelIdentifier = modelProfile?.ModelIdentifier,
            ModelProfile = modelProfile,
            GenerationPreset = generationPreset
        };
    }
}
