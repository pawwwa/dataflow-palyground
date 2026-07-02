using DataflowPreps.Core.Models;

namespace DataflowPreps.Core.Pipeline;

internal static class ReactionAnalyzer
{
    private static readonly string[] NegativeWords = ["bad", "terrible", "hate", "awful", "broken", "worst"];
    private static readonly string[] PositiveWords = ["love", "great", "awesome", "excellent", "best", "amazing"];

    public static Reaction Analyze(string rawContent)
    {
        var text = rawContent.ToLowerInvariant();
        
        var negativeReactionEncounters = NegativeWords.Count(word => text.Contains(word, StringComparison.Ordinal));
        var positiveReactionEncounters = PositiveWords.Count(word => text.Contains(word, StringComparison.Ordinal));
        
        if (positiveReactionEncounters > negativeReactionEncounters)
        {
            return Reaction.Positive;
        }

        if (negativeReactionEncounters > positiveReactionEncounters)
        {
            return Reaction.Negative;
        }

        return Reaction.Neutral;
    }
    
}