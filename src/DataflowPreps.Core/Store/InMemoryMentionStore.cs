using DataflowPreps.Core.Models;

namespace DataflowPreps.Core.Store;

public interface IMentionStore
{
    Task PersistAsync(IEnumerable<ProcessedMention> mentions, CancellationToken ct);
}

public sealed class InMemoryMentionStore : IMentionStore
{
    private readonly List<ProcessedMention> _mentions = [];
    private readonly Lock _lock = new ();

    public IReadOnlyList<ProcessedMention> Mentions
    {
        get
        {
            lock (_lock)
            {
                return _mentions.ToArray();
            }
        }
    }
    
    public Task PersistAsync(IEnumerable<ProcessedMention> mentions, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_lock)
        {
            _mentions.AddRange(mentions);
        }
        
        return Task.CompletedTask;
    }
}