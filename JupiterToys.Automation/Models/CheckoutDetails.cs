namespace JupiterToys.Automation.Models;

public record CheckoutDetails(
    string Forename,
    string Email,
    string Address,
    string CardType,
    string CardNumber,
    string? Surname = null,
    string? Telephone = null);
