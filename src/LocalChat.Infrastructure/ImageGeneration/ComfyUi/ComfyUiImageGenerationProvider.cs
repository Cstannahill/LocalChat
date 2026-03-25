using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using LocalChat.Application.Abstractions.ImageGeneration;
using LocalChat.Infrastructure.Options;
using Microsoft.Extensions.Hosting;

namespace LocalChat.Infrastructure.ImageGeneration.ComfyUi;

public sealed class ComfyUiImageGenerationProvider : IImageGenerationProvider
{
    private readonly HttpClient _httpClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ComfyUiOptions _options;

    public ComfyUiImageGenerationProvider(
        HttpClient httpClient,
        IHostEnvironment hostEnvironment,
        ComfyUiOptions options)
    {
        _httpClient = httpClient;
        _hostEnvironment = hostEnvironment;
        _options = options;
    }

    public async Task<ImageGenerationProviderResult> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await LoadWorkflowTemplateAsync(cancellationToken);

        ApplyPromptInputs(workflow, request);

        var promptId = Guid.NewGuid().ToString("N");
        var clientId = Guid.NewGuid().ToString("N");

        using (var response = await _httpClient.PostAsJsonAsync(
                   "prompt",
                   new ComfyUiQueuePromptRequest
                   {
                       Prompt = workflow,
                       ClientId = clientId,
                       PromptId = promptId
                   },
                   cancellationToken))
        {
            response.EnsureSuccessStatusCode();
        }

        var imageRefs = await WaitForImagesAsync(promptId, cancellationToken);

        var images = new List<GeneratedImageBinary>();

        for (var i = 0; i < imageRefs.Count; i++)
        {
            var imageRef = imageRefs[i];
            var bytes = await DownloadImageAsync(
                imageRef.Filename,
                imageRef.Subfolder,
                imageRef.Type,
                cancellationToken);

            var extension = Path.GetExtension(imageRef.Filename);
            var contentType = GuessContentType(extension);

            images.Add(new GeneratedImageBinary
            {
                Bytes = bytes,
                ContentType = contentType,
                FileName = imageRef.Filename,
                SortOrder = i
            });
        }

        return new ImageGenerationProviderResult
        {
            ProviderJobId = promptId,
            Images = images
        };
    }

    private async Task<JsonObject> LoadWorkflowTemplateAsync(CancellationToken cancellationToken)
    {
        var fullPath = Path.IsPathRooted(_options.WorkflowTemplatePath)
            ? _options.WorkflowTemplatePath
            : Path.Combine(_hostEnvironment.ContentRootPath, _options.WorkflowTemplatePath);

        if (!File.Exists(fullPath))
        {
            throw new InvalidOperationException($"ComfyUI workflow template was not found at '{fullPath}'.");
        }

        var json = await File.ReadAllTextAsync(fullPath, cancellationToken);
        var node = JsonNode.Parse(json)?.AsObject();

        if (node is null)
        {
            throw new InvalidOperationException("ComfyUI workflow template could not be parsed.");
        }

        return node;
    }

    private void ApplyPromptInputs(JsonObject workflow, ImageGenerationRequest request)
    {
        SetNodeInput(workflow, _options.PositivePromptNodeId, "text", request.Prompt);
        TrySetNodeInput(workflow, _options.NegativePromptNodeId, "text", request.NegativePrompt ?? string.Empty);
        SetNodeInput(workflow, _options.LatentNodeId, "width", request.Width);
        SetNodeInput(workflow, _options.LatentNodeId, "height", request.Height);
        SetNodeInput(workflow, _options.SamplerNodeId, "steps", request.Steps);
        SetNodeInput(workflow, _options.SamplerNodeId, "cfg", request.Cfg);
        SetNodeInput(workflow, _options.SamplerNodeId, "seed", request.Seed < 0 ? Random.Shared.NextInt64() : request.Seed);
        SetNodeInput(workflow, _options.SaveImageNodeId, "filename_prefix", $"{_options.FilenamePrefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
    }

    private static void SetNodeInput(JsonObject workflow, string nodeId, string inputName, object? value)
    {
        if (workflow[nodeId] is not JsonObject node ||
            node["inputs"] is not JsonObject inputs)
        {
            throw new InvalidOperationException($"Workflow node '{nodeId}' with inputs was not found.");
        }

        inputs[inputName] = JsonSerializer.SerializeToNode(value);
    }

    private static void TrySetNodeInput(JsonObject workflow, string? nodeId, string inputName, object? value)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (workflow[nodeId] is not JsonObject node ||
            node["inputs"] is not JsonObject inputs)
        {
            return;
        }

        if (!inputs.ContainsKey(inputName))
        {
            return;
        }

        inputs[inputName] = JsonSerializer.SerializeToNode(value);
    }

    private async Task<List<ImageRef>> WaitForImagesAsync(string promptId, CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds);

        while (DateTime.UtcNow < timeoutAt)
        {
            using var response = await _httpClient.GetAsync($"history/{promptId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonNode.Parse(json)?.AsObject();

            if (root is not null &&
                root[promptId] is JsonObject promptHistory &&
                promptHistory["outputs"] is JsonObject outputs)
            {
                var refs = new List<ImageRef>();

                foreach (var output in outputs)
                {
                    if (output.Value is not JsonObject nodeOutput)
                    {
                        continue;
                    }

                    if (nodeOutput["images"] is not JsonArray imagesArray)
                    {
                        continue;
                    }

                    foreach (var imageNode in imagesArray)
                    {
                        if (imageNode is not JsonObject imageObj)
                        {
                            continue;
                        }

                        var filename = imageObj["filename"]?.GetValue<string>();
                        var subfolder = imageObj["subfolder"]?.GetValue<string>() ?? string.Empty;
                        var type = imageObj["type"]?.GetValue<string>() ?? "output";

                        if (!string.IsNullOrWhiteSpace(filename))
                        {
                            refs.Add(new ImageRef
                            {
                                Filename = filename,
                                Subfolder = subfolder,
                                Type = type
                            });
                        }
                    }
                }

                if (refs.Count > 0)
                {
                    return refs;
                }
            }

            await Task.Delay(_options.PollIntervalMs, cancellationToken);
        }

        throw new TimeoutException("Timed out waiting for ComfyUI image generation to complete.");
    }

    private async Task<byte[]> DownloadImageAsync(
        string filename,
        string subfolder,
        string type,
        CancellationToken cancellationToken)
    {
        var query = $"filename={Uri.EscapeDataString(filename)}&subfolder={Uri.EscapeDataString(subfolder)}&type={Uri.EscapeDataString(type)}";

        using var response = await _httpClient.GetAsync($"view?{query}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private static string GuessContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

    private sealed class ImageRef
    {
        public required string Filename { get; init; }

        public required string Subfolder { get; init; }

        public required string Type { get; init; }
    }
}
