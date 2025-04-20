using PasswordGenCLI.Common.Models;

namespace PasswordGenCLI.Common.Service;
public static class TablePrinter
{
    public static void PrintTable(IEnumerable<PasswordEntry> entries)
    {
        bool isColorSupported = IsColorSupported();

        const int serviceWidth = 15;
        const int loginWidth = 20;
        const int urlWidth = 30;
        const int noteWidth = 30;

        static string Format(string value, int width) =>
            value.Length > width ? string.Concat(value.AsSpan(0, width - 3), "...") : value.PadRight(width);

        if (isColorSupported)
        {
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        Console.WriteLine($"{Format("Service", serviceWidth)} {Format("Login", loginWidth)} {Format("URL", urlWidth)} {Format("Note", noteWidth)}");

        if (isColorSupported)
            Console.ResetColor();

        int index = 0;
        foreach (var entry in entries)
        {
            if (isColorSupported)
            {
                Console.BackgroundColor = index % 2 == 0 ? ConsoleColor.White : ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
            }

            Console.WriteLine($"{Format(entry.Service, serviceWidth)} {Format(entry.Login, loginWidth)} {Format(entry.Url, urlWidth)} {Format(entry.Note, noteWidth)}");

            if (isColorSupported)
                Console.ResetColor();

            index++;
        }
    }

    private static bool IsColorSupported()
    {
        if (OperatingSystem.IsWindows())
            return true;

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var term = Environment.GetEnvironmentVariable("TERM");
            return !string.IsNullOrEmpty(term) && !term.Equals("dumb", StringComparison.CurrentCultureIgnoreCase);
        }

        return false;
    }
}

