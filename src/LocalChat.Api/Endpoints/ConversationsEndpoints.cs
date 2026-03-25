using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Background;
using LocalChat.Application.Chat;
using LocalChat.Contracts.Conversations;
using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Api.Endpoints;

public static class ConversationsEndpoints
{
    public static IEndpointRouteBuilder MapConversationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conversations")
            .WithTags("Conversations");

        group.MapPost("/", async (
            CreateConversationRequest request,
            IConversationRepository conversationRepository,
            ICharacterRepository characterRepository,
            IUserPersonaRepository userPersonaRepository,
            CancellationToken cancellationToken) =>
        {
            var character = await characterRepository.GetByIdWithDetailsAsync(request.CharacterId, cancellationToken);
            if (character is null)
            {
                return Results.BadRequest(new { error = $"Character '{request.CharacterId}' was not found." });
            }

            if (request.UserPersonaId.HasValue)
            {
                var persona = await userPersonaRepository.GetByIdAsync(request.UserPersonaId.Value, cancellationToken);
                if (persona is null)
                {
                    return Results.BadRequest(new { error = $"User persona '{request.UserPersonaId.Value}' was not found." });
                }
            }

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                Character = character,
                UserPersonaId = request.UserPersonaId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? "New Conversation" : request.Title.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            ConversationMessageSeeder.SeedGreetingIfNeeded(conversation);

            await conversationRepository.AddAsync(conversation, cancellationToken);
            await conversationRepository.SaveChangesAsync(cancellationToken);

            var created = await conversationRepository.GetByIdWithMessagesAsync(conversation.Id, cancellationToken);
            return Results.Ok(ToConversationResponse(created!));
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IConversationRepository repository,
            CancellationToken cancellationToken) =>
        {
            var conversation = await repository.GetByIdWithMessagesAsync(id, cancellationToken);
            if (conversation is null)
            {
                return Results.NotFound();
            }

            var seeded = ConversationMessageSeeder.SeedGreetingIfNeeded(conversation);
            if (seeded)
            {
                await repository.SaveChangesAsync(cancellationToken);
                conversation = await repository.GetByIdWithMessagesAsync(id, cancellationToken);
                if (conversation is null)
                {
                    return Results.NotFound();
                }
            }

            return Results.Ok(ToConversationResponse(conversation));
        });

        group.MapGet("/by-character/{characterId:guid}", async (
            Guid characterId,
            IConversationRepository repository,
            CancellationToken cancellationToken) =>
        {
            var conversations = await repository.ListByCharacterAsync(characterId, cancellationToken);

            var response = conversations
                .Select(x => new ConversationResponse
                {
                    Id = x.Id,
                    CharacterId = x.CharacterId,
                    UserPersonaId = x.UserPersonaId,
                    RuntimeModelProfileOverrideId = x.RuntimeModelProfileOverrideId,
                    RuntimeGenerationPresetOverrideId = x.RuntimeGenerationPresetOverrideId,
                    ParentConversationId = x.ParentConversationId,
                    BranchedFromMessageId = x.BranchedFromMessageId,
                    DirectorInstructions = x.DirectorInstructions,
                    SceneContext = x.SceneContext,
                    IsOocModeEnabled = x.IsOocModeEnabled,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Messages = Array.Empty<MessageResponse>()
                })
                .ToList();

            return Results.Ok(response);
        });

        group.MapPut("/{conversationId:guid}/messages/{messageId:guid}", async (
            Guid conversationId,
            Guid messageId,
            UpdateConversationMessageRequest request,
            ConversationMessageMutationService mutationService,
            CancellationToken cancellationToken) =>
        {
            var result = await mutationService.EditAsync(
                conversationId,
                messageId,
                request.Content,
                request.RegenerateAssistant,
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/{conversationId:guid}/messages/{messageId:guid}/delete", async (
            Guid conversationId,
            Guid messageId,
            DeleteConversationMessageRequest request,
            ConversationMessageMutationService mutationService,
            CancellationToken cancellationToken) =>
        {
            var result = await mutationService.DeleteAsync(
                conversationId,
                messageId,
                request.RegenerateAssistant,
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/branch/{messageId:guid}", async (
            Guid id,
            Guid messageId,
            IConversationRepository repository,
            IConversationBackgroundWorkScheduler scheduler,
            CancellationToken cancellationToken) =>
        {
            var sourceConversation = await repository.GetByIdWithMessagesAsync(id, cancellationToken);
            if (sourceConversation is null)
            {
                return Results.NotFound();
            }

            var orderedMessages = sourceConversation.Messages
                .OrderBy(x => x.SequenceNumber)
                .ToList();

            var branchPoint = orderedMessages.FirstOrDefault(x => x.Id == messageId);
            if (branchPoint is null)
            {
                return Results.BadRequest(new { error = $"Message '{messageId}' was not found in the source conversation." });
            }

            var messagesToCopy = orderedMessages
                .Where(x => x.SequenceNumber <= branchPoint.SequenceNumber)
                .OrderBy(x => x.SequenceNumber)
                .ToList();

            var branchedConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                CharacterId = sourceConversation.CharacterId,
                UserPersonaId = sourceConversation.UserPersonaId,
                ParentConversationId = sourceConversation.Id,
                BranchedFromMessageId = branchPoint.Id,
                DirectorInstructions = sourceConversation.DirectorInstructions,
                DirectorInstructionsUpdatedAt = sourceConversation.DirectorInstructionsUpdatedAt,
                SceneContext = sourceConversation.SceneContext,
                SceneContextUpdatedAt = sourceConversation.SceneContextUpdatedAt,
                IsOocModeEnabled = sourceConversation.IsOocModeEnabled,
                RuntimeModelProfileOverrideId = sourceConversation.RuntimeModelProfileOverrideId,
                RuntimeGenerationPresetOverrideId = sourceConversation.RuntimeGenerationPresetOverrideId,
                Title = $"{sourceConversation.Title} (branch)",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(branchedConversation, cancellationToken);

            foreach (var sourceMessage in messagesToCopy)
            {
                var copiedMessage = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = branchedConversation.Id,
                    Role = sourceMessage.Role,
                    OriginType = sourceMessage.OriginType,
                    Content = sourceMessage.Content,
                    SequenceNumber = sourceMessage.SequenceNumber,
                    CreatedAt = DateTime.UtcNow,
                    SelectedVariantIndex = sourceMessage.SelectedVariantIndex
                };

                await repository.AddMessageAsync(copiedMessage, cancellationToken);

                foreach (var variant in sourceMessage.Variants.OrderBy(x => x.VariantIndex))
                {
                    var copiedVariant = new MessageVariant
                    {
                        Id = Guid.NewGuid(),
                        MessageId = copiedMessage.Id,
                        VariantIndex = variant.VariantIndex,
                        Content = variant.Content,
                        CreatedAt = DateTime.UtcNow,
                        ProviderType = variant.ProviderType,
                        ModelIdentifier = variant.ModelIdentifier,
                        ModelProfileId = variant.ModelProfileId,
                        GenerationPresetId = variant.GenerationPresetId,
                        RuntimeSourceType = variant.RuntimeSourceType,
                        GenerationStartedAt = variant.GenerationStartedAt,
                        GenerationCompletedAt = variant.GenerationCompletedAt,
                        ResponseTimeMs = variant.ResponseTimeMs
                    };

                    await repository.AddMessageVariantAsync(copiedVariant, cancellationToken);
                }
            }

            await repository.SaveChangesAsync(cancellationToken);
            await scheduler.ScheduleConversationChangeAsync(
                branchedConversation.Id,
                ConversationBackgroundWorkType.All,
                "conversation-branch",
                cancellationToken);

            var created = await repository.GetByIdWithMessagesAsync(branchedConversation.Id, cancellationToken);
            return Results.Ok(ToConversationResponse(created!));
        });

        group.MapPut("/{conversationId:guid}/messages/{messageId:guid}/selected-variant", async (
            Guid conversationId,
            Guid messageId,
            SelectMessageVariantRequest request,
            ChatOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            var result = await orchestrator.SelectMessageVariantAsync(
                conversationId,
                messageId,
                request.VariantIndex,
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPut("/{id:guid}/persona", async (
            Guid id,
            UpdateConversationPersonaRequest request,
            IConversationRepository conversationRepository,
            IUserPersonaRepository userPersonaRepository,
            CancellationToken cancellationToken) =>
        {
            var conversation = await conversationRepository.GetByIdWithMessagesAsync(id, cancellationToken);
            if (conversation is null)
            {
                return Results.NotFound();
            }

            if (request.UserPersonaId.HasValue)
            {
                var persona = await userPersonaRepository.GetByIdAsync(request.UserPersonaId.Value, cancellationToken);
                if (persona is null)
                {
                    return Results.BadRequest(new { error = $"User persona '{request.UserPersonaId.Value}' was not found." });
                }

                conversation.UserPersonaId = persona.Id;
                conversation.UserPersona = persona;
            }
            else
            {
                conversation.UserPersonaId = null;
                conversation.UserPersona = null;
            }

            conversation.UpdatedAt = DateTime.UtcNow;

            await conversationRepository.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToConversationResponse(conversation));
        });

        group.MapPut("/{conversationId:guid}/settings", async (
            Guid conversationId,
            UpdateConversationSettingsRequest request,
            IConversationRepository conversationRepository,
            CancellationToken cancellationToken) =>
        {
            var conversation = await conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken);
            if (conversation is null)
            {
                return Results.NotFound();
            }

            conversation.UserPersonaId = request.UserPersonaId;
            conversation.RuntimeModelProfileOverrideId = request.RuntimeModelProfileOverrideId;
            conversation.RuntimeGenerationPresetOverrideId = request.RuntimeGenerationPresetOverrideId;
            conversation.UpdatedAt = DateTime.UtcNow;

            await conversationRepository.SaveChangesAsync(cancellationToken);

            return Results.Ok();
        });

        return app;
    }

    private static ConversationResponse ToConversationResponse(Conversation conversation) =>
        new()
        {
            Id = conversation.Id,
            CharacterId = conversation.CharacterId,
            UserPersonaId = conversation.UserPersonaId,
            RuntimeModelProfileOverrideId = conversation.RuntimeModelProfileOverrideId,
            RuntimeGenerationPresetOverrideId = conversation.RuntimeGenerationPresetOverrideId,
            ParentConversationId = conversation.ParentConversationId,
            BranchedFromMessageId = conversation.BranchedFromMessageId,
            DirectorInstructions = conversation.DirectorInstructions,
            SceneContext = conversation.SceneContext,
            IsOocModeEnabled = conversation.IsOocModeEnabled,
            Title = conversation.Title,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            Messages = conversation.Messages
                .OrderBy(x => x.SequenceNumber)
                .Select(ToMessageResponse)
                .ToList()
        };

    private static MessageResponse ToMessageResponse(Message message)
    {
        var orderedVariants = message.Variants
            .OrderBy(x => x.VariantIndex)
            .ToList();

        var selectedVariant = orderedVariants
            .FirstOrDefault(x => x.VariantIndex == message.SelectedVariantIndex)
            ?? orderedVariants.FirstOrDefault();

        return new MessageResponse
        {
            Id = message.Id,
            Role = message.Role.ToString(),
            OriginType = message.OriginType.ToString(),
            Content = message.Content,
            SequenceNumber = message.SequenceNumber,
            CreatedAt = message.CreatedAt,
            SelectedVariantIndex = message.SelectedVariantIndex,
            VariantCount = orderedVariants.Count,
            Variants = orderedVariants
                .Select(x => new MessageVariantResponse
                {
                    Id = x.Id,
                    Content = x.Content,
                    VariantIndex = x.VariantIndex,
                    CreatedAt = x.CreatedAt,
                    Provider = x.ProviderType.HasValue
                        ? ModelRoute.ProviderToWireValue(x.ProviderType.Value)
                        : null,
                    ModelIdentifier = x.ModelIdentifier,
                    ModelProfileId = x.ModelProfileId,
                    GenerationPresetId = x.GenerationPresetId,
                    RuntimeSource = x.RuntimeSourceType?.ToString(),
                    GenerationStartedAt = x.GenerationStartedAt,
                    GenerationCompletedAt = x.GenerationCompletedAt,
                    ResponseTimeMs = x.ResponseTimeMs
                })
                .ToList(),
            SelectedProvider = selectedVariant?.ProviderType.HasValue == true
                ? ModelRoute.ProviderToWireValue(selectedVariant.ProviderType.Value)
                : null,
            SelectedModelIdentifier = selectedVariant?.ModelIdentifier,
            SelectedModelProfileId = selectedVariant?.ModelProfileId,
            SelectedGenerationPresetId = selectedVariant?.GenerationPresetId,
            SelectedRuntimeSource = selectedVariant?.RuntimeSourceType?.ToString(),
            SelectedGenerationStartedAt = selectedVariant?.GenerationStartedAt,
            SelectedGenerationCompletedAt = selectedVariant?.GenerationCompletedAt,
            SelectedResponseTimeMs = selectedVariant?.ResponseTimeMs
        };
    }
}
