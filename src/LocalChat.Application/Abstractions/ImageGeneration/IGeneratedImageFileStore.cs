namespace LocalChat.Application.Abstractions.ImageGeneration;

public interface IGeneratedImageFileStore
{
    Task<string> SaveAsync(
        byte[] imageBytes,
        string fileName,
        CancellationToken cancellationToken = default);
}
