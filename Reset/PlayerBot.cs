using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Reset
{
  internal class PlayerBot
  {

    public event EventHandler<PlayerLeveledUpEventArgs> PlayerLeveledUp;
    public event EventHandler TryingToReset;
    public event EventHandler<PlayerResettedEventArgs> PlayerResetted;
    public event EventHandler ProcessNotFound;
    public event EventHandler ProcessFound;

    private TimeSpan updateInterval;
    private int lastLevel;
    private int resetLevel;
    private int resets;
    private int resetAttempts;
    private CancellationTokenSource cancellationTokenSource;

    public PlayerBot(TimeSpan _updateInterval, int _resetLevel)
    {
      updateInterval = _updateInterval;
      resetLevel = _resetLevel;
      cancellationTokenSource = new();
    }

    public void StartBot()
    {
      bool firstLevel = true;
      CancellationToken cancellationToken = cancellationTokenSource.Token;
      Task.Run(() =>
          {
            while (!cancellationToken.IsCancellationRequested)
            {
              (Process? process, bool foundProcess) = tryGetGameProcess();
              if (foundProcess && process != null)
              {
                ProcessFound?.Invoke(this, EventArgs.Empty);
                (string playerName, int playerLevel) = getPjNameAndLevel(process.Id);

                if (firstLevel)
                {
                  lastLevel = playerLevel;
                  firstLevel = false;
                }

                if (lastLevel >= resetLevel && playerLevel < lastLevel)
                {
                  resets++;
                  PlayerResettedEventArgs args = new PlayerResettedEventArgs
                  {
                    Name = playerName,
                    Resets = resets,
                    ResetAttempts = resetAttempts,
                  };
                  OnPlayerResetted(args);
                  firstLevel = true;
                  resetAttempts = 0;
                }
                else if (playerLevel > lastLevel)
                {
                  PlayerLeveledUpEventArgs args = new PlayerLeveledUpEventArgs
                  {
                    Name = playerName,
                    Level = playerLevel
                  };
                  OnPlayerLeveledUp(args);
                }
                else if (playerLevel >= resetLevel)
                {
                  tryToReset(process);
                  resetAttempts++;
                }
                lastLevel = playerLevel;
              }
              else
              {
                ProcessNotFound?.Invoke(this, EventArgs.Empty);
              }
              Thread.Sleep(updateInterval.Milliseconds);
            }
          }, cancellationToken);
    }

    private void tryToReset(Process process)
    {
      var activeProcess = ProcessUtils.GetForegroundProcess();
      if (activeProcess != null && activeProcess.Id == process.Id)
      { 
        Thread.Sleep(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 1).Milliseconds);
        TryingToReset?.Invoke(this, EventArgs.Empty);
        ProcessUtils.Execute("send_message_reset.ahk");
      }
    }

    public void StopBot()
    {
      cancellationTokenSource.Cancel();
    }

    protected virtual void OnPlayerResetted(PlayerResettedEventArgs e)
    {
      EventHandler<PlayerResettedEventArgs> handler = PlayerResetted;
      if (handler != null)
      {
        handler(this, e);
      }
    }

    protected virtual void OnPlayerLeveledUp(PlayerLeveledUpEventArgs e)
    {
      EventHandler<PlayerLeveledUpEventArgs> handler = PlayerLeveledUp;
      if (handler != null)
      {
        handler(this, e);
      }
    }

    private (Process?, bool) tryGetGameProcess()
    {
      try
      {
        Process[] processes = Process.GetProcessesByName("main");
        if (processes.Length == 0)
        { return (default, false); }
        foreach (Process proc in processes)
        {
          (string pjName, int level) = getPjNameAndLevel(proc.Id);
          if (pjName == "unknown" && level == -1)
          { return (default, false); }
          else { return (proc, true); }
        }
      }
      catch (Exception)
      {
      }
      return (default, false);
    }

    private (string, int) getPjNameAndLevel(int processId)
    {
      string pjName = "unknown";
      int pjLevel = -1;
      Process? proc = Process.GetProcessById(processId);
      string? title = proc.MainWindowTitle.Trim();
      Regex regex = new Regex(@"[a-zA-Z0-9 ]+ \|\| ([a-zA-Z0-9]+) \|\| [a-zA-Z +]+: (\d+) \|\| [a-zA-Z]+: \d+ \|\| [a-zA-Z]+: \d+ \|\| [a-zA-Z]+: \d+ \|\| [a-zA-Z]+: \d+");
      Match match = regex.Match(title);
      if (match.Success)
      {
        pjName = match.Groups[1].Value;
        pjLevel = int.Parse(match.Groups[2].Value);
      }
      return (pjName, pjLevel);
    }
  }
}
