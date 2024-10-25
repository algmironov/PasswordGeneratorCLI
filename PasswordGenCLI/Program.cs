using System.CommandLine;

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

            rootCommand.AddOption(lengthOption);
            rootCommand.AddOption(symbolsOption);
            rootCommand.AddOption(useSymbolsOption);

            rootCommand.SetHandler((length, symbols, useSymbols) =>
            {
                var password = PasswordGenerator.Generate(length, symbols, useSymbols);
                Console.WriteLine(password);
                return Task.FromResult(0);
            }, lengthOption, symbolsOption, useSymbolsOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
