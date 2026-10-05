using System.Globalization;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class CartPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture = new();

    public Task InitializeAsync() => _fixture.InitializeAsync();
    public Task DisposeAsync() => _fixture.DisposeAsync();

    [Fact]
    public async Task CartPage_WithMultipleProductsAndQuantities_CalculatesPricesAndTotalCorrectly()
    {
        var itemsToBuy = new Dictionary<string, int>
        {
            ["Stuffed Frog"] = 2,
            ["Fluffy Bunny"] = 5,
            ["Valentine Bear"] = 3
        };

        var shopPage = new ShopPage(_fixture.Page);
        await shopPage.NavigateAsync();

        // Prices must be read from the Shop page before it's navigated away from, since the
        // Cart page replaces it in the DOM.
        var expectedUnitPrices = new Dictionary<string, decimal>();
        foreach (var (productName, quantity) in itemsToBuy)
        {
            expectedUnitPrices[productName] = ParseCurrency(await shopPage.GetProductPriceAsync(productName));
            await shopPage.BuyProductAsync(productName, quantity);
        }

        var cartPage = await shopPage.Navigation.GoToCartAsync();

        var expectedTotal = 0m;

        foreach (var (productName, quantity) in itemsToBuy)
        {
            var expectedUnitPrice = expectedUnitPrices[productName];
            var actualUnitPrice = ParseCurrency(await cartPage.GetPriceAsync(productName));
            Assert.Equal(expectedUnitPrice, actualUnitPrice);

            var expectedSubtotal = expectedUnitPrice * quantity;
            var actualSubtotal = ParseCurrency(await cartPage.GetSubtotalAsync(productName));
            Assert.Equal(expectedSubtotal, actualSubtotal);

            expectedTotal += expectedSubtotal;
        }

        var actualTotal = ParseCurrency(await cartPage.GetTotalAsync());
        Assert.Equal(expectedTotal, actualTotal);
    }

    // The cart renders price/subtotal cells as "$12.99" but the grand total as "Total: 12.99";
    // this normalizes both formats to a comparable decimal.
    private static decimal ParseCurrency(string rawText)
    {
        var numericPart = rawText.Replace("Total:", string.Empty).Replace("$", string.Empty).Trim();
        return decimal.Parse(numericPart, CultureInfo.InvariantCulture);
    }
}
