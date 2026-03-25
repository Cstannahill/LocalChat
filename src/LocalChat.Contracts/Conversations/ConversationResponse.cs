namespace LocalChat.Contracts.Conversations;

public sealed class ConversationResponse
{
    public required Guid Id { get; init; }

    public required Guid AgentId { get; init; }

    public Guid? UserProfileId { get; init; }

    public Guid? RuntimeModelProfileOverrideId { get; init; }

    public Guid? RuntimeGenerationPresetOverrideId { get; init; }

    public Guid? ParentConversationId { get; init; }

    public Guid? BranchedFromMessageId { get; init; }

    public string? DirectorInstructions { get; init; }

    public string? SceneContext { get; init; }

    public bool IsOocModeEnabled { get; init; }

    public required string Title { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required IReadOnlyList<MessageResponse> Messages { get; init; }
}
