using Microsoft.Playwright;

namespace JupiterToys.Automation.PageObjects;

public class CartPage : BasePage
{
    protected override string Path => "/#/cart";

    public CartPage(IPage page) : base(page)
    {
    }

    public ILocator CartItemsTable => Page.Locator("table.cart-items");
    public ILocator CartRows => Page.Locator("tr.cart-item");
    public ILocator TotalText => Page.Locator("strong.total");
    public ILocator CheckOutButton => Page.Locator("a.btn-checkout");
    public ILocator EmptyCartButton => Page.Locator("a.btn.btn-danger:has-text('Empty Cart')");
    public ILocator ContinueShoppingLink => Page.Locator("a[href='#/shop/']");

    // ng-confirm renders a confirmation modal ("Remove Item" / "Empty Cart") with Yes/No buttons.
    private ILocator ConfirmModal => Page.Locator("div.popup.modal.in");
    public ILocator ConfirmYesButton => ConfirmModal.Locator("button:has-text('Yes')");
    public ILocator ConfirmNoButton => ConfirmModal.Locator("button:has-text('No')");

    private ILocator RowByName(string productName) =>
        CartRows.Filter(new LocatorFilterOptions { HasTextString = productName });

    public async Task<IReadOnlyList<string>> GetItemNamesAsync()
    {
        await CartRows.First.WaitForAsync();
        var rows = await CartRows.AllAsync();
        var names = new List<string>();
        foreach (var row in rows)
        {
            names.Add((await row.Locator("td").First.InnerTextAsync()).Trim());
        }
        return names;
    }

    public ILocator QuantityInputFor(string productName) => RowByName(productName).Locator("input[name='quantity']");

    public async Task SetQuantityAsync(string productName, int quantity)
    {
        var input = QuantityInputFor(productName);
        await input.FillAsync(quantity.ToString());
        await input.DispatchEventAsync("change");
    }

    // Cart row columns: 0 = Item, 1 = Price, 2 = Quantity, 3 = Subtotal, 4 = Actions.
    public async Task<string> GetPriceAsync(string productName)
    {
        return (await RowByName(productName).Locator("td").Nth(1).InnerTextAsync()).Trim();
    }

    public async Task<string> GetSubtotalAsync(string productName)
    {
        return (await RowByName(productName).Locator("td").Nth(3).InnerTextAsync()).Trim();
    }

    public async Task RemoveItemAsync(string productName)
    {
        await RowByName(productName).Locator("a.remove-item").ClickAsync();
        await ConfirmYesButton.ClickAsync();
    }

    public async Task<string> GetTotalAsync()
    {
        return (await TotalText.InnerTextAsync()).Trim();
    }

    public async Task<CheckoutPage> ClickCheckOutAsync()
    {
        await CheckOutButton.ClickAsync();
        return new CheckoutPage(Page);
    }

    public async Task EmptyCartAsync()
    {
        await EmptyCartButton.ClickAsync();
        await ConfirmYesButton.ClickAsync();
    }

    public async Task<ShopPage> ContinueShoppingAsync()
    {
        await ContinueShoppingLink.ClickAsync();
        return new ShopPage(Page);
    }

    public async Task<bool> IsCartEmptyAsync()
    {
        return !await CartItemsTable.IsVisibleAsync();
    }
}
