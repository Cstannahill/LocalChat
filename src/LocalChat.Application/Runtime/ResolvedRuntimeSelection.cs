using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Runtime;

public sealed class ResolvedRuntimeSelection
{
    public required RuntimeSourceType SourceType { get; init; }

    public ProviderType? ProviderType { get; init; }

    public string? ModelIdentifier { get; init; }

    public ModelProfile? ModelProfile { get; init; }

    public GenerationPreset? GenerationPreset { get; init; }
}
