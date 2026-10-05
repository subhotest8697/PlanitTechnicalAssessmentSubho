namespace JupiterToys.Automation.Models;

public record ContactMessage(
    string Forename,
    string Email,
    string Message,
    string? Surname = null,
    string? Telephone = null);
