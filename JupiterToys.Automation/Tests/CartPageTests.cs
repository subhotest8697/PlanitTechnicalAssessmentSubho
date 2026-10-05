using System.Globalization;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class CartPageTests : IAsyncLifetime
{
    // Each test gets its own browser context via the fixture, so tests don't share cookies/state.
    private readonly PlaywrightFixture _fixture = new();

    public Task InitializeAsync() => _fixture.InitializeAsync();
    public Task DisposeAsync() => _fixture.DisposeAsync();

    /// <summary>
    /// Test Case 3:
    ///   1. Buy 2 Stuffed Frog, 5 Fluffy Bunny, 3 Valentine Bear.
    ///   2. Go to the Cart page.
    ///   3. Verify the subtotal for each product is correct.
    ///   4. Verify the price for each product.
    ///   5. Verify that total = sum(subtotals).
    /// </summary>
    [Fact]
    public async Task CartPage_WithMultipleProductsAndQuantities_CalculatesPricesAndTotalCorrectly()
    {
        // ---------------------------------------------------------------------------------
        // The quantities to buy for each product. Using a dictionary keeps the "what to buy"
        // data separate from the "how to verify it" logic below, so this list is the single
        // source of truth the rest of the test is checked against.
        // ---------------------------------------------------------------------------------
        var itemsToBuy = new Dictionary<string, int>
        {
            ["Stuffed Frog"] = 2,
            ["Fluffy Bunny"] = 5,
            ["Valentine Bear"] = 3
        };

        // ---------------------------------------------------------------------------------
        // Step 1: Buy each product the requested number of times.
        // Clicking "Buy" repeatedly on the same product increments its quantity in the cart
        // rather than adding duplicate rows (confirmed against the live app), so N clicks
        // buys N units.
        // ---------------------------------------------------------------------------------
        var shopPage = new ShopPage(_fixture.Page);
        await shopPage.NavigateAsync();

        // Capture each product's advertised unit price *before* buying/navigating away - once
        // we move to the Cart page, the Shop page's product list is no longer in the DOM.
        var expectedUnitPrices = new Dictionary<string, decimal>();
        foreach (var (productName, quantity) in itemsToBuy)
        {
            expectedUnitPrices[productName] = ParseCurrency(await shopPage.GetProductPriceAsync(productName));
            await shopPage.BuyProductAsync(productName, quantity);
        }

        // ---------------------------------------------------------------------------------
        // Step 2: Go to the Cart page.
        // ---------------------------------------------------------------------------------
        var cartPage = await shopPage.Navigation.GoToCartAsync();

        // ---------------------------------------------------------------------------------
        // Steps 3 & 4: For each product, verify its unit price matches what the Shop page
        // advertised, and that its subtotal equals price x quantity bought.
        // ---------------------------------------------------------------------------------
        decimal expectedTotal = 0m;

        foreach (var (productName, quantity) in itemsToBuy)
        {
            // Step 4: the unit price shown in the cart must match the Shop page's price -
            // comparing against what we actually read from Shop (rather than hardcoding the
            // dollar amount) means this test keeps working even if catalog prices change.
            var expectedUnitPrice = expectedUnitPrices[productName];
            var actualUnitPrice = ParseCurrency(await cartPage.GetPriceAsync(productName));
            Assert.Equal(expectedUnitPrice, actualUnitPrice);

            // Step 3: the subtotal for this line must equal unit price x quantity.
            var expectedSubtotal = expectedUnitPrice * quantity;
            var actualSubtotal = ParseCurrency(await cartPage.GetSubtotalAsync(productName));
            Assert.Equal(expectedSubtotal, actualSubtotal);

            expectedTotal += expectedSubtotal;
        }

        // ---------------------------------------------------------------------------------
        // Step 5: the cart's grand total must equal the sum of every line's subtotal.
        // ---------------------------------------------------------------------------------
        var actualTotal = ParseCurrency(await cartPage.GetTotalAsync());
        Assert.Equal(expectedTotal, actualTotal);
    }

    /// <summary>
    /// Strips a leading "Total: " label and/or "$" sign (the cart renders price/subtotal
    /// cells as "$12.99" but the grand total as "Total: 12.99") and parses what's left as
    /// a decimal, so both cell formats can be compared on equal footing.
    /// </summary>
    private static decimal ParseCurrency(string rawText)
    {
        var numericPart = rawText.Replace("Total:", string.Empty).Replace("$", string.Empty).Trim();
        return decimal.Parse(numericPart, CultureInfo.InvariantCulture);
    }
}
