using System.Threading.Tasks.Dataflow;
using DataflowPreps.Core.Models;
using DataflowPreps.Core.Store;

namespace DataflowPreps.Core.Pipeline;

public sealed class MentionsProcessingPipeline : IAsyncDisposable
{
    private readonly IMentionStore _store;

    private readonly BufferBlock<Mention> _buffer;
    private readonly TransformBlock<Mention, ProcessedMention> _mentionProcessor;
    private readonly BatchBlock<ProcessedMention> _processedMentions;
    private readonly ActionBlock<ProcessedMention[]> _persist;

    private readonly Task _completion;

    public MentionsProcessingPipeline(IMentionStore store, PipelineOptions options = null!)
    {
        _store = store;

        var pipelineOptions = options ?? new PipelineOptions();

        _buffer = new BufferBlock<Mention>(new DataflowBlockOptions
        {
            BoundedCapacity = pipelineOptions.BoundedCapacity,
        });

        var mentionProcessorOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = pipelineOptions.BoundedCapacity,
            MaxDegreeOfParallelism = pipelineOptions.MentionProcessingParallelism
        };

        _mentionProcessor = new TransformBlock<Mention, ProcessedMention>(mention =>
        {
            var processed = new ProcessedMention
            {
                Id = mention.Id,
                Source = mention.Source,
                AuthorTag = mention.AuthorTag,
                RawContent = mention.RawContent,
                Timestamp = mention.Timestamp,
                Hash = Hasher.Hash(mention.Source, mention.RawContent),
                Reaction = ReactionAnalyzer.Analyze(mention.RawContent)
            };

            return processed;
        }, mentionProcessorOptions);
        
        _processedMentions = new BatchBlock<ProcessedMention>(pipelineOptions.BatchSize, new GroupingDataflowBlockOptions
        {
            BoundedCapacity = pipelineOptions.BoundedCapacity
        });

        var persistOptions = new ExecutionDataflowBlockOptions()
        {
            BoundedCapacity = 32,
            MaxDegreeOfParallelism = pipelineOptions.StoreWriteParallelism
        };
        _persist = new ActionBlock<ProcessedMention[]>(async batch =>
        {
            await _store.PersistAsync(batch, CancellationToken.None);
        }, persistOptions);
        
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        
        _buffer.LinkTo(_mentionProcessor, linkOptions);
        _mentionProcessor.LinkTo(_processedMentions, linkOptions);
        _processedMentions.LinkTo(_persist, linkOptions);

        _completion = _persist.Completion;
    }

    public async Task PublishAsync(Mention mention, CancellationToken cancellationToken = default)
    {
        while (!await _buffer.SendAsync(mention, cancellationToken))
        {
            await Task.Delay(1, cancellationToken);
        }
    }

    public async Task CompleteAsync()
    {
        _buffer.Complete();
        _processedMentions.TriggerBatch();
        await _completion;
    }

    public ValueTask DisposeAsync()
    {
        _buffer.Complete();
        _processedMentions.TriggerBatch();
        return ValueTask.CompletedTask;
    }
}