namespace LocalChat.Contracts.Characters;

public sealed class CharacterSampleDialogueResponse
{
    public required Guid Id { get; init; }

    public required string UserMessage { get; init; }

    public required string AssistantMessage { get; init; }

    public required int SortOrder { get; init; }
}
