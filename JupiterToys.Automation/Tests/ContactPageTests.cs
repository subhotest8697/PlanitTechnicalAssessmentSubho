using Microsoft.Playwright;
using JupiterToys.Automation.Fixtures;
using JupiterToys.Automation.PageObjects;

namespace JupiterToys.Automation.Tests;

public class ContactPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture = new(); // owns the browser/page used by every test in this class

    public Task InitializeAsync() => _fixture.InitializeAsync(); // launches a fresh browser before each test
    public Task DisposeAsync() => _fixture.DisposeAsync();       // closes it afterwards

    /// <summary>
    /// Test Case 1: Contact Form - Mandatory Field Validation
    ///
    /// Summary: Navigates from the Home page to Contact, submits the form empty, verifies a
    /// "required" error appears for each mandatory field (Forename, Email, Message), then
    /// fills those fields in and verifies every error disappears.
    ///
    /// Business Context: The Contact form is a primary channel for customers to reach the
    /// business directly. If validation doesn't clearly flag missing required fields,
    /// customers may abandon the form in confusion, or submit incomplete enquiries that staff
    /// can't action (e.g. no email address to reply to). This test confirms the form blocks
    /// incomplete submissions with clear, field-specific errors, and that those errors clear
    /// once corrected so the user isn't left thinking something is still wrong.
    /// </summary>
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

    /// <summary>
    /// Test Case 2: Contact Form - Successful Submission
    ///
    /// Summary: Navigates from the Home page to Contact, fills in the mandatory fields
    /// (Forename, Email, Message) with valid values, submits the form, and verifies the
    /// success banner confirms the submission.
    ///
    /// Business Context: This is the Contact form's core "happy path" - a customer with a
    /// genuine enquiry or piece of feedback fills in the form and expects confirmation that
    /// it reached the business. If submission silently failed or gave no confirmation,
    /// customer feedback would be lost without the customer or the business ever knowing.
    /// </summary>
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
