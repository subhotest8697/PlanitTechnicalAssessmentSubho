using Microsoft.Playwright;

namespace JupiterToys.Automation.PageObjects;

public abstract class BasePage
{
    protected const string BaseUrl = "https://jupiter.cloud.planittesting.com";

    protected readonly IPage Page;

    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected abstract string Path { get; }

    public NavigationComponent Navigation => new(Page);

    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{BaseUrl}{Path}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }
}
