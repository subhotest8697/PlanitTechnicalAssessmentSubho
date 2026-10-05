using JupiterToys.Automation.Reporting;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: JupiterToys.Automation.Reporting <path-to-trx-file> [output-directory]");
    return 1;
}

var trxPath = args[0];
if (!File.Exists(trxPath))
{
    Console.Error.WriteLine($"TRX file not found: {trxPath}");
    return 1;
}

var outputDirectory = args.Length > 1 ? args[1] : Path.Combine(Path.GetDirectoryName(trxPath)!, "report");

var report = TrxTestResultParser.Parse(trxPath);

var jsonPath = Path.Combine(outputDirectory, "report.json");
var htmlPath = Path.Combine(outputDirectory, "report.html");

await TestReportWriter.WriteJsonAsync(report, jsonPath);
await TestReportWriter.WriteHtmlAsync(report, htmlPath);

Console.WriteLine($"{report.PassedCount}/{report.TotalCount} passed ({report.PassRatePercent}%), {report.FailedCount} failed, {report.SkippedCount} skipped.");
Console.WriteLine($"JSON report: {jsonPath}");
Console.WriteLine($"HTML report: {htmlPath}");

return report.FailedCount == 0 ? 0 : 1;
