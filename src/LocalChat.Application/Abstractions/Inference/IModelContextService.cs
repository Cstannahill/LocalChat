namespace LocalChat.Application.Abstractions.Inference;

public interface IModelContextService
{
    Task<ModelContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<ModelContextInfo> GetForModelAsync(
        string? modelIdentifier,
        int? contextWindowOverride,
        CancellationToken cancellationToken = default);
}
