# Jupiter Toys Automation

Playwright + xUnit UI test suite for [Jupiter Toys](https://jupiter.cloud.planittesting.com), PlanIT's demo e-commerce site, plus a small reporting tool that turns a `dotnet test` run into a JSON/HTML summary.

## Tech stack

- .NET 10 / C#
- [Microsoft.Playwright](https://playwright.dev/dotnet/) for browser automation
- xUnit as the test runner

## Project structure

```
JupiterToys.Automation/              Test project
  Fixtures/PlaywrightFixture.cs       Browser lifecycle (one browser per test via IAsyncLifetime)
  PageObjects/                        Page object model: Home, Shop, Cart, Checkout, Contact, Login modal, Navbar
  Models/                             Plain data records passed into page objects (ContactMessage, CheckoutDetails, OrderConfirmation)
  Tests/                              Test classes
    ContactPageTests.cs                Contact form validation + successful submission (run 5x)
    CartPageTests.cs                   Cart pricing/subtotal/total calculation across multiple products

JupiterToys.Automation.Reporting/    Console tool: TRX -> JSON + HTML report
```

## Prerequisites

- .NET 10 SDK
- A browser for Playwright to drive. Either:
  - run `playwright install chromium` once (uses the Playwright CDN), or
  - have Chrome or Edge installed locally — `PlaywrightFixture` automatically falls back to a local Chrome/Edge install if the Playwright-managed browser isn't available (useful behind a proxy that blocks the Playwright CDN)

## Running the tests

```bash
dotnet restore
dotnet test
```

This runs against the live `jupiter.cloud.planittesting.com` site, so an internet connection is required.

### Test coverage

| Test | What it checks |
|---|---|
| `ContactPage_SubmitEmptyForm_ShowsRequiredErrors_ThenClearsAfterMandatoryFieldsFilled` | Submitting the Contact form empty shows a "required" error for each mandatory field; filling them in clears every error |
| `ContactPage_SubmitWithMandatoryFieldsPopulated_ShowsSuccessMessage` | Submitting the Contact form with valid mandatory fields shows the success confirmation. Runs 5 times (`[Theory]`/`[InlineData]`) so CI reports 5 independent pass/fail results |
| `CartPage_WithMultipleProductsAndQuantities_CalculatesPricesAndTotalCorrectly` | Buying several products in different quantities, the Cart shows the correct unit price and subtotal per line, and a grand total equal to the sum of all subtotals |

## Generating a test report

After `dotnet test` produces a `.trx` file (e.g. via `--logger "trx;LogFileName=results.trx"`), convert it to a JSON + HTML report:

```bash
dotnet run --project JupiterToys.Automation.Reporting -- <path-to-results.trx> [output-directory]
```

This writes `report.json` and `report.html` (defaulting to a `report/` folder next to the `.trx` file) and exits non-zero if any test failed.

## CI/CD

Builds run in TeamCity under the build configuration
[`PlanitTechnicalAssessmentSubho_JupiterToysAutomation`](https://planittesting.teamcity.com/buildConfiguration/PlanitTechnicalAssessmentSubho_JupiterToysAutomation),
triggered on commits to `main`.

### Build steps

1. **Restore** — `dotnet restore`
2. **Install browsers** — `pwsh JupiterToys.Automation/bin/Debug/net10.0/playwright.ps1 install chromium`
   (or rely on the fixture's local Chrome/Edge fallback — see [`PlaywrightFixture`](JupiterToys.Automation/Fixtures/PlaywrightFixture.cs) — if the
   build agent can't reach the Playwright CDN)
3. **Build** — `dotnet build --configuration Release`
4. **Test** — `dotnet test --configuration Release --logger "trx;LogFileName=results.trx"`
   - Runs headless against the live `jupiter.cloud.planittesting.com` site, so the build agent needs outbound internet access
   - Produces `TestResults/results.trx`
5. **Report** — `dotnet run --project JupiterToys.Automation.Reporting --configuration Release -- TestResults/results.trx TestResults/report`
   - Converts the `.trx` into `report.json` and `report.html`; exits non-zero if any test failed, which should fail the build step
6. **Publish artifacts** — archive `TestResults/results.trx`, `TestResults/report/report.json`, and `TestResults/report/report.html` so results are viewable from the build's Artifacts tab

### Notes for anyone editing the build configuration

- The agent needs the .NET 10 SDK installed and (ideally) outbound access to the Playwright CDN; if the agent sits behind a proxy that blocks it, `PlaywrightFixture` will fall back to a Chrome/Edge binary already present on the agent, so installing Chrome on the build agent is a safe alternative.
- Tests hit a shared public demo site rather than a disposable test environment, so a flaky/slow response from `jupiter.cloud.planittesting.com` can fail the build independently of the code under test — re-running the build is a reasonable first response to an unexplained failure.
- `ContactPage_SubmitWithMandatoryFieldsPopulated_ShowsSuccessMessage` runs 5 times per build (one `[Theory]` row each) and should show as 5 separate results in the TeamCity test report, not one.
