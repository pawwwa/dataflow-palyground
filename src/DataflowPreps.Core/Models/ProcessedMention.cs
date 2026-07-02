namespace DataflowPreps.Core.Models;

public sealed class ProcessedMention
{
    public required Guid Id { get; set; }
    public required string Source { get; set; }
    public required string AuthorTag { get; set; }
    public required string RawContent { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string Hash { get; set; }
    public required Reaction Reaction { get; set; }
}

public enum Reaction
{
    Negative = -1,
    Neutral = 0,
    Positive = 1,
}