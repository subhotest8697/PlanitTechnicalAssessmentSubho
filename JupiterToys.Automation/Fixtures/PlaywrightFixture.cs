using Microsoft.Playwright;

namespace JupiterToys.Automation.Fixtures;

/// <summary>
/// Prefers Playwright's own managed browser (installed via `playwright install chromium`),
/// falling back to a locally installed Chrome/Edge if that's unavailable - needed in
/// environments like this sandbox, where a corporate proxy blocks the Playwright CDN.
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
        _browser = await LaunchBrowserAsync();
        Context = await _browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.CloseAsync();
        await _browser!.CloseAsync();
        _playwright!.Dispose();
    }

    private async Task<IBrowser> LaunchBrowserAsync()
    {
        try
        {
            return await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
        catch (PlaywrightException)
        {
            return await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                ExecutablePath = ResolveLocalChromePath(),
                Headless = true
            });
        }
    }

    private static string ResolveLocalChromePath()
    {
        var candidates = new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            "/usr/bin/google-chrome",
            "/usr/bin/google-chrome-stable",
            "/usr/bin/chromium-browser",
            "/usr/bin/chromium",
            "/opt/google/chrome/google-chrome"
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException(
            "No Playwright-managed browser and no local Chrome/Edge installation found. " +
            "Run 'playwright install chromium' or install Chrome/Edge locally.");
    }
}
