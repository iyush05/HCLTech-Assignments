namespace NumberGuessingGame;

internal static class ConsoleApp
{
    public static void Run()
    {
        Console.WriteLine("=== Number Guessing Game ===");

        do
        {
            if (!PlayRound())
            {
                break;
            }

            Console.Write("Play again? (y/n): ");
        }
        while (string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine("Thanks for playing!");
    }

    private static bool PlayRound()
    {
        // The upper bound of Random.Next is exclusive.
        int secretNumber = Random.Shared.Next(1, 101);
        int attempts = 0;
        Console.WriteLine("I picked a number from 1 to 100. Enter q to quit.");

        while (true)
        {
            Console.Write("Your guess: ");
            string? input = Console.ReadLine();

            if (input is null || input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!int.TryParse(input, out int guess) || guess < 1 || guess > 100)
            {
                Console.WriteLine("Please enter a whole number from 1 to 100.");
                continue;
            }

            attempts++;

            if (guess < secretNumber)
            {
                Console.WriteLine("Too low!");
            }
            else if (guess > secretNumber)
            {
                Console.WriteLine("Too high!");
            }
            else
            {
                Console.WriteLine($"Correct! You guessed it in {attempts} valid attempt(s).");
                return true;
            }
        }
    }
}
