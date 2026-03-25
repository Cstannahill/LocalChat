namespace LocalChat.Contracts.Authoring;

public sealed class AuthoringFieldTemplateResponse
{
    public required string EntityType { get; init; }

    public required string FieldName { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Content { get; init; }
}
