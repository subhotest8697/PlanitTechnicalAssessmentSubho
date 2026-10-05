using System.Globalization;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class CartPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture = new(); // owns the browser/page used by every test in this class

    public Task InitializeAsync() => _fixture.InitializeAsync(); // launches a fresh browser before each test
    public Task DisposeAsync() => _fixture.DisposeAsync();       // closes it afterwards

    /// <summary>
    /// Test Case 3: Shopping Cart - Pricing and Total Calculation Accuracy
    ///
    /// Summary: Buys 2x Stuffed Frog, 5x Fluffy Bunny and 3x Valentine Bear from the Shop
    /// page, then on the Cart page verifies each product's unit price matches what Shop
    /// advertised, each line's subtotal equals price x quantity, and the grand total equals
    /// the sum of all subtotals.
    ///
    /// Business Context: Cart pricing is the most financially sensitive calculation on an
    /// e-commerce site - customers are charged based on exactly what the cart displays. An
    /// error in a line's unit price, its subtotal, or the grand total could mean customers
    /// are undercharged (direct revenue loss) or overcharged (customer trust and compliance
    /// risk). This test validates that pricing math holds across multiple products bought in
    /// different quantities within a single basket.
    /// </summary>
    [Fact]
    public async Task CartPage_WithMultipleProductsAndQuantities_CalculatesPricesAndTotalCorrectly()
    {
        var itemsToBuy = new Dictionary<string, int> // the products and quantities to buy for this test
        {
            ["Stuffed Frog"] = 2,
            ["Fluffy Bunny"] = 5,
            ["Valentine Bear"] = 3
        };

        var shopPage = new ShopPage(_fixture.Page); // the Shop page we'll buy from
        await shopPage.NavigateAsync();             // load it

        var expectedUnitPrices = new Dictionary<string, decimal>(); // unit prices read from Shop, to compare against the Cart later
        foreach (var (productName, quantity) in itemsToBuy)
        {
            expectedUnitPrices[productName] = ParseCurrency(await shopPage.GetProductPriceAsync(productName)); // record the advertised price
            await shopPage.BuyProductAsync(productName, quantity);                                             // buy the requested quantity
        }

        var cartPage = await shopPage.Navigation.GoToCartAsync(); // go to the Cart page

        var expectedTotal = 0m; // running total of every line's subtotal, to check against the cart's grand total

        foreach (var (productName, quantity) in itemsToBuy)
        {
            var expectedUnitPrice = expectedUnitPrices[productName];                 // price captured earlier on Shop
            var actualUnitPrice = ParseCurrency(await cartPage.GetPriceAsync(productName)); // price now shown in Cart
            Assert.Equal(expectedUnitPrice, actualUnitPrice);                         // they must match

            var expectedSubtotal = expectedUnitPrice * quantity;                           // unit price x quantity bought
            var actualSubtotal = ParseCurrency(await cartPage.GetSubtotalAsync(productName)); // subtotal shown in Cart
            Assert.Equal(expectedSubtotal, actualSubtotal);                                // they must match

            expectedTotal += expectedSubtotal; // accumulate for the grand-total check below
        }

        var actualTotal = ParseCurrency(await cartPage.GetTotalAsync()); // the cart's displayed grand total
        Assert.Equal(expectedTotal, actualTotal);                        // must equal the sum of all subtotals
    }

    // The cart renders price/subtotal cells as "$12.99" but the grand total as "Total: 12.99";
    // this normalizes both formats to a comparable decimal.
    private static decimal ParseCurrency(string rawText)
    {
        var numericPart = rawText.Replace("Total:", string.Empty).Replace("$", string.Empty).Trim(); // strip label/currency sign
        return decimal.Parse(numericPart, CultureInfo.InvariantCulture);                              // parse what's left
    }
}
