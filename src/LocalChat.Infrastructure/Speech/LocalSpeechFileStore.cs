using LocalChat.Application.Abstractions.Speech;
using LocalChat.Infrastructure.Options;
using Microsoft.Extensions.Hosting;

namespace LocalChat.Infrastructure.Speech;

public sealed class LocalSpeechFileStore : ISpeechFileStore
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly KokoroTtsOptions _options;

    public LocalSpeechFileStore(
        IHostEnvironment hostEnvironment,
        KokoroTtsOptions options)
    {
        _hostEnvironment = hostEnvironment;
        _options = options;
    }

    public async Task<string> SaveAsync(
        byte[] audioBytes,
        string responseFormat,
        CancellationToken cancellationToken = default)
    {
        var extension = responseFormat.StartsWith('.')
            ? responseFormat
            : $".{responseFormat.ToLowerInvariant()}";

        var folder = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "wwwroot",
            "generated",
            "audio",
            DateTime.UtcNow.ToString("yyyy"),
            DateTime.UtcNow.ToString("MM"));

        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await File.WriteAllBytesAsync(fullPath, audioBytes, cancellationToken);

        var relativeSegments = fullPath.Split(Path.DirectorySeparatorChar);
        var generatedIndex = Array.FindIndex(
            relativeSegments,
            x => string.Equals(x, "generated", StringComparison.OrdinalIgnoreCase));

        if (generatedIndex < 0)
        {
            return $"{_options.PublicBasePath.TrimEnd('/')}/{fileName}";
        }

        var publicPath = "/" + string.Join('/', relativeSegments.Skip(generatedIndex));
        return publicPath.Replace("\\", "/");
    }
}
