namespace LocalChat.Application.Features.Commands;

public sealed class ParsedCommand
{
    public required CommandType Type { get; init; }

    public required string CommandName { get; init; }

    public string? Argument { get; init; }

    public string? RawText { get; init; }
}
