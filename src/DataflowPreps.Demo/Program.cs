using System.Diagnostics;
using DataflowPreps.Core.Models;
using DataflowPreps.Core.Pipeline;
using DataflowPreps.Core.Store;
using DataflowPreps.Demo;

const int mentionsCount = 5_000;
const double positiveRatio = 0.40;
const double negativeRatio = 0.35;

Console.WriteLine("DataflowPreps — mention processing demo");
Console.WriteLine("=======================================");
Console.WriteLine($"Publishing {mentionsCount:N0} mentions...");
Console.WriteLine($"Target mix: positive={positiveRatio:P0}, negative={negativeRatio:P0}, neutral={(1 - positiveRatio - negativeRatio):P0}");
Console.WriteLine();

var store = new InMemoryMentionStore();
var options = new PipelineOptions
{
    BoundedCapacity = 512,
    BatchSize = 100,
    MentionProcessingParallelism = Environment.ProcessorCount,
    StoreWriteParallelism = 2
};

await using var pipeline = new MentionsProcessingPipeline(store, options);
var stopwatch = Stopwatch.StartNew();

foreach (var mention in MockedMentionsGenerator.Generate(
             mentionsCount,
             positiveRatio: positiveRatio,
             negativeRatio: negativeRatio,
             seed: 42))
{
    await pipeline.PublishAsync(mention);
}

await pipeline.CompleteAsync();
stopwatch.Stop();

var persisted = store.Mentions;
var reactionBreakdown = persisted
    .GroupBy(mention => mention.Reaction)
    .OrderBy(group => group.Key)
    .Select(group => $"{group.Key}: {group.Count():N0}");

Console.WriteLine("Results");
Console.WriteLine($"  persisted: {persisted.Count:N0}");
Console.WriteLine($"  elapsed:   {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"  throughput: {persisted.Count / stopwatch.Elapsed.TotalSeconds:N0} mentions/sec");
Console.WriteLine();
Console.WriteLine("Reaction breakdown");
foreach (var line in reactionBreakdown)
{
    Console.WriteLine($"  {line}");
}

Console.WriteLine();
Console.WriteLine("Sample output (first 3 mentions):");
foreach (var mention in persisted.Take(3))
{
    Console.WriteLine($"  [{mention.Source}] {mention.AuthorTag}: {mention.RawContent}");
    Console.WriteLine($"    hash={mention.Hash}, reaction={mention.Reaction}");
}
