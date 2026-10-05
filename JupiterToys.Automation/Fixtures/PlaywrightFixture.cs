using Microsoft.Playwright;

namespace JupiterToys.Automation.Fixtures;

/// <summary>
/// Launches the machine's installed Chrome rather than a Playwright-managed browser download,
/// since this environment's proxy blocks the Playwright CDN used by `playwright install`.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            ExecutablePath = ResolveChromePath(),
            Headless = true
        });
        Context = await _browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.CloseAsync();
        await _browser!.CloseAsync();
        _playwright!.Dispose();
    }

    private static string ResolveChromePath()
    {
        var candidates = new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException("No local Chrome or Edge installation found to drive Playwright with.");
    }
}
