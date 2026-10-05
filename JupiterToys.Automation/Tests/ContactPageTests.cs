using Microsoft.Playwright;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class ContactPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture = new(); // owns the browser/page used by every test in this class

    public Task InitializeAsync() => _fixture.InitializeAsync(); // launches a fresh browser before each test
    public Task DisposeAsync() => _fixture.DisposeAsync();       // closes it afterwards

    // Reason: validates that the Contact form guides users correctly - submitting with
    // required fields empty must show a clear "required" error per field, and those errors
    // must disappear once the user fixes them. Without this, users could be stuck unable to
    // tell why submission is blocked, or see stale errors after correcting their input.
    [Fact]
    public async Task ContactPage_SubmitEmptyForm_ShowsRequiredErrors_ThenClearsAfterMandatoryFieldsFilled()
    {
        var contactPage = await NavigateToContactPageAsync(); // Home -> Contact, as a real user would

        await contactPage.SubmitAsync(); // click Submit while every field is still empty

        // every mandatory field should now show a "required" error
        await Assertions.Expect(contactPage.ErrorAlert).ToBeVisibleAsync();                        // page-level error banner is visible
        Assert.True(await contactPage.HasFieldErrorAsync("forename"));                             // Forename error is showing
        Assert.Equal("Forename is required", await contactPage.GetFieldErrorAsync("forename"));    // with the expected text
        Assert.True(await contactPage.HasFieldErrorAsync("email"));                                // Email error is showing
        Assert.Equal("Email is required", await contactPage.GetFieldErrorAsync("email"));          // with the expected text
        Assert.True(await contactPage.HasFieldErrorAsync("message"));                              // Message error is showing
        Assert.Equal("Message is required", await contactPage.GetFieldErrorAsync("message"));      // with the expected text

        await FillMandatoryFieldsAsync(contactPage); // fill Forename, Email and Message with valid values

        // every error should now have cleared
        Assert.False(await contactPage.HasFieldErrorAsync("forename"));                  // Forename error is gone
        Assert.False(await contactPage.HasFieldErrorAsync("email"));                     // Email error is gone
        Assert.False(await contactPage.HasFieldErrorAsync("message"));                   // Message error is gone
        await Assertions.Expect(contactPage.ErrorAlert).Not.ToBeVisibleAsync();          // page-level error banner is gone too
    }

    // Reason: validates the Contact form's happy path - once every mandatory field is filled
    // in correctly, submission must succeed and the user must see a clear confirmation that
    // their feedback was received. This is the core function of the page; if it silently
    // failed to submit, the business would lose customer feedback without anyone noticing.
    [Fact]
    public async Task ContactPage_SubmitWithMandatoryFieldsPopulated_ShowsSuccessMessage()
    {
        var contactPage = await NavigateToContactPageAsync(); // Home -> Contact

        await FillMandatoryFieldsAsync(contactPage); // populate Forename, Email and Message
        await contactPage.SubmitAsync();             // submit the form

        var confirmationMessage = await contactPage.GetConfirmationMessageAsync(); // wait for and read the success banner

        Assert.Contains("Thanks Jane", confirmationMessage);                        // greets the submitter by name
        Assert.Contains("we appreciate your feedback", confirmationMessage);        // and confirms the feedback was received
        await Assertions.Expect(contactPage.SuccessAlert).ToBeVisibleAsync();       // the success banner itself is visible
        Assert.True(await contactPage.IsSubmittedSuccessfullyAsync());              // and the form has been replaced by it
    }

    private async Task<ContactPage> NavigateToContactPageAsync()
    {
        var homePage = new HomePage(_fixture.Page); // start from the Home page
        await homePage.NavigateAsync();             // load it
        return await homePage.Navigation.GoToContactAsync(); // click through to Contact via the navbar
    }

    // Surname and Telephone are optional on this form and are deliberately left blank.
    private static async Task FillMandatoryFieldsAsync(ContactPage contactPage)
    {
        await contactPage.ForenameInput.FillAsync("Jane"); // type a forename
        await contactPage.ForenameInput.BlurAsync();       // leave the field, triggering validation

        await contactPage.EmailInput.FillAsync("jane.doe@example.com"); // type a valid email
        await contactPage.EmailInput.BlurAsync();                       // leave the field, triggering validation

        await contactPage.MessageInput.FillAsync("This is a test message for the contact form."); // type a message
        await contactPage.MessageInput.BlurAsync();                                                // leave the field, triggering validation
    }
}
