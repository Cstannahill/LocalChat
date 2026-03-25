namespace LocalChat.Contracts.Conversations;

public sealed class SelectMessageVariantRequest
{
    public required int VariantIndex { get; init; }
}
