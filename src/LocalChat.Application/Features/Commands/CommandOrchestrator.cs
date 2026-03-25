using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Chat;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Commands;

public sealed class CommandOrchestrator
{
    private readonly SlashCommandParser _parser;
    private readonly IConversationRepository _conversationRepository;
    private readonly ChatOrchestrator _chatOrchestrator;

    public CommandOrchestrator(
        SlashCommandParser parser,
        IConversationRepository conversationRepository,
        ChatOrchestrator chatOrchestrator)
    {
        _parser = parser;
        _conversationRepository = conversationRepository;
        _chatOrchestrator = chatOrchestrator;
    }

    public async Task<CommandExecutionResult> ExecuteAsync(
        Guid? conversationId,
        string commandText,
        CancellationToken cancellationToken = default)
    {
        var parsed = _parser.Parse(commandText);

        return parsed.Type switch
        {
            CommandType.Help => ExecuteHelp(),
            CommandType.DirectorSet => await ExecuteDirectorSetAsync(conversationId, parsed.Argument!, cancellationToken),
            CommandType.DirectorShow => await ExecuteDirectorShowAsync(conversationId, cancellationToken),
            CommandType.DirectorClear => await ExecuteDirectorClearAsync(conversationId, cancellationToken),
            CommandType.SceneSet => await ExecuteSceneSetAsync(conversationId, parsed.Argument!, cancellationToken),
            CommandType.SceneShow => await ExecuteSceneShowAsync(conversationId, cancellationToken),
            CommandType.SceneClear => await ExecuteSceneClearAsync(conversationId, cancellationToken),
            CommandType.OocOn => await ExecuteOocSetAsync(conversationId, true, cancellationToken),
            CommandType.OocOff => await ExecuteOocSetAsync(conversationId, false, cancellationToken),
            CommandType.OocToggle => await ExecuteOocToggleAsync(conversationId, cancellationToken),
            CommandType.OocShow => await ExecuteOocShowAsync(conversationId, cancellationToken),
            CommandType.Reroll => await ExecuteRerollAsync(conversationId, cancellationToken),
            _ => new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = parsed.CommandName,
                Message = "Unknown command. Use /help to see available commands.",
                ConversationId = conversationId,
                ReloadConversation = false
            }
        };
    }

    private static CommandExecutionResult ExecuteHelp() =>
        new()
        {
            Succeeded = true,
            CommandName = "help",
            Message = SlashCommandParser.HelpText(),
            ReloadConversation = false
        };

    private async Task<CommandExecutionResult> ExecuteDirectorSetAsync(
        Guid? conversationId,
        string instructions,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "director",
                Message = "Director instructions require an active conversation.",
                ReloadConversation = false
            };
        }

        if (string.IsNullOrWhiteSpace(instructions))
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "director",
                Message = "Director instructions cannot be empty.",
                ConversationId = conversationId,
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        conversation.DirectorInstructions = instructions.Trim();
        conversation.DirectorInstructionsUpdatedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "director",
            $"Director instructions updated:\n\n{conversation.DirectorInstructions}",
            reloadConversation: true);
    }

    private async Task<CommandExecutionResult> ExecuteDirectorShowAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "director",
                Message = "Director instructions require an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        var message = string.IsNullOrWhiteSpace(conversation.DirectorInstructions)
            ? "No director instructions are currently set for this conversation."
            : $"Current director instructions:\n\n{conversation.DirectorInstructions}";

        return BuildConversationStateResult(
            conversation,
            "director",
            message,
            reloadConversation: false);
    }

    private async Task<CommandExecutionResult> ExecuteDirectorClearAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "director",
                Message = "Director instructions require an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        conversation.DirectorInstructions = null;
        conversation.DirectorInstructionsUpdatedAt = null;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "director",
            "Director instructions cleared.",
            reloadConversation: true);
    }

    private async Task<CommandExecutionResult> ExecuteSceneSetAsync(
        Guid? conversationId,
        string sceneContext,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "scene",
                Message = "Scene context requires an active conversation.",
                ReloadConversation = false
            };
        }

        if (string.IsNullOrWhiteSpace(sceneContext))
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "scene",
                Message = "Scene context cannot be empty.",
                ConversationId = conversationId,
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        conversation.SceneContext = sceneContext.Trim();
        conversation.SceneContextUpdatedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "scene",
            $"Scene context updated:\n\n{conversation.SceneContext}",
            reloadConversation: true);
    }

    private async Task<CommandExecutionResult> ExecuteSceneShowAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "scene",
                Message = "Scene context requires an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        var message = string.IsNullOrWhiteSpace(conversation.SceneContext)
            ? "No scene context is currently set for this conversation."
            : $"Current scene context:\n\n{conversation.SceneContext}";

        return BuildConversationStateResult(
            conversation,
            "scene",
            message,
            reloadConversation: false);
    }

    private async Task<CommandExecutionResult> ExecuteSceneClearAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "scene",
                Message = "Scene context requires an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        conversation.SceneContext = null;
        conversation.SceneContextUpdatedAt = null;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "scene",
            "Scene context cleared.",
            reloadConversation: true);
    }

    private async Task<CommandExecutionResult> ExecuteOocSetAsync(
        Guid? conversationId,
        bool enabled,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "ooc",
                Message = "OOC mode requires an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        conversation.IsOocModeEnabled = enabled;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "ooc",
            enabled ? "OOC mode enabled." : "OOC mode disabled.",
            reloadConversation: true);
    }

    private async Task<CommandExecutionResult> ExecuteOocToggleAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "ooc",
                Message = "OOC mode requires an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        conversation.IsOocModeEnabled = !conversation.IsOocModeEnabled;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "ooc",
            conversation.IsOocModeEnabled ? "OOC mode enabled." : "OOC mode disabled.",
            reloadConversation: true);
    }

    private async Task<CommandExecutionResult> ExecuteOocShowAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "ooc",
                Message = "OOC mode requires an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        return BuildConversationStateResult(
            conversation,
            "ooc",
            conversation.IsOocModeEnabled ? "OOC mode is currently enabled." : "OOC mode is currently disabled.",
            reloadConversation: false);
    }

    private async Task<CommandExecutionResult> ExecuteRerollAsync(
        Guid? conversationId,
        CancellationToken cancellationToken)
    {
        if (!conversationId.HasValue)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "reroll",
                Message = "Reroll requires an active conversation.",
                ReloadConversation = false
            };
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

        var latestAssistantMessage = conversation.Messages
            .Where(x => x.Role == MessageRole.Assistant)
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefault();

        if (latestAssistantMessage is null)
        {
            return new CommandExecutionResult
            {
                Succeeded = false,
                CommandName = "reroll",
                Message = "No assistant message exists yet to reroll.",
                ConversationId = conversation.Id,
                ReloadConversation = false
            };
        }

        await _chatOrchestrator.RegenerateLatestAssistantMessageAsync(
            conversation.Id,
            latestAssistantMessage.Id,
            cancellationToken: cancellationToken);

        return BuildConversationStateResult(
            conversation,
            "reroll",
            "Latest assistant message regenerated.",
            reloadConversation: true);
    }

    private static CommandExecutionResult BuildConversationStateResult(
        Domain.Entities.Conversations.Conversation conversation,
        string commandName,
        string message,
        bool reloadConversation) =>
        new()
        {
            Succeeded = true,
            CommandName = commandName,
            Message = message,
            ConversationId = conversation.Id,
            ReloadConversation = reloadConversation,
            DirectorInstructions = conversation.DirectorInstructions,
            SceneContext = conversation.SceneContext,
            IsOocModeEnabled = conversation.IsOocModeEnabled
        };
}
