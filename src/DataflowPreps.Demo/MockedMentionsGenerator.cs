using DataflowPreps.Core.Models;

namespace DataflowPreps.Demo;

internal static class MockedMentionsGenerator
{
    private static readonly string[] Platforms = ["twitter", "instagram", "facebook", "telegram"];
    private static readonly string[] Authors = ["@user_alex", "@user_john", "@user_annie", "@user_marta"];

    private static readonly string[] PositiveMentions =
    [
        "I love this product, absolutely great experience!",
        "Customer support was awesome today, thank you!",
        "Best purchase this year, excellent quality.",
        "Amazing update, everything works great now."
    ];

    private static readonly string[] NegativeMentions =
    [
        "This is terrible and broken, worst purchase ever.",
        "Why is this so bad? Hate waiting for fixes.",
        "Awful experience, would not recommend.",
        "Broken again, worst support ever."
    ];

    private static readonly string[] NeutralMentions =
    [
        "Just saw the new campaign, looks interesting.",
        "Neutral update about the release schedule.",
        "Posting about the event next week.",
        "Sharing the link for anyone interested."
    ];

    /// <param name="positiveRatio">Share of positive comments, from 0 to 1.</param>
    /// <param name="negativeRatio">Share of negative comments, from 0 to 1.</param>
    /// <param name="seed">Optional seed for reproducible output.</param>
    /// <remarks>Neutral share is whatever remains: 1 - positiveRatio - negativeRatio.</remarks>
    internal static IEnumerable<Mention> Generate(
        int count,
        double positiveRatio = 0.35,
        double negativeRatio = 0.35,
        int? seed = null)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (positiveRatio is < 0 or > 1 || negativeRatio is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException("Ratios must be between 0 and 1.");
        }

        if (positiveRatio + negativeRatio > 1)
        {
            throw new ArgumentException("positiveRatio + negativeRatio cannot exceed 1.");
        }

        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        for (var i = 0; i < count; i++)
        {
            var roll = random.NextDouble();
            var content = roll switch
            {
                _ when roll < positiveRatio => Pick(PositiveMentions, random),
                _ when roll < positiveRatio + negativeRatio => Pick(NegativeMentions, random),
                _ => Pick(NeutralMentions, random)
            };

            yield return new Mention
            {
                Id = Guid.NewGuid(),
                Source = Platforms[random.Next(Platforms.Length)],
                AuthorTag = Authors[random.Next(Authors.Length)],
                RawContent = $"{content} #{i}",
                Timestamp = DateTime.UtcNow.AddMilliseconds(-random.Next(0, 60_000))
            };
        }

        static string Pick(string[] pool, Random random) => pool[random.Next(pool.Length)];
    }
}
