using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Reset
{
    internal class PlayerBot
    {

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        public event EventHandler<PlayerLeveledUpEventArgs> PlayerLeveledUp;
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
                        (Process? process, bool foundProcess) = TryGetGameProcess();
                        if (foundProcess && process != null)
                        {
                            ProcessFound?.Invoke(this, EventArgs.Empty);
                            (string playerName, int playerLevel) = getPsjNameAndLevel(process.Id);

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
                                Thread.Sleep(TimeSpan.FromSeconds(.5).Milliseconds);
                                SetForegroundWindow(process.MainWindowHandle);
                                Thread.Sleep(TimeSpan.FromSeconds(.5).Milliseconds);
                                SendKeys.SendWait("{ENTER}");
                                Thread.Sleep(TimeSpan.FromSeconds(.5).Milliseconds);
                                SendKeys.SendWait("/reset");
                                Thread.Sleep(TimeSpan.FromSeconds(.5).Milliseconds);
                                SendKeys.SendWait("{ENTER}");
                                Thread.Sleep(TimeSpan.FromSeconds(.5).Milliseconds);
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

        private (Process?, bool) TryGetGameProcess()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("main");
                if (processes.Length == 0)
                {
                    return (default, false);
                }
                foreach (Process proc in processes)
                {
                    (string pjName, int level) = getPsjNameAndLevel(proc.Id);
                    if (pjName != "unknown" && level != -1)
                    {
                        return (proc, true);
                    }
                }
                return (default, false);
            }
            catch (Exception)
            {
                return (default, false);
            }
        }

        private (string, int) getPsjNameAndLevel(int processId)
        {
            Process? proc = Process.GetProcessById(processId);
            string? title = proc.MainWindowTitle.Trim();
            int level = getLevelFromWindowsTitle(title);
            string pjName = getPjNameFromWindowsTitle(title);
            return (pjName, level);

        }

        private int getLevelFromWindowsTitle(string title)
        {

            Regex regex = new Regex(@"[a-zA-Z0-9 ] \|\| [a-zA-Z0-9]+ \|\| Level \+ Master Level: (\d+) \|\| Str: \d+ \|\| Agi: \d+ \|\| Vit: \d+ \|\| Ene: \d+");
            Match match = regex.Match(title);
            if (match.Success)
            {
                string value1 = match.Groups[1].Value;
                return int.Parse(value1);
            }
            return -1;
        }

        private string getPjNameFromWindowsTitle(string title)
        {
            Regex regex = new Regex(@"[a-zA-Z0-9 ] \|\| ([a-zA-Z0-9]+) \|\| Level \+ Master Level: \d+ \|\| Str: \d+ \|\| Agi: \d+ \|\| Vit: \d+ \|\| Ene: \d+");
            Match match = regex.Match(title);
            if (match.Success)
            {
                return match.Groups[1].Value;

            }
            return "unknown";
        }
    }
}
