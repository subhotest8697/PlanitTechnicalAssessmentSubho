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
    // adding a duplicate line, so buying N units is N clicks. Each click waits for the navbar
    // cart count to reflect the increment (rather than a fixed delay) so the next click can't
    // race the Angular digest cycle that updates it.
    public async Task BuyProductAsync(string productName, int quantity)
    {
        var navigation = Navigation;
        var expectedCount = await navigation.GetCartCountAsync();

        for (var i = 0; i < quantity; i++)
        {
            await BuyProductAsync(productName);
            expectedCount++;
            await Assertions.Expect(navigation.CartCount).ToHaveTextAsync(expectedCount.ToString());
        }
    }

    public async Task BuyProductAsync(int index)
    {
        await ProductItems.Nth(index).Locator("a.btn-success").ClickAsync();
    }
}
