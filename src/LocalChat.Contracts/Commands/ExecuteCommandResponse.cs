namespace LocalChat.Contracts.Commands;

public sealed class ExecuteCommandResponse
{
    public required bool Succeeded { get; init; }

    public required string CommandName { get; init; }

    public required string Message { get; init; }

    public Guid? ConversationId { get; init; }

    public bool ReloadConversation { get; init; }

    public string? DirectorInstructions { get; init; }

    public string? SceneContext { get; init; }

    public bool? IsOocModeEnabled { get; init; }
}
