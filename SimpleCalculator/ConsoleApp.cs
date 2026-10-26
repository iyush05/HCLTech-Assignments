using System.Globalization;

namespace SimpleCalculator;

internal static class ConsoleApp
{
    public static void Run()
    {
        Console.WriteLine("=== Simple Calculator ===");
        Console.WriteLine("Use a dot for decimals, for example 12.5.");

        while (true)
        {
            Console.Write("\nChoose +, -, *, / (or q to quit): ");
            string? operation = Console.ReadLine()?.Trim();

            if (operation is null || operation.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (operation != "+" && operation != "-" && operation != "*" && operation != "/")
            {
                Console.WriteLine("Please choose one of the listed operations.");
                continue;
            }

            decimal? first = ReadNumber("First number: ");
            if (first is null) break;
            decimal? second = ReadNumber("Second number: ");
            if (second is null) break;

            if (operation == "/" && second.Value == 0)
            {
                Console.WriteLine("Cannot divide by zero.");
                continue;
            }

            try
            {
                decimal result = Calculate(first.Value, second.Value, operation);
                Console.WriteLine(FormattableString.Invariant($"Result: {first.Value} {operation} {second.Value} = {result}"));
            }
            catch (OverflowException)
            {
                Console.WriteLine("The result is outside the supported decimal range. Try smaller numbers.");
            }
        }

        Console.WriteLine("Goodbye!");
    }

    private static decimal? ReadNumber(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (input is null || input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (decimal.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal number))
            {
                return number;
            }

            Console.WriteLine("Enter a valid number using a dot for decimals, or q to quit.");
        }
    }

    private static decimal Calculate(decimal first, decimal second, string operation)
    {
        switch (operation)
        {
            case "+": return first + second;
            case "-": return first - second;
            case "*": return first * second;
            case "/": return first / second;
            default: throw new ArgumentException("Unknown operation.", nameof(operation));
        }
    }
}
