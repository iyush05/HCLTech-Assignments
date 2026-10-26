using System.Globalization;

namespace UnitConverter;

internal static class ConsoleApp
{
    private const double KilometersPerMile = 1.609344;
    private const double KilogramsPerPound = 0.45359237;

    public static void Run()
    {
        Console.WriteLine("=== Unit Converter ===");
        Console.WriteLine("Use a dot for decimals, for example 12.5.");

        while (true)
        {
            ShowMenu();
            string? choice = Console.ReadLine()?.Trim();
            if (choice is null || choice.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!int.TryParse(choice, out int option) || option < 1 || option > 6)
            {
                Console.WriteLine("Please choose a number from 1 to 6.");
                continue;
            }

            double? value = ReadValue(option);
            if (value is null) break;

            double result = ConvertValue(value.Value, option);
            if (!double.IsFinite(result))
            {
                Console.WriteLine("The result is too large. Try a smaller value.");
                continue;
            }

            Console.WriteLine($"Result: {result.ToString("G12", CultureInfo.InvariantCulture)} {GetTargetUnit(option)}");
        }

        Console.WriteLine("Goodbye!");
    }

    private static void ShowMenu()
    {
        Console.WriteLine("\n1. Kilometers to miles");
        Console.WriteLine("2. Miles to kilometers");
        Console.WriteLine("3. Kilograms to pounds");
        Console.WriteLine("4. Pounds to kilograms");
        Console.WriteLine("5. Celsius to Fahrenheit");
        Console.WriteLine("6. Fahrenheit to Celsius");
        Console.Write("Choose a conversion (or q to quit): ");
    }

    private static double? ReadValue(int option)
    {
        while (true)
        {
            Console.Write("Value to convert (or q to quit): ");
            string? input = Console.ReadLine();
            if (input is null || input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                || !double.IsFinite(value))
            {
                Console.WriteLine("Enter a finite number using a dot for decimals.");
                continue;
            }

            if (option <= 4 && value < 0)
            {
                Console.WriteLine("Distance and weight must be zero or greater.");
                continue;
            }

            if ((option == 5 && value < -273.15) || (option == 6 && value < -459.67))
            {
                Console.WriteLine("Temperature cannot be below absolute zero (-273.15 C / -459.67 F).");
                continue;
            }

            return value;
        }
    }

    private static double ConvertValue(double value, int option)
    {
        switch (option)
        {
            case 1: return value / KilometersPerMile;
            case 2: return value * KilometersPerMile;
            case 3: return value / KilogramsPerPound;
            case 4: return value * KilogramsPerPound;
            case 5: return value * 1.8 + 32;
            case 6: return (value - 32) / 1.8;
            default: throw new ArgumentOutOfRangeException(nameof(option));
        }
    }

    private static string GetTargetUnit(int option)
    {
        switch (option)
        {
            case 1: return "miles";
            case 2: return "kilometers";
            case 3: return "pounds";
            case 4: return "kilograms";
            case 5: return "degrees Fahrenheit";
            case 6: return "degrees Celsius";
            default: throw new ArgumentOutOfRangeException(nameof(option));
        }
    }
}
