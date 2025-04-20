﻿using System.Security.Cryptography;
using System.Text.Json;

using PasswordGenCLI.Common.Models;

using TextCopy;

namespace PasswordGenCLI.Common.Service;
public class EncryptionService
{
    public static void InitializeStorage()
    {
        string storagePath = GetStoragePath();

        if (File.Exists(storagePath))
        {
            Console.WriteLine("Password storage already exists. Do you want to overwrite it? (y/n)");

            // TODO manage invalid input
            string response = Console.ReadLine().ToLower();

            if (response == null || response != "y" && response != "yes")
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }
        }

        Console.WriteLine("Creating new password storage");
        Console.WriteLine("WARNING: If you forget your master password, your stored passwords CANNOT be recovered!");

        string masterPassword = ReadMasterPassword();
        string confirmPassword = ReadMasterPassword("Confirm master password: ");

        if (masterPassword != confirmPassword)
        {
            Console.WriteLine("Passwords don't match. Please try again.");
            return;
        }

        var storage = new PasswordStorage();
        string json = JsonSerializer.Serialize(storage);

        byte[] encrypted = Encrypt(json, masterPassword);
        File.WriteAllBytes(storagePath, encrypted);

        Console.Clear();
        Console.WriteLine("Password storage initialized successfully.");
    }

    public static void AddNewPassword(string service, string login, string url = "", string note = "")
    {
        string masterPassword = ReadMasterPassword();

        var storage = LoadStorage(masterPassword);
        if (storage == null) return;

        if (storage.Entries.Exists(e => e.Service.Equals(service, StringComparison.OrdinalIgnoreCase) &&
                                        e.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Password for {service} with login {login} already exists. Use 'update' command to change it.");
            return;
        }

        Console.WriteLine("Do you want to generate a new password? (y/n)");

        // TODO manage invalid input
        string generateNew = Console.ReadLine().ToLower();

        string password;
        if (generateNew == "y" || generateNew == "yes")
        {
            Console.Write("Password length (default 14): ");

            // TODO manage invalid input
            string lengthInput = Console.ReadLine();
            int length = string.IsNullOrEmpty(lengthInput) ? 14 : int.Parse(lengthInput);

            Console.Write("Include special characters? (y/n, default: n): ");

            // TODO manage invalid input
            string useSpecial = Console.ReadLine().ToLower();
            bool useSymbols = useSpecial == "y" || useSpecial == "yes";

            password = PasswordGenerator.Generate(length, PasswordGenerator.GetDefaultSymbols(), useSymbols);
        }
        else
        {
            password = ReadMasterPassword("Enter password to store: ");
        }

        storage.Entries.Add(new PasswordEntry
        {
            Service = service,
            Login = login,
            Password = password,
            Url = url,
            Note = note
        });

        SaveStorage(storage, masterPassword);
        Console.WriteLine($"Password for {service} with login {login} saved successfully.");
    }

    public static void ReadPasswords(string service, bool list, int clipboardTimeout)
    {
        string masterPassword = ReadMasterPassword();
        var storage = LoadStorage(masterPassword);

        if (storage == null) return;

        if (list)
        {
            if (storage.Entries.Count == 0)
            {
                Console.WriteLine("No passwords stored.");
                return;
            }

            TablePrinter.PrintTable(storage.Entries);
        }
        else if (!string.IsNullOrEmpty(service))
        {
            var entries = storage.Entries.FindAll(e => e.Service.Equals(service, StringComparison.OrdinalIgnoreCase));

            if (entries.Count == 0)
            {
                Console.WriteLine($"No passwords found for service: {service}");
                return;
            }

            if (entries.Count == 1)
            {
                var entry = entries[0];
                Console.WriteLine($"Service: {entry.Service}");

                if (!string.IsNullOrEmpty(entry.Url))
                {
                    Console.WriteLine($"Url: {entry.Url}");
                }

                Console.WriteLine($"Login: {entry.Login}");

                ClipboardService.SetText(entry.Password);
                Console.WriteLine("Password has been copied to clipboard.");
                
                if (clipboardTimeout > 0)
                {
                    Console.WriteLine($"Clipboard will be cleared in {clipboardTimeout} seconds.");

                    Task.Run(async () => {
                        await Task.Delay(clipboardTimeout * 1000);
                        ClipboardService.SetText(string.Empty);
                        Console.WriteLine("Clipboard has been cleared.");
                    });
                }
            }
            else
            {
                Console.WriteLine($"Multiple entries found for {service}:");
                for (int i = 1; i < entries.Count; i++)
                {
                    Console.WriteLine($"{i}. Login: {entries[i].Login}");
                }

                Console.Write("Enter number to copy password: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= entries.Count)
                {
                    var entry = entries[index - 1];
                    Console.WriteLine($"Service: {entry.Service}");
                    Console.WriteLine($"Login: {entry.Login}");
                    
                    ClipboardService.SetText(entry.Password);
                    Console.WriteLine("Password has been copied to clipboard.");

                    if (clipboardTimeout > 0)
                    {
                        Console.WriteLine($"Clipboard will be cleared in {clipboardTimeout} seconds.");

                        Task.Run(async () => {
                            await Task.Delay(clipboardTimeout * 1000);
                            ClipboardService.SetText(string.Empty);
                            Console.WriteLine("Clipboard has been cleared.");
                        });
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }
        }
        else
        {
            Console.WriteLine("Please specify either --service or --list option.");
        }
    }

    public static void UpdatePassword(string service, string login)
    {
        string masterPassword = ReadMasterPassword();
        var storage = LoadStorage(masterPassword);
        if (storage == null) return;

        List<PasswordEntry> matchingEntries;

        if (!string.IsNullOrEmpty(login))
        {
            matchingEntries = storage.Entries.FindAll(e =>
                e.Service.Equals(service, StringComparison.OrdinalIgnoreCase) &&
                e.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            matchingEntries = storage.Entries.FindAll(e =>
                e.Service.Equals(service, StringComparison.OrdinalIgnoreCase));
        }

        if (matchingEntries.Count == 0)
        {
            Console.WriteLine($"No passwords found for service: {service}");
            return;
        }

        PasswordEntry entryToUpdate;

        if (matchingEntries.Count == 1)
            entryToUpdate = matchingEntries[0];
        else
        {
            Console.WriteLine($"Multiple entries found for {service}:");
            for (int i = 0; i < matchingEntries.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Login: {matchingEntries[i].Login}");
            }

            Console.Write("Enter number to update: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= matchingEntries.Count)
                entryToUpdate = matchingEntries[index - 1];
            else
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
        }

        Console.WriteLine("Do you want to generate a new password? (y/n)");

        // TODO manage invalid input
        string generateNew = Console.ReadLine().ToLower();

        string password;
        if (generateNew == "y" || generateNew == "yes")
        {
            Console.Write("Password length (default 14): ");

            // TODO manage invalid input
            string lengthInput = Console.ReadLine();
            int length = string.IsNullOrEmpty(lengthInput) ? 14 : int.Parse(lengthInput);

            Console.Write("Include special characters? (y/n, default: n): ");

            // TODO manage invalid input
            string useSpecial = Console.ReadLine().ToLower();
            bool useSymbols = useSpecial == "y" || useSpecial == "yes";

            password = PasswordGenerator.Generate(length, PasswordGenerator.GetDefaultSymbols(), useSymbols);
            Console.WriteLine($"Generated password: {password}");
        }
        else
        {
            password = ReadMasterPassword("Enter new password: ");
        }

        entryToUpdate.Password = password;

        SaveStorage(storage, masterPassword);
        Console.WriteLine($"Password for {service} with login {entryToUpdate.Login} updated successfully.");
    }

    public static void DeletePassword(string service)
    {
        string masterPassword = ReadMasterPassword();
        var storage = LoadStorage(masterPassword);
        if (storage == null) return;

        var matchingEntries = storage.Entries.FindAll(e =>
            e.Service.Equals(service, StringComparison.OrdinalIgnoreCase));

        if (matchingEntries.Count == 0)
        {
            Console.WriteLine($"No passwords found for service: {service}");
            return;
        }

        PasswordEntry entryToDelete;

        if (matchingEntries.Count == 1)
            entryToDelete = matchingEntries[0];
        else
        {
            Console.WriteLine($"Multiple entries found for {service}:");
            for (int i = 0; i < matchingEntries.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Login: {matchingEntries[i].Login}");
            }

            Console.Write("Enter number to delete: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= matchingEntries.Count)
                entryToDelete = matchingEntries[index - 1];
            else
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
        }

        Console.WriteLine($"Are you sure you want to delete password for {service} with login {entryToDelete.Login}? (y/n)");

        // TODO manage invalid input
        string confirmation = Console.ReadLine().ToLower();

        if (confirmation == "y" || confirmation == "yes")
        {
            storage.Entries.Remove(entryToDelete);
            SaveStorage(storage, masterPassword);
            Console.WriteLine($"Password for {service} with login {entryToDelete.Login} deleted successfully.");
        }
        else
        {
            Console.WriteLine("Operation cancelled.");
        }
    }

    private static string ReadMasterPassword(string prompt = "Enter master password: ")
    {
        Console.Write(prompt);
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    private static PasswordStorage? LoadStorage(string masterPassword)
    {
        string storagePath = GetStoragePath();

        if (!File.Exists(storagePath))
        {
            Console.WriteLine("Password storage not found. Please initialize it with 'pwgen init' command.");
            return null;
        }

        try
        {
            byte[] encrypted = File.ReadAllBytes(storagePath);
            string json = Decrypt(encrypted, masterPassword);
            return JsonSerializer.Deserialize<PasswordStorage>(json);
        }
        catch (CryptographicException)
        {
            Console.WriteLine("Invalid master password or corrupted storage file.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading storage: {ex.Message}");
            return null;
        }
    }

    private static void SaveStorage(PasswordStorage storage, string masterPassword)
    {
        string storagePath = GetStoragePath();
        string json = JsonSerializer.Serialize(storage);
        byte[] encrypted = Encrypt(json, masterPassword);
        File.WriteAllBytes(storagePath, encrypted);
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }

    private static byte[] Encrypt(string plainText, string password)
    {
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] key = DeriveKey(password, salt);
        byte[] iv = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        ms.Write(salt, 0, salt.Length);
        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return ms.ToArray();
    }

    private static string Decrypt(byte[] cipherText, string password)
    {
        byte[] salt = new byte[16];
        byte[] iv = new byte[16];

        Array.Copy(cipherText, 0, salt, 0, 16);
        Array.Copy(cipherText, 16, iv, 0, 16);

        byte[] key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipherText, 32, cipherText.Length - 32);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    private static string GetStoragePath()
    {
        string appDataPath;

        if (OperatingSystem.IsWindows())
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system");
        }

        string pwgenDir = Path.Combine(appDataPath, "pwgen");
        Directory.CreateDirectory(pwgenDir);

        return Path.Combine(pwgenDir, "storage.cpwgen");
    }
}
