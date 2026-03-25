using LocalChat.Application.Abstractions.ImageGeneration;
using Microsoft.Extensions.Hosting;

namespace LocalChat.Infrastructure.ImageGeneration;

public sealed class LocalGeneratedImageFileStore : IGeneratedImageFileStore
{
    private readonly IHostEnvironment _hostEnvironment;

    public LocalGeneratedImageFileStore(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<string> SaveAsync(
        byte[] imageBytes,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";

        var folder = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "wwwroot",
            "generated",
            "images",
            DateTime.UtcNow.ToString("yyyy"),
            DateTime.UtcNow.ToString("MM"));

        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, safeFileName);

        await File.WriteAllBytesAsync(fullPath, imageBytes, cancellationToken);

        var parts = fullPath.Split(Path.DirectorySeparatorChar);
        var generatedIndex = Array.FindIndex(parts, x => string.Equals(x, "generated", StringComparison.OrdinalIgnoreCase));

        return generatedIndex >= 0
            ? "/" + string.Join('/', parts.Skip(generatedIndex)).Replace("\\", "/")
            : $"/generated/images/{safeFileName}";
    }
}
