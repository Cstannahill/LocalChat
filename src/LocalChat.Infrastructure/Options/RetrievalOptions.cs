namespace LocalChat.Infrastructure.Options;

public sealed class RetrievalOptions
{
    public const string SectionName = "Retrieval";

    public bool Enabled { get; set; } = true;

    public int CandidatePoolSize { get; set; } = 24;

    public int MaxSelectedMemoryItems { get; set; } = 6;

    public int MaxSelectedLoreEntries { get; set; } = 4;

    public double SemanticWeight { get; set; } = 0.72;

    public double LexicalWeight { get; set; } = 0.22;

    public double RecencyWeight { get; set; } = 0.06;

    public double MemoryBaseBoost { get; set; } = 0.10;

    public double LoreBaseBoost { get; set; } = 0.05;

    public double PinnedMemoryBoost { get; set; } = 0.18;

    public double QueryPhraseBoost { get; set; } = 0.12;

    public double ExactTokenBoost { get; set; } = 0.06;

    public double MinSemanticScore { get; set; } = 0.18;

    public double MinFinalScore { get; set; } = 0.24;

    public double StrongLexicalScore { get; set; } = 0.70;

    public double FallbackMinFinalScore { get; set; } = 0.20;

    public int MinTokenLength { get; set; } = 3;

    public double RecencyHalfLifeDays { get; set; } = 30.0;
}
