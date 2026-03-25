namespace LocalChat.Application.Authoring;

public interface IAuthoringAssistantService
{
    Task<IReadOnlyList<AuthoringFieldTemplate>> GetTemplatesAsync(
        string entityType,
        string? fieldName = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthoringStarterPack>> GetStarterPacksAsync(
        CancellationToken cancellationToken = default);

    Task<AuthoringEnhancementResult> EnhanceAsync(
        AuthoringEnhancementInput input,
        CancellationToken cancellationToken = default);

    Task<FullAuthoringBundleGenerationResult> GenerateFullBundleFromBriefAsync(
        FullAuthoringBundleGenerationInput input,
        CancellationToken cancellationToken = default);

    Task<AuthoringConsistencyCheckResult> CheckConsistencyAsync(
        AuthoringConsistencyCheckInput input,
        CancellationToken cancellationToken = default);

    Task<AuthoringEnhancementResult> RepairConsistencyIssueAsync(
        ConsistencyIssueRepairInput input,
        CancellationToken cancellationToken = default);
}
