using LocalChat.Application.Abstractions.Inference;

namespace LocalChat.Infrastructure.Inference.Tokenization;

public sealed class BasicTokenEstimator : ITokenEstimator
{
    public int EstimateTokens(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Very rough Stage 1 estimate:
        // ~4 characters per token is a decent crude approximation.
        return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
    }
}
