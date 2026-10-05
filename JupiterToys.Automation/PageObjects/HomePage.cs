using Microsoft.Playwright;

namespace JupiterToys.Automation.PageObjects;

public class HomePage : BasePage
{
    protected override string Path => "/#/home";

    public HomePage(IPage page) : base(page)
    {
    }

    public ILocator Heading => Page.Locator("div[ng-view] h1, .container-fluid[ng-view] h1").First;
    public ILocator StartShoppingButton => Page.Locator("a.btn.btn-success.btn-large[href='#/shop']");

    public async Task<string> GetHeadingTextAsync()
    {
        return (await Heading.InnerTextAsync()).Trim();
    }

    public async Task<ShopPage> ClickStartShoppingAsync()
    {
        await StartShoppingButton.ClickAsync();
        return new ShopPage(Page);
    }
}
