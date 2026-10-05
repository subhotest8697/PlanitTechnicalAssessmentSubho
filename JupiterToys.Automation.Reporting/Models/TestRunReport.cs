namespace JupiterToys.Automation.Reporting.Models;

/// <summary>
/// Aggregate view of an entire test run: every individual result plus the summary counts
/// derived from them.
/// </summary>
public record TestRunReport(string RunName, IReadOnlyList<TestResult> Results)
{
    public int TotalCount => Results.Count;
    public int PassedCount => Results.Count(r => r.Outcome == TestOutcome.Passed);
    public int FailedCount => Results.Count(r => r.Outcome == TestOutcome.Failed);
    public int SkippedCount => Results.Count(r => r.Outcome == TestOutcome.Skipped);

    public double PassRatePercent => TotalCount == 0 ? 0 : Math.Round(PassedCount * 100.0 / TotalCount, 1);

    public TimeSpan TotalDuration => Results.Aggregate(TimeSpan.Zero, (sum, r) => sum + r.Duration);

    public DateTimeOffset? StartedAt => Results.Count == 0 ? null : Results.Min(r => r.StartedAt);
}
