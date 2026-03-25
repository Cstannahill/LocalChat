namespace LocalChat.Application.Abstractions.Retrieval;

public sealed class RetrievalAdminStats
{
    public required int ConversationCount { get; init; }

    public required int MessageCount { get; init; }

    public required int AcceptedMemoryCount { get; init; }

    public required int ProposedMemoryCount { get; init; }

    public required int EnabledLoreEntryCount { get; init; }

    public required int RetrievalChunkCount { get; init; }
}
