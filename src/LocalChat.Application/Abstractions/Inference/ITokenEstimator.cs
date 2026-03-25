namespace LocalChat.Application.Abstractions.Inference;

public interface ITokenEstimator
{
    int EstimateTokens(string? text);
}