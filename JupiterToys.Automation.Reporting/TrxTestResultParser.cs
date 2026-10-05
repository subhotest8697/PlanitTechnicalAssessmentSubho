using System.Globalization;
using System.Xml.Linq;
using JupiterToys.Automation.Reporting.Models;

namespace JupiterToys.Automation.Reporting;

/// <summary>
/// Parses the .trx file VSTest writes when tests run with `dotnet test --logger trx`.
/// Reading VSTest's own record of what happened (rather than hooking into xUnit directly)
/// keeps this reporter decoupled from the test project - it works for any test run that
/// produces a standard trx file.
/// </summary>
public static class TrxTestResultParser
{
    private static readonly XNamespace Ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    public static TestRunReport Parse(string trxFilePath)
    {
        var document = XDocument.Load(trxFilePath);
        var root = document.Root ?? throw new InvalidDataException($"'{trxFilePath}' has no root element.");

        var runName = root.Attribute("name")?.Value ?? Path.GetFileNameWithoutExtension(trxFilePath);

        // TestDefinitions holds the class/method name for each test; Results holds the
        // outcome/duration/error for each execution. They're joined by "testId".
        var testNamesById = root
            .Descendants(Ns + "UnitTest")
            .Select(unitTest => new
            {
                Id = unitTest.Attribute("id")?.Value,
                Method = unitTest.Element(Ns + "TestMethod")
            })
            .Where(x => x.Id is not null && x.Method is not null)
            .ToDictionary(
                x => x.Id!,
                x => (
                    ClassName: x.Method!.Attribute("className")?.Value ?? "Unknown",
                    MethodName: x.Method!.Attribute("name")?.Value ?? "Unknown"));

        var results = new List<TestResult>();

        foreach (var unitTestResult in root.Descendants(Ns + "UnitTestResult"))
        {
            var testId = unitTestResult.Attribute("testId")?.Value;
            var (className, methodName) = testId is not null && testNamesById.TryGetValue(testId, out var names)
                ? names
                : ("Unknown", unitTestResult.Attribute("testName")?.Value ?? "Unknown");

            var outcome = MapOutcome(unitTestResult.Attribute("outcome")?.Value);
            var duration = ParseDuration(unitTestResult.Attribute("duration")?.Value);
            var startedAt = ParseStartTime(unitTestResult.Attribute("startTime")?.Value);

            var errorInfo = unitTestResult.Element(Ns + "Output")?.Element(Ns + "ErrorInfo");
            var errorMessage = errorInfo?.Element(Ns + "Message")?.Value;
            var stackTrace = errorInfo?.Element(Ns + "StackTrace")?.Value;

            results.Add(new TestResult(className, methodName, outcome, duration, startedAt, errorMessage, stackTrace));
        }

        return new TestRunReport(runName, results);
    }

    private static TestOutcome MapOutcome(string? rawOutcome) => rawOutcome switch
    {
        "Passed" => TestOutcome.Passed,
        "Failed" => TestOutcome.Failed,
        // VSTest reports xUnit's Skip as "NotExecuted"; treat any other status the same way.
        _ => TestOutcome.Skipped
    };

    private static TimeSpan ParseDuration(string? rawDuration) =>
        rawDuration is not null && TimeSpan.TryParse(rawDuration, CultureInfo.InvariantCulture, out var duration)
            ? duration
            : TimeSpan.Zero;

    private static DateTimeOffset ParseStartTime(string? rawStartTime) =>
        rawStartTime is not null && DateTimeOffset.TryParse(rawStartTime, CultureInfo.InvariantCulture, out var startTime)
            ? startTime
            : DateTimeOffset.MinValue;
}
