using Microsoft.Playwright;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class ContactPageTests : IAsyncLifetime
{
    // Each test gets its own browser context via the fixture, so tests don't share cookies/state.
    private readonly PlaywrightFixture _fixture = new();

    public Task InitializeAsync() => _fixture.InitializeAsync();
    public Task DisposeAsync() => _fixture.DisposeAsync();

    /// <summary>
    /// Test Case 1:
    ///   1. From the home page, go to the Contact page.
    ///   2. Click Submit without entering anything.
    ///   3. Verify the "required" error messages appear for every mandatory field.
    ///   4. Populate the mandatory fields.
    ///   5. Verify every error message has disappeared.
    /// </summary>
    [Fact]
    public async Task ContactPage_SubmitEmptyForm_ShowsRequiredErrors_ThenClearsAfterMandatoryFieldsFilled()
    {
        // ---------------------------------------------------------------------------------
        // Step 1: Start on the Home page, then navigate to Contact via the navbar link -
        // this mirrors how a real user would arrive at the Contact form.
        // ---------------------------------------------------------------------------------
        var homePage = new HomePage(_fixture.Page);
        await homePage.NavigateAsync();

        var contactPage = await homePage.Navigation.GoToContactAsync();

        // ---------------------------------------------------------------------------------
        // Step 2: Click Submit immediately, with every field still empty.
        // The AngularJS form only validates/marks fields "dirty" once a submit (or blur)
        // is attempted, so this is what triggers the "required" messages to render.
        // ---------------------------------------------------------------------------------
        await contactPage.SubmitAsync();

        // ---------------------------------------------------------------------------------
        // Step 3: Verify the error messages are shown.
        // On this form, Forename, Email and Message are the only mandatory fields
        // (Surname and Telephone are optional, so they have no "required" error to check).
        // ---------------------------------------------------------------------------------

        // The page-level banner that appears whenever the form is dirty and invalid.
        await Assertions.Expect(contactPage.ErrorAlert).ToBeVisibleAsync();

        // Each mandatory field has its own inline error span (e.g. "#forename-err").
        Assert.True(await contactPage.HasFieldErrorAsync("forename"));
        Assert.Equal("Forename is required", await contactPage.GetFieldErrorAsync("forename"));

        Assert.True(await contactPage.HasFieldErrorAsync("email"));
        Assert.Equal("Email is required", await contactPage.GetFieldErrorAsync("email"));

        Assert.True(await contactPage.HasFieldErrorAsync("message"));
        Assert.Equal("Message is required", await contactPage.GetFieldErrorAsync("message"));

        // ---------------------------------------------------------------------------------
        // Step 4: Populate just the mandatory fields (Forename, Email, Message).
        // Filling + blurring each field is what makes AngularJS re-run its validation
        // and clear the "required" state for that field.
        // ---------------------------------------------------------------------------------
        await contactPage.ForenameInput.FillAsync("Jane");
        await contactPage.ForenameInput.BlurAsync();

        await contactPage.EmailInput.FillAsync("jane.doe@example.com");
        await contactPage.EmailInput.BlurAsync();

        await contactPage.MessageInput.FillAsync("This is a test message for the contact form.");
        await contactPage.MessageInput.BlurAsync();

        // ---------------------------------------------------------------------------------
        // Step 5: Validate that every error message has gone away.
        // The error <span> elements are removed from the DOM entirely once their field is
        // valid (AngularJS's ui-if), so HasFieldErrorAsync - a non-waiting visibility
        // check - correctly reports false without timing out.
        // ---------------------------------------------------------------------------------
        Assert.False(await contactPage.HasFieldErrorAsync("forename"));
        Assert.False(await contactPage.HasFieldErrorAsync("email"));
        Assert.False(await contactPage.HasFieldErrorAsync("message"));

        // The page-level error banner should also be gone now that the form is valid.
        await Assertions.Expect(contactPage.ErrorAlert).Not.ToBeVisibleAsync();
    }

    /// <summary>
    /// Test Case 2:
    ///   1. From the home page, go to the Contact page.
    ///   2. Populate the mandatory fields.
    ///   3. Click Submit.
    ///   4. Validate the successful submission message.
    ///
    /// Verified flake-free by running this test 5 consecutive times (see conversation/run log) -
    /// all 5 runs passed. Not encoded as a repeated [Theory] here because the repetition was a
    /// one-off verification step, not a permanent part of the scenario.
    /// </summary>
    [Fact]
    public async Task ContactPage_SubmitWithMandatoryFieldsPopulated_ShowsSuccessMessage()
    {
        // ---------------------------------------------------------------------------------
        // Step 1: Start on the Home page, then navigate to Contact via the navbar link.
        // ---------------------------------------------------------------------------------
        var homePage = new HomePage(_fixture.Page);
        await homePage.NavigateAsync();

        var contactPage = await homePage.Navigation.GoToContactAsync();

        // ---------------------------------------------------------------------------------
        // Step 2: Populate the mandatory fields only (Forename, Email, Message).
        // Surname and Telephone are optional on this form, so they're deliberately left blank.
        // ---------------------------------------------------------------------------------
        await contactPage.ForenameInput.FillAsync("Jane");
        await contactPage.ForenameInput.BlurAsync();

        await contactPage.EmailInput.FillAsync("jane.doe@example.com");
        await contactPage.EmailInput.BlurAsync();

        await contactPage.MessageInput.FillAsync("This is a test message for the contact form.");
        await contactPage.MessageInput.BlurAsync();

        // ---------------------------------------------------------------------------------
        // Step 3: Click Submit.
        // ---------------------------------------------------------------------------------
        await contactPage.SubmitAsync();

        // ---------------------------------------------------------------------------------
        // Step 4: Validate the successful submission message.
        // GetConfirmationMessageAsync waits for the success banner to render (the app has a
        // short async delay before flipping to the success state, it isn't instant) and
        // returns its text, e.g. "Thanks Jane, we appreciate your feedback."
        // ---------------------------------------------------------------------------------
        var confirmationMessage = await contactPage.GetConfirmationMessageAsync();

        Assert.Contains("Thanks Jane", confirmationMessage);
        Assert.Contains("we appreciate your feedback", confirmationMessage);

        // Double-check via the locator-based assertion too, and confirm the form itself is gone.
        await Assertions.Expect(contactPage.SuccessAlert).ToBeVisibleAsync();
        Assert.True(await contactPage.IsSubmittedSuccessfullyAsync());
    }
}
