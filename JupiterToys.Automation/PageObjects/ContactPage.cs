using Microsoft.Playwright;
using JupiterToys.Automation.Models;

namespace JupiterToys.Automation.PageObjects;

public class ContactPage : BasePage
{
    protected override string Path => "/#/contact";

    public ContactPage(IPage page) : base(page)
    {
    }

    public ILocator InfoAlert => Page.Locator(".alert-info");
    public ILocator ErrorAlert => Page.Locator(".alert-error");
    public ILocator SuccessAlert => Page.Locator(".alert-success");

    public ILocator ForenameInput => Page.Locator("#forename");
    public ILocator SurnameInput => Page.Locator("#surname");
    public ILocator EmailInput => Page.Locator("#email");
    public ILocator TelephoneInput => Page.Locator("#telephone");
    public ILocator MessageInput => Page.Locator("#message");
    public ILocator SubmitButton => Page.Locator("a.btn-contact");

    // The form is removed from the DOM (ui-if="!contactValidSubmit") once the submission succeeds.
    private ILocator Form => Page.Locator("div[ui-if='!contactValidSubmit']");

    public async Task FillContactFormAsync(ContactMessage contact)
    {
        await ForenameInput.FillAsync(contact.Forename);
        await ForenameInput.BlurAsync();
        if (contact.Surname is not null)
        {
            await SurnameInput.FillAsync(contact.Surname);
            await SurnameInput.BlurAsync();
        }
        await EmailInput.FillAsync(contact.Email);
        await EmailInput.BlurAsync();
        if (contact.Telephone is not null)
        {
            await TelephoneInput.FillAsync(contact.Telephone);
            await TelephoneInput.BlurAsync();
        }
        await MessageInput.FillAsync(contact.Message);
        await MessageInput.BlurAsync();
    }

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task<string> GetFieldErrorAsync(string fieldId)
    {
        return (await Page.Locator($"#{fieldId}-err").InnerTextAsync()).Trim();
    }

    // Non-waiting check: the error <span> is removed from the DOM (not just hidden) once the
    // field is valid, so GetFieldErrorAsync would hang waiting for an element that never appears.
    public async Task<bool> HasFieldErrorAsync(string fieldId)
    {
        return await Page.Locator($"#{fieldId}-err").IsVisibleAsync();
    }

    public async Task<bool> IsSubmittedSuccessfullyAsync()
    {
        try
        {
            await Form.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 30000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    // The success banner appears after a randomized server-side delay, hence the generous timeout.
    public async Task<string> GetConfirmationMessageAsync()
    {
        await SuccessAlert.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });
        return (await SuccessAlert.InnerTextAsync()).Trim();
    }
}
