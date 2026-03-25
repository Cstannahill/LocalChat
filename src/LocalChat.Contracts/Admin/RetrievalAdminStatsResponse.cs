namespace LocalChat.Contracts.Admin;

public sealed class RetrievalAdminStatsResponse
{
    public required int ConversationCount { get; init; }

    public required int MessageCount { get; init; }

    public required int AcceptedMemoryCount { get; init; }

    public required int ProposedMemoryCount { get; init; }

    public required int EnabledLoreEntryCount { get; init; }

    public required int RetrievalChunkCount { get; init; }
}
