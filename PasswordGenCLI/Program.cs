using System.CommandLine;
using System.Threading;

using PasswordGenCLI.Common;

namespace PasswordGenCLI
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Password generation CLI tool");

            var lengthOption = new Option<int>(
                "--length",
                getDefaultValue: () => 14,
                description: "Password length. Default value is 14");
            lengthOption.AddAlias("-l");

            var symbolsOption = new Option<string>(
                "--symbols",
                getDefaultValue: () => PasswordGenerator.GetDefaultSymbols(),
                description: "Input desired special symbols to be used in password");
            symbolsOption.AddAlias("-s");

            var useSymbolsOption = new Option<bool>(
                "--use-symbols",
                getDefaultValue: () => false,
                description: "Include special characters");
            useSymbolsOption.AddAlias("-u");

            var initCommand = new Command("init", "Initialize password storage");

            var newCommand = new Command("new", "Add new password");
            var newServiceOption = new Option<string>("--service", "Service name") { IsRequired = true };
            newServiceOption.AddAlias("-s");

            var newLoginOption = new Option<string>("--login", "Username or login") { IsRequired = true };
            newLoginOption.AddAlias("-l");

            newCommand.AddOption(newServiceOption);
            newCommand.AddOption(newLoginOption);

            var readCommand = new Command("read", "Read stored password");
            var readServiceOption = new Option<string>("--service", "Service name to read password for");
            readServiceOption.AddAlias("-s");

            var listOption = new Option<bool>("--list", "List all stored services and logins");
            listOption.AddAlias("-l");

            var clipboardTimeoutOption = new Option<int>("--timeout",
                getDefaultValue: () => 30,
                description: "Clear clipboard after specified seconds (0 to disable)");
            clipboardTimeoutOption.AddAlias("-t");

            readCommand.AddOption(clipboardTimeoutOption);
            readCommand.AddOption(readServiceOption);
            readCommand.AddOption(listOption);

            var updateCommand = new Command("update", "Update stored password");
            var updateServiceOption = new Option<string>("--service", "Service name to update password for") { IsRequired = true };
            updateServiceOption.AddAlias("-s");
            var updateLoginOption = new Option<string>("--login", "Username or login") { IsRequired = false };
            updateLoginOption.AddAlias("-l");
            updateCommand.AddOption(updateServiceOption);
            updateCommand.AddOption(updateLoginOption);

            var deleteCommand = new Command("delete", "Delete stored password");
            var deleteServiceOption = new Option<string>("--service", "Service name to delete") { IsRequired = true };
            deleteServiceOption.AddAlias("-s");
            deleteCommand.AddOption(deleteServiceOption);

            rootCommand.AddOption(lengthOption);
            rootCommand.AddOption(symbolsOption);
            rootCommand.AddOption(useSymbolsOption);
            rootCommand.AddCommand(initCommand);
            rootCommand.AddCommand(newCommand);
            rootCommand.AddCommand(readCommand);
            rootCommand.AddCommand(updateCommand);
            rootCommand.AddCommand(deleteCommand);

            rootCommand.SetHandler((length, symbols, useSymbols) =>
            {
                var password = PasswordGenerator.Generate(length, symbols, useSymbols);
                Console.WriteLine(password);
                return Task.FromResult(0);
            }, lengthOption, symbolsOption, useSymbolsOption);

            initCommand.SetHandler(() =>
            {
                EncryptionService.InitializeStorage();
                return Task.FromResult(0);
            });

            newCommand.SetHandler((string service, string login) =>
            {
                EncryptionService.AddNewPassword(service, login);
                return Task.FromResult(0);
            }, newServiceOption, newLoginOption);

            readCommand.SetHandler((string service, bool list, int timeout) =>
            {
                EncryptionService.ReadPasswords(service, list, timeout);
                return Task.FromResult(0);
            }, readServiceOption, listOption, clipboardTimeoutOption);

            updateCommand.SetHandler((string service, string login) =>
            {
                EncryptionService.UpdatePassword(service, login);
                return Task.FromResult(0);
            }, updateServiceOption, updateLoginOption);

            deleteCommand.SetHandler((string service) =>
            {
                EncryptionService.DeletePassword(service);
                return Task.FromResult(0);
            }, deleteServiceOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
