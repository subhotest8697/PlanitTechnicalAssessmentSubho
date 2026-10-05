using Microsoft.Playwright;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class ContactPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture = new();

    public Task InitializeAsync() => _fixture.InitializeAsync();
    public Task DisposeAsync() => _fixture.DisposeAsync();

    [Fact]
    public async Task ContactPage_SubmitEmptyForm_ShowsRequiredErrors_ThenClearsAfterMandatoryFieldsFilled()
    {
        var contactPage = await NavigateToContactPageAsync();

        await contactPage.SubmitAsync();

        await Assertions.Expect(contactPage.ErrorAlert).ToBeVisibleAsync();
        Assert.True(await contactPage.HasFieldErrorAsync("forename"));
        Assert.Equal("Forename is required", await contactPage.GetFieldErrorAsync("forename"));
        Assert.True(await contactPage.HasFieldErrorAsync("email"));
        Assert.Equal("Email is required", await contactPage.GetFieldErrorAsync("email"));
        Assert.True(await contactPage.HasFieldErrorAsync("message"));
        Assert.Equal("Message is required", await contactPage.GetFieldErrorAsync("message"));

        await FillMandatoryFieldsAsync(contactPage);

        Assert.False(await contactPage.HasFieldErrorAsync("forename"));
        Assert.False(await contactPage.HasFieldErrorAsync("email"));
        Assert.False(await contactPage.HasFieldErrorAsync("message"));
        await Assertions.Expect(contactPage.ErrorAlert).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task ContactPage_SubmitWithMandatoryFieldsPopulated_ShowsSuccessMessage()
    {
        var contactPage = await NavigateToContactPageAsync();

        await FillMandatoryFieldsAsync(contactPage);
        await contactPage.SubmitAsync();

        var confirmationMessage = await contactPage.GetConfirmationMessageAsync();

        Assert.Contains("Thanks Jane", confirmationMessage);
        Assert.Contains("we appreciate your feedback", confirmationMessage);
        await Assertions.Expect(contactPage.SuccessAlert).ToBeVisibleAsync();
        Assert.True(await contactPage.IsSubmittedSuccessfullyAsync());
    }

    private async Task<ContactPage> NavigateToContactPageAsync()
    {
        var homePage = new HomePage(_fixture.Page);
        await homePage.NavigateAsync();
        return await homePage.Navigation.GoToContactAsync();
    }

    // Surname and Telephone are optional on this form and are deliberately left blank.
    private static async Task FillMandatoryFieldsAsync(ContactPage contactPage)
    {
        await contactPage.ForenameInput.FillAsync("Jane");
        await contactPage.ForenameInput.BlurAsync();

        await contactPage.EmailInput.FillAsync("jane.doe@example.com");
        await contactPage.EmailInput.BlurAsync();

        await contactPage.MessageInput.FillAsync("This is a test message for the contact form.");
        await contactPage.MessageInput.BlurAsync();
    }
}
