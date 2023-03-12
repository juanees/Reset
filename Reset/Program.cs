using ResetterBot;
using System.Diagnostics;
using NLog;

internal static class Program
{
  private static bool? processFound = null;
  private static Stopwatch resetStopWatch;
  private static Stopwatch levelStopWatch;

  private static DatabaseContext? databaseContext;

  private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
  private static void Main()
  {
    resetStopWatch = Stopwatch.StartNew();
    levelStopWatch = Stopwatch.StartNew();
    var refreshRate = 1 + (Random.Shared.NextDouble() * .5);
    var resetLevel = 350;

    Logger.Info($"Bot configured with a refresh rate of: {refreshRate} and a reset level of {resetLevel}");
    PlayerBot bot = new(TimeSpan.FromSeconds(refreshRate), resetLevel);

    databaseContext = new DatabaseContext();
    databaseContext.Database.EnsureCreated();

    bot.ProcessFound += Bot_ProcessFound;
    bot.ProcessNotFound += Bot_ProcessNotFound;
    bot.PlayerLeveledUp += Bot_PlayerLeveledUp;
    bot.TryingToReset += Bot_TryingToReset;
    bot.PlayerResetted += Bot_PlayerResetted;

    Logger.Info(">Press any key to STAR the bot<");

    Console.ReadKey();

    bot.StartBot();
    resetStopWatch.Start();
    Logger.Info("Bot started");

    Logger.Info(">Type stop to STOP the bot<");
    var input = Console.ReadLine();
    while (!String.Equals(input?.ToLower() ?? "", "stop"))
    {
      input = Console.ReadLine();
    }
    bot.StopBot();
    Logger.Info("Bot stopped");
  }


  private static void Bot_PlayerResetted(object? sender, PlayerResettedEventArgs e)
  {
    resetStopWatch.Stop();
    Logger.Info($"The player {e.PlayerName} has resetted in {resetStopWatch.Elapsed:g}, after a total of {e.ResetAttempts} attempts! Total number of resets from this current session: {e.Resets}");
    databaseContext.Resets.Add(new Reset(e.PlayerName, e.Resets, e.ResetAttempts, DateTimeOffset.UtcNow));
    databaseContext.SaveChanges();
    resetStopWatch.Restart();
  }

  private static void Bot_TryingToReset(object? sender, PlayerBaseEventArgs e)
  {
    Logger.Info($"Trying to reset the character");
    databaseContext.Events.Add(new Event(EventType.Reset, e.PlayerName, "", DateTimeOffset.UtcNow));
    databaseContext.SaveChanges();
  }

  private static void Bot_PlayerLeveledUp(object? sender, PlayerLeveledUpEventArgs e)
  {
    levelStopWatch.Stop();
    int elapsed = levelStopWatch.Elapsed.Seconds;
    Logger.Info($"The player {e.PlayerName} has leveled up!" +
        $"Current leve: {e.Level}" +
        $"{(elapsed > 5 ? $". Delta {levelStopWatch.Elapsed:g}" : "")}");
    levelStopWatch.Restart();
    databaseContext.Events.Add(new Event(EventType.Level, e.PlayerName, $"Current leve: {e.Level}" +
        $" in {levelStopWatch.Elapsed:g}", DateTimeOffset.UtcNow));
    databaseContext.SaveChanges();
  }

  private static void Bot_ProcessNotFound(object? sender, EventArgs e)
  {
    if (!processFound.HasValue || processFound.Value)
    {
      Logger.Warn($"The process was not found");
      processFound = false;
      databaseContext.Events.Add(new Event(EventType.NotFound, "", "", DateTimeOffset.UtcNow));
      databaseContext.SaveChanges();
    }
  }

  private static void Bot_ProcessFound(object? sender, EventArgs e)
  {
    if (!processFound.HasValue || !processFound.Value)
    {
      Logger.Info($"The process was found");
      processFound = true;
      databaseContext.Events.Add(new Event(EventType.Found, "", "", DateTimeOffset.UtcNow));
      databaseContext.SaveChanges();
    }
  }
}