using System.Text.RegularExpressions;
using Microsoft.Playwright;
using JupiterToys.Automation.Models;

namespace JupiterToys.Automation.PageObjects;

public class CheckoutPage : BasePage
{
    protected override string Path => "/#/checkout";

    public CheckoutPage(IPage page) : base(page)
    {
    }

    // Shown when the cart is empty - replaces the delivery/payment form entirely.
    public ILocator EmptyCartAlert => Page.Locator(".alert:has-text('Your cart is empty')");
    public ILocator StartShoppingLink => Page.Locator("a.btn-success[href='#/shop']");

    public ILocator InfoAlert => Page.Locator(".alert-info");
    public ILocator ErrorAlert => Page.Locator(".alert-error");

    public ILocator ForenameInput => Page.Locator("#forename");
    public ILocator SurnameInput => Page.Locator("#surname");
    public ILocator EmailInput => Page.Locator("#email");
    public ILocator TelephoneInput => Page.Locator("#telephone");
    public ILocator AddressInput => Page.Locator("#address");
    public ILocator CardTypeSelect => Page.Locator("#cardType");
    public ILocator CardNumberInput => Page.Locator("#card");
    public ILocator SubmitButton => Page.Locator("#checkout-submit-btn");

    public ILocator ForenameError => Page.Locator("#forename-err");
    public ILocator EmailError => Page.Locator("#email-err");

    private ILocator ProcessingModal => Page.Locator("div.popup.modal:has-text('Processing Order')");
    public ILocator SuccessAlert => Page.Locator(".alert-success");
    public ILocator ShoppingAgainLink => Page.Locator("a.btn-success[href='#/shop']");

    public async Task FillDeliveryAndPaymentDetailsAsync(CheckoutDetails details)
    {
        await ForenameInput.FillAsync(details.Forename);
        await ForenameInput.BlurAsync();
        if (details.Surname is not null)
        {
            await SurnameInput.FillAsync(details.Surname);
            await SurnameInput.BlurAsync();
        }
        await EmailInput.FillAsync(details.Email);
        await EmailInput.BlurAsync();
        if (details.Telephone is not null)
        {
            await TelephoneInput.FillAsync(details.Telephone);
            await TelephoneInput.BlurAsync();
        }
        await AddressInput.FillAsync(details.Address);
        await AddressInput.BlurAsync();
        await CardTypeSelect.SelectOptionAsync(details.CardType);
        await CardNumberInput.FillAsync(details.CardNumber);
        await CardNumberInput.BlurAsync();
    }

    // Waits out the "Processing Order" modal, whose duration is randomized by the app.
    public async Task<OrderConfirmation> SubmitOrderAsync()
    {
        await SubmitButton.ClickAsync();
        await ProcessingModal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 30000
        });

        var message = (await SuccessAlert.InnerTextAsync()).Trim();
        var match = Regex.Match(message, @"order nu+mber is\s*(\S+)", RegexOptions.IgnoreCase);
        return new OrderConfirmation(message, match.Success ? match.Groups[1].Value : null);
    }

    public async Task<string> GetFieldErrorAsync(string fieldId)
    {
        return (await Page.Locator($"#{fieldId}-err").InnerTextAsync()).Trim();
    }

    public async Task<bool> IsCartEmptyAsync()
    {
        return await EmptyCartAlert.IsVisibleAsync();
    }
}
