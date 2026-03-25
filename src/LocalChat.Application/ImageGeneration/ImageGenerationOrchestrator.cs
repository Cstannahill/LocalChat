using LocalChat.Application.Abstractions.ImageGeneration;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Images;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.ImageGeneration;

public sealed class ImageGenerationOrchestrator
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IImageGenerationJobRepository _imageGenerationJobRepository;
    private readonly IImageGenerationProvider _imageGenerationProvider;
    private readonly IGeneratedImageFileStore _generatedImageFileStore;

    public ImageGenerationOrchestrator(
        IConversationRepository conversationRepository,
        IImageGenerationJobRepository imageGenerationJobRepository,
        IImageGenerationProvider imageGenerationProvider,
        IGeneratedImageFileStore generatedImageFileStore)
    {
        _conversationRepository = conversationRepository;
        _imageGenerationJobRepository = imageGenerationJobRepository;
        _imageGenerationProvider = imageGenerationProvider;
        _generatedImageFileStore = generatedImageFileStore;
    }

    public async Task<ImageGenerationJob> GenerateAsync(
        Guid conversationId,
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var job = new ImageGenerationJob
        {
            Id = Guid.NewGuid(),
            CharacterId = conversation.CharacterId,
            ConversationId = conversation.Id,
            Provider = "ComfyUI",
            PromptText = request.Prompt,
            NegativePromptText = request.NegativePrompt ?? string.Empty,
            Width = request.Width,
            Height = request.Height,
            Steps = request.Steps,
            Cfg = request.Cfg,
            Seed = request.Seed,
            Status = ImageGenerationJobStatus.Running,
            CreatedAt = DateTime.UtcNow
        };

        Exception? generationException = null;

        try
        {
            var providerResult = await _imageGenerationProvider.GenerateAsync(request, cancellationToken);

            job.ProviderJobId = providerResult.ProviderJobId;
            job.Status = ImageGenerationJobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;

            foreach (var image in providerResult.Images.OrderBy(x => x.SortOrder))
            {
                var url = await _generatedImageFileStore.SaveAsync(
                    image.Bytes,
                    image.FileName,
                    cancellationToken);

                job.Assets.Add(new GeneratedImageAsset
                {
                    Id = Guid.NewGuid(),
                    ImageGenerationJobId = job.Id,
                    RelativeUrl = url,
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    SortOrder = image.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            generationException = ex;
            job.Status = ImageGenerationJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
        }

        await _imageGenerationJobRepository.AddAsync(job, cancellationToken);
        await _imageGenerationJobRepository.SaveChangesAsync(cancellationToken);

        if (job.Status == ImageGenerationJobStatus.Failed)
        {
            throw new InvalidOperationException(
                job.ErrorMessage ?? "Image generation failed.",
                generationException);
        }

        return await _imageGenerationJobRepository.GetByIdAsync(job.Id, cancellationToken)
               ?? throw new InvalidOperationException("Generated image job could not be reloaded.");
    }

    public Task<IReadOnlyList<ImageGenerationJob>> ListByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return _imageGenerationJobRepository.ListByConversationAsync(conversationId, cancellationToken);
    }
}
