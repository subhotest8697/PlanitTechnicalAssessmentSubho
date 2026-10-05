namespace JupiterToys.Automation.Reporting.Models;

/// <summary>
/// The outcome of a single test method, as recorded by VSTest in the .trx file produced by
/// `dotnet test --logger trx`.
/// </summary>
public record TestResult(
    string ClassName,
    string MethodName,
    TestOutcome Outcome,
    TimeSpan Duration,
    DateTimeOffset StartedAt,
    string? ErrorMessage = null,
    string? StackTrace = null)
{
    public string FullName => $"{ClassName}.{MethodName}";
}
