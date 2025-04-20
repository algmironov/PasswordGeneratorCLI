namespace PasswordGenCLI.Common.Models;

public class PasswordEntry
{
    public required string Service { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}