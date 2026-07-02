namespace DataflowPreps.Core.Models;

public sealed class PipelineOptions
{
    public int BoundedCapacity { get; set; } = 1000;
    public int BatchSize { get; init; } = 50;
    public int MentionProcessingParallelism { get; set; } = 4;
    public int StoreWriteParallelism { get; set; } = 4;
}