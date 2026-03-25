using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Speech;
using LocalChat.Domain.Entities.Audio;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Speech;

public sealed class SpeechOrchestrator
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ISpeechClipRepository _speechClipRepository;
    private readonly ISpeechSynthesisProvider _speechSynthesisProvider;
    private readonly ISpeechFileStore _speechFileStore;

    public SpeechOrchestrator(
        IConversationRepository conversationRepository,
        ISpeechClipRepository speechClipRepository,
        ISpeechSynthesisProvider speechSynthesisProvider,
        ISpeechFileStore speechFileStore)
    {
        _conversationRepository = conversationRepository;
        _speechClipRepository = speechClipRepository;
        _speechSynthesisProvider = speechSynthesisProvider;
        _speechFileStore = speechFileStore;
    }

    public async Task<SpeechClip> SynthesizeMessageAsync(
        Guid messageId,
        string? voiceOverride,
        string? modelOverride,
        double? speedOverride,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByMessageIdWithMessagesAsync(
            messageId,
            cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException($"Message '{messageId}' was not found.");
        }

        var message = conversation.Messages.FirstOrDefault(x => x.Id == messageId);
        if (message is null)
        {
            throw new InvalidOperationException($"Message '{messageId}' was not found.");
        }

        if (message.Role != MessageRole.Assistant)
        {
            throw new InvalidOperationException("Only assistant messages can be synthesized to speech.");
        }

        var synthesis = await _speechSynthesisProvider.SynthesizeAsync(
            new SpeechSynthesisRequest
            {
                Input = message.Content,
                Voice = !string.IsNullOrWhiteSpace(voiceOverride)
                    ? voiceOverride.Trim()
                    : conversation.Character?.DefaultTtsVoice,
                ModelIdentifier = string.IsNullOrWhiteSpace(modelOverride)
                    ? null
                    : modelOverride.Trim(),
                ResponseFormat = null,
                Speed = speedOverride
            },
            cancellationToken);

        var relativeUrl = await _speechFileStore.SaveAsync(
            synthesis.AudioBytes,
            synthesis.ResponseFormat,
            cancellationToken);

        var clip = new SpeechClip
        {
            Id = Guid.NewGuid(),
            CharacterId = conversation.CharacterId,
            ConversationId = conversation.Id,
            MessageId = message.Id,
            Provider = "Kokoro",
            Voice = synthesis.EffectiveVoice,
            ModelIdentifier = synthesis.EffectiveModelIdentifier,
            ResponseFormat = synthesis.ResponseFormat,
            ContentType = synthesis.ContentType,
            RelativeUrl = relativeUrl,
            SourceText = message.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _speechClipRepository.AddAsync(clip, cancellationToken);
        await _speechClipRepository.SaveChangesAsync(cancellationToken);

        return clip;
    }

    public Task<IReadOnlyList<SpeechClip>> ListByMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return _speechClipRepository.ListByMessageAsync(messageId, cancellationToken);
    }
}
