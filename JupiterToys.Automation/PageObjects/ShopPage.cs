using Microsoft.Playwright;

namespace JupiterToys.Automation.PageObjects;

public class ShopPage : BasePage
{
    protected override string Path => "/#/shop";

    public ShopPage(IPage page) : base(page)
    {
    }

    public ILocator ProductItems => Page.Locator("li.product");

    private ILocator ProductByName(string productName) =>
        ProductItems.Filter(new LocatorFilterOptions { Has = Page.Locator(".product-title", new() { HasTextString = productName }) });

    public async Task<IReadOnlyList<string>> GetProductNamesAsync()
    {
        await ProductItems.First.WaitForAsync();
        return await ProductItems.Locator(".product-title").AllInnerTextsAsync();
    }

    public async Task<string> GetProductPriceAsync(string productName)
    {
        return (await ProductByName(productName).Locator(".product-price").InnerTextAsync()).Trim();
    }

    public async Task BuyProductAsync(string productName)
    {
        await ProductByName(productName).Locator("a.btn-success").ClickAsync();
    }

    // Clicking "Buy" on a product already in the cart increments its quantity rather than
    // adding a duplicate line, so buying N units is N clicks.
    public async Task BuyProductAsync(string productName, int quantity)
    {
        for (var i = 0; i < quantity; i++)
        {
            await BuyProductAsync(productName);
            await Page.WaitForTimeoutAsync(150);
        }
    }

    public async Task BuyProductAsync(int index)
    {
        await ProductItems.Nth(index).Locator("a.btn-success").ClickAsync();
    }
}
