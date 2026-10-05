using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JupiterToys.Automation.Reporting.Models;

namespace JupiterToys.Automation.Reporting;

public static class TestReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task WriteJsonAsync(TestRunReport report, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, report, JsonOptions);
    }

    public static async Task WriteHtmlAsync(TestRunReport report, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, BuildHtml(report), Encoding.UTF8);
    }

    private static string BuildHtml(TestRunReport report)
    {
        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"UTF-8\">");
        html.AppendLine($"<title>Test Report - {Escape(report.RunName)}</title>");
        html.AppendLine("<style>");
        html.AppendLine(Css);
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("<header>");
        html.AppendLine($"<h1>{Escape(report.RunName)}</h1>");
        if (report.StartedAt is { } startedAt)
        {
            html.AppendLine($"<p class=\"subtitle\">Run started: {startedAt:yyyy-MM-dd HH:mm:ss zzz}</p>");
        }
        html.AppendLine("</header>");

        html.AppendLine("<section class=\"summary\">");
        html.AppendLine(SummaryCard("Total", report.TotalCount.ToString(), "neutral"));
        html.AppendLine(SummaryCard("Passed", report.PassedCount.ToString(), "passed"));
        html.AppendLine(SummaryCard("Failed", report.FailedCount.ToString(), "failed"));
        html.AppendLine(SummaryCard("Skipped", report.SkippedCount.ToString(), "skipped"));
        html.AppendLine(SummaryCard("Pass rate", $"{report.PassRatePercent}%", "neutral"));
        html.AppendLine(SummaryCard("Duration", FormatDuration(report.TotalDuration), "neutral"));
        html.AppendLine("</section>");

        html.AppendLine("<table>");
        html.AppendLine("<thead><tr><th>Test</th><th>Outcome</th><th>Duration</th><th>Error</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var result in report.Results.OrderBy(r => r.Outcome).ThenBy(r => r.FullName))
        {
            var badgeClass = result.Outcome switch
            {
                TestOutcome.Passed => "badge-passed",
                TestOutcome.Failed => "badge-failed",
                _ => "badge-skipped"
            };

            html.AppendLine("<tr>");
            html.AppendLine($"<td>{Escape(result.FullName)}</td>");
            html.AppendLine($"<td><span class=\"badge {badgeClass}\">{result.Outcome}</span></td>");
            html.AppendLine($"<td>{FormatDuration(result.Duration)}</td>");
            html.AppendLine(result.ErrorMessage is { Length: > 0 }
                ? $"<td class=\"error\"><details><summary>{Escape(Truncate(result.ErrorMessage, 120))}</summary><pre>{Escape(result.ErrorMessage)}{(result.StackTrace is { Length: > 0 } ? "\n\n" + Escape(result.StackTrace) : string.Empty)}</pre></details></td>"
                : "<td></td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static string SummaryCard(string label, string value, string tone) =>
        $"<div class=\"card card-{tone}\"><div class=\"card-value\">{value}</div><div class=\"card-label\">{label}</div></div>";

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalMinutes >= 1
            ? $"{(int)duration.TotalMinutes}m {duration.Seconds}s"
            : $"{duration.TotalSeconds:0.0}s";

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength] + "...";

    private static string Escape(string text) => System.Net.WebUtility.HtmlEncode(text);

    private const string Css = """
        :root {
            color-scheme: light dark;
            --passed: #2e7d32;
            --failed: #c62828;
            --skipped: #ef6c00;
            --border: #d0d0d0;
            --bg-card: #f5f5f5;
        }
        body { font-family: Segoe UI, Arial, sans-serif; margin: 2rem; color: #1a1a1a; background: #ffffff; }
        header h1 { margin-bottom: 0.2rem; }
        .subtitle { color: #666; margin-top: 0; }
        .summary { display: flex; gap: 1rem; flex-wrap: wrap; margin: 1.5rem 0; }
        .card { border: 1px solid var(--border); border-radius: 8px; padding: 1rem 1.5rem; background: var(--bg-card); min-width: 100px; text-align: center; }
        .card-value { font-size: 1.8rem; font-weight: 600; }
        .card-label { color: #555; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 0.04em; }
        .card-passed .card-value { color: var(--passed); }
        .card-failed .card-value { color: var(--failed); }
        .card-skipped .card-value { color: var(--skipped); }
        table { width: 100%; border-collapse: collapse; margin-top: 1rem; }
        th, td { padding: 0.6rem 0.8rem; border-bottom: 1px solid var(--border); text-align: left; vertical-align: top; }
        th { background: #fafafa; }
        .badge { padding: 0.15rem 0.6rem; border-radius: 12px; font-size: 0.8rem; font-weight: 600; color: #fff; }
        .badge-passed { background: var(--passed); }
        .badge-failed { background: var(--failed); }
        .badge-skipped { background: var(--skipped); }
        .error pre { white-space: pre-wrap; font-size: 0.8rem; background: #fff3f3; border: 1px solid #f0c0c0; padding: 0.5rem; border-radius: 4px; }
        details summary { cursor: pointer; color: var(--failed); }

        @media (prefers-color-scheme: dark) {
            body { background: #1e1e1e; color: #e0e0e0; }
            .card { background: #2a2a2a; border-color: #3a3a3a; }
            .card-label { color: #aaa; }
            th { background: #2a2a2a; }
            th, td { border-color: #3a3a3a; }
            .error pre { background: #2a1515; border-color: #5a2a2a; }
        }
        """;
}
