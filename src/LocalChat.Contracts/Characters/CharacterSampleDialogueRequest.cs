namespace LocalChat.Contracts.Characters;

public sealed class CharacterSampleDialogueRequest
{
    public required string UserMessage { get; init; }

    public required string AssistantMessage { get; init; }
}
