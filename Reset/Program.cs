using Reset;
using System.Diagnostics;

internal static class Program
{
    private static bool? processFound = null;
    private static Stopwatch resetStopWatch;
    private static Stopwatch levelStopWatch;

    private static void Main()
    {
        resetStopWatch = Stopwatch.StartNew();
        levelStopWatch = Stopwatch.StartNew();
        PlayerBot bot = new(TimeSpan.FromSeconds(1.5), 400);

        bot.ProcessFound += Bot_ProcessFound;
        bot.ProcessNotFound += Bot_ProcessNotFound;
        bot.PlayerLeveledUp += Bot_PlayerLeveledUp;
        bot.PlayerResetted += Bot_PlayerResetted;

        Console.WriteLine(">Press any key to STAR and STOP the bot<");

        Console.ReadKey();
        bot.StartBot();
        resetStopWatch.Start();
        Console.WriteLine("Bot started");
        Console.ReadKey();
        bot.StopBot();
        Console.WriteLine("Bot stopped");
    }


    private static void Bot_PlayerResetted(object? sender, PlayerResettedEventArgs e)
    {
        resetStopWatch.Stop();
        Console.WriteLine($"The player {e.Name} has resetted in {resetStopWatch.Elapsed.TotalSeconds} seconds, after a total of {e.ResetAttempts} attempts! Total number of resets from this current session: {e.Resets}");
        resetStopWatch.Start();
    }

    private static void Bot_PlayerLeveledUp(object? sender, PlayerLeveledUpEventArgs e)
    {
        levelStopWatch.Stop();
        int elapsed = levelStopWatch.Elapsed.Seconds;
        Console.WriteLine($"The player {e.Name} has leveled up " +
            $"{(elapsed > 1 ? $"after {elapsed} seconds" : "")}! " +
            $"Current leve: {e.Level}");
        levelStopWatch.Start();
    }

    private static void Bot_ProcessNotFound(object? sender, EventArgs e)
    {
        if (!processFound.HasValue || processFound.Value)
        {
            Console.WriteLine($"The process was not found");
            processFound = false;
        }
    }

    private static void Bot_ProcessFound(object? sender, EventArgs e)
    {
        if (!processFound.HasValue || !processFound.Value)
        {
            Console.WriteLine($"The process was found");
            processFound = true;
        }
    }
}