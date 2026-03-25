namespace LocalChat.Contracts.Admin;

public sealed class RetrievalReindexAllResponse
{
    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public required int ReindexedDocumentCount { get; init; }
}
