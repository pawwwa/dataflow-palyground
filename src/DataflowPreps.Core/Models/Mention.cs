namespace DataflowPreps.Core.Models;

public sealed class Mention
{
    public required Guid Id { get; set; }
    public required string Source { get; set; }
    public required string AuthorTag { get; set; }
    public required string RawContent { get; set; }
    public required DateTime Timestamp { get; set; }
}