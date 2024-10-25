using System.Text;

namespace PasswordGenCLI
{
    internal class PasswordGenerator
    {
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string DefaultSymbols = @"+-/\|=_()[]{}!?@$#^%:*";
        private const int MinLength = 6;
        private const int MaxLength = 30;
        private const int MaxLengthForOneDelimiter = 14;
        private const int MaxLengthForTwoDelimiters = 20;

        private static readonly Random Random = new();

        public static string Generate(int length, string? symbols, bool useSpecialSymbols)
        {
            length = Math.Clamp(length, MinLength, MaxLength);
            var sb = new StringBuilder();

            if (useSpecialSymbols)
            {
                GenerateWithSpecialSymbols(sb, length, symbols ?? DefaultSymbols);
            }
            else
            {
                GenerateWithDelimiters(sb, length);
            }
            if (sb.Length > length)
            {
                while (sb.Length > length)
                {
                    sb.Remove(length - 1, 1);
                }
            }
            return sb.ToString();
        }

        public static string GetDefaultSymbols() => DefaultSymbols;

        private static void GenerateWithSpecialSymbols(StringBuilder sb, int length, string symbols)
        {
            var availableChars = (Letters + symbols).ToCharArray();
            for (int i = 0; i < length; i++)
            {
                sb.Append(availableChars[Random.Next(availableChars.Length)]);
            }
        }

        private static void GenerateWithDelimiters(StringBuilder sb, int length)
        {
            var delimiters = GetDelimiterPositions(length);

            for (int i = 0; i <= length; i++)
            {
                if (delimiters.Contains(i))
                {
                    sb.Append('-');
                    continue;
                }
                sb.Append(Letters[Random.Next(Letters.Length)]);
            }
        }

        private static int[] GetDelimiterPositions(int length)
        {
            if (length < MaxLengthForOneDelimiter)
            {
                return [length / 2];
            }

            if (length < MaxLengthForTwoDelimiters)
            {
                var step = length / 3;
                return [step, step * 2];
            }

            var longStep = length / 4;
            return [longStep, longStep * 2, longStep * 3];
        }
    }
}
