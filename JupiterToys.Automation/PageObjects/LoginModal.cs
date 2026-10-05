using Microsoft.Playwright;

namespace JupiterToys.Automation.PageObjects;

/// <summary>
/// Login on Jupiter Toys is a Bootstrap modal popup opened from the navbar, not a routed page
/// (there is no register page/modal on this site - login is the only auth entry point).
/// </summary>
public class LoginModal
{
    private readonly IPage _page;
    private ILocator Modal => _page.Locator("div.popup.modal.in");

    public LoginModal(IPage page)
    {
        _page = page;
    }

    public ILocator UsernameInput => Modal.Locator("#loginUserName");
    public ILocator PasswordInput => Modal.Locator("#loginPassword");
    public ILocator LoginButton => Modal.Locator("button[type='submit']");
    public ILocator CancelButton => Modal.Locator("button.btn-cancel");
    public ILocator MessageContainer => Modal.Locator("#messageContainer");

    public async Task WaitForOpenAsync()
    {
        await Modal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public async Task CancelAsync()
    {
        await CancelButton.ClickAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        return (await MessageContainer.InnerTextAsync()).Trim();
    }

    public async Task<bool> IsOpenAsync()
    {
        return await Modal.IsVisibleAsync();
    }
}
