using Microsoft.Playwright;

namespace JupiterToys.Automation.PageObjects;

/// <summary>
/// The navbar is shared across every route (#/home, #/shop, #/cart, #/checkout, #/contact),
/// so it's modeled as a component rather than duplicated per page.
/// </summary>
public class NavigationComponent
{
    private readonly IPage _page;

    public NavigationComponent(IPage page)
    {
        _page = page;
    }

    public ILocator BrandLink => _page.Locator("a.brand");
    public ILocator HomeLink => _page.Locator("a[href='#/home']");
    public ILocator ShopLink => _page.Locator("a[href='#/shop']");
    public ILocator ContactLink => _page.Locator("a[href='#/contact']");
    public ILocator LoginLink => _page.Locator("#nav-login a");
    public ILocator LogoutLink => _page.Locator("#nav-logout a");
    public ILocator CartLink => _page.Locator("a[href='#/cart']");
    public ILocator CartCount => _page.Locator(".cart-count");
    public ILocator LoggedInUsername => _page.Locator("#nav-user .user");

    public async Task<HomePage> GoToHomeAsync()
    {
        await HomeLink.ClickAsync();
        return new HomePage(_page);
    }

    public async Task<ShopPage> GoToShopAsync()
    {
        await ShopLink.ClickAsync();
        return new ShopPage(_page);
    }

    public async Task<ContactPage> GoToContactAsync()
    {
        await ContactLink.ClickAsync();
        return new ContactPage(_page);
    }

    public async Task<CartPage> GoToCartAsync()
    {
        await CartLink.ClickAsync();
        return new CartPage(_page);
    }

    public async Task<LoginModal> OpenLoginModalAsync()
    {
        await LoginLink.ClickAsync();
        var modal = new LoginModal(_page);
        await modal.WaitForOpenAsync();
        return modal;
    }

    public async Task LogoutAsync()
    {
        await LogoutLink.ClickAsync();
    }

    public async Task<int> GetCartCountAsync()
    {
        var text = await CartCount.InnerTextAsync();
        return int.Parse(text.Trim());
    }

    public async Task<bool> IsLoggedInAsync()
    {
        return await LogoutLink.IsVisibleAsync();
    }
}
