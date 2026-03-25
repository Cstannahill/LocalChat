namespace LocalChat.Application.Features.Commands;

public sealed class SlashCommandParser
{
    public ParsedCommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new ParsedCommand
            {
                Type = CommandType.Unknown,
                CommandName = "unknown",
                RawText = input,
            };
        }

        var trimmed = input.Trim();

        if (!trimmed.StartsWith('/'))
        {
            return new ParsedCommand
            {
                Type = CommandType.Unknown,
                CommandName = "unknown",
                RawText = input,
            };
        }

        var body = trimmed[1..].Trim();

        if (
            string.Equals(body, "help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(body, "commands", StringComparison.OrdinalIgnoreCase)
        )
        {
            return new ParsedCommand
            {
                Type = CommandType.Help,
                CommandName = "help",
                RawText = input,
            };
        }

        if (string.Equals(body, "reroll", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand
            {
                Type = CommandType.Reroll,
                CommandName = "reroll",
                RawText = input,
            };
        }

        if (body.StartsWith("director", StringComparison.OrdinalIgnoreCase))
        {
            var remainder = body["director".Length..].Trim();

            if (
                string.IsNullOrWhiteSpace(remainder)
                || string.Equals(remainder, "show", StringComparison.OrdinalIgnoreCase)
            )
            {
                return new ParsedCommand
                {
                    Type = CommandType.DirectorShow,
                    CommandName = "director",
                    RawText = input,
                };
            }

            if (string.Equals(remainder, "clear", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand
                {
                    Type = CommandType.DirectorClear,
                    CommandName = "director",
                    RawText = input,
                };
            }

            return new ParsedCommand
            {
                Type = CommandType.DirectorSet,
                CommandName = "director",
                Argument = remainder,
                RawText = input,
            };
        }

        if (body.StartsWith("scene", StringComparison.OrdinalIgnoreCase))
        {
            var remainder = body["scene".Length..].Trim();

            if (
                string.IsNullOrWhiteSpace(remainder)
                || string.Equals(remainder, "show", StringComparison.OrdinalIgnoreCase)
            )
            {
                return new ParsedCommand
                {
                    Type = CommandType.SceneShow,
                    CommandName = "scene",
                    RawText = input,
                };
            }

            if (string.Equals(remainder, "clear", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand
                {
                    Type = CommandType.SceneClear,
                    CommandName = "scene",
                    RawText = input,
                };
            }

            return new ParsedCommand
            {
                Type = CommandType.SceneSet,
                CommandName = "scene",
                Argument = remainder,
                RawText = input,
            };
        }

        if (body.StartsWith("ooc", StringComparison.OrdinalIgnoreCase))
        {
            var remainder = body["ooc".Length..].Trim();

            if (
                string.IsNullOrWhiteSpace(remainder)
                || string.Equals(remainder, "show", StringComparison.OrdinalIgnoreCase)
            )
            {
                return new ParsedCommand
                {
                    Type = CommandType.OocShow,
                    CommandName = "ooc",
                    RawText = input,
                };
            }

            if (string.Equals(remainder, "on", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand
                {
                    Type = CommandType.OocOn,
                    CommandName = "ooc",
                    RawText = input,
                };
            }

            if (string.Equals(remainder, "off", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand
                {
                    Type = CommandType.OocOff,
                    CommandName = "ooc",
                    RawText = input,
                };
            }

            if (string.Equals(remainder, "toggle", StringComparison.OrdinalIgnoreCase))
            {
                return new ParsedCommand
                {
                    Type = CommandType.OocToggle,
                    CommandName = "ooc",
                    RawText = input,
                };
            }
        }

        return new ParsedCommand
        {
            Type = CommandType.Unknown,
            CommandName = "unknown",
            RawText = input,
        };
    }

    public static string HelpText() =>
        """
            Available commands:

            /help
              Show available commands.

            /director <instructions>
              Set persistent out-of-band director instructions for the active conversation.

            /director show
              Show the current director instructions for the active conversation.

            /director clear
              Remove the current director instructions from the active conversation.

            /scene <context>
              Set the current scene context for the active conversation.

            /scene show
              Show the current scene context.

            /scene clear
              Clear the current scene context.

            /ooc on
                        Enable out-of-character mode.

            /ooc off
                        Disable out-of-character mode.

            /ooc toggle
                        Toggle out-of-character mode.

            /ooc show
              Show whether OOC mode is active.

            /reroll
              Regenerate the latest assistant message in the active conversation.
            """;
}
