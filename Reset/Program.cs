using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

internal static class Program
{
    public static int RESET_RETRY_TRESHOLD = 10;

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hWnd);

    [STAThread]
    private static void Main()
    {
        int resetTries = 0;
        while (true)
        {
            Process[] processes = Process.GetProcessesByName("main");
            string pjName = "unknown";

            foreach (Process proc in processes)
            {
                string? title = proc.MainWindowTitle.Trim();
                int level = getLevelFromWindowsTitle(title);
                pjName = getPjNameFromWindowsTitle(title);
                Console.WriteLine(pjName + " level -> " + level);
                if (level >= 350)
                {
                    resetTries++;

                    Console.WriteLine("trying to reset " + pjName + " for the " + resetTries + " time");

                    SetForegroundWindow(proc.MainWindowHandle);

                    SendKeys.SendWait("{ENTER}");
                    SendKeys.SendWait("/reset");
                    Thread.Sleep(1 * 1000);
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(1 * 500);
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(1 * 500);
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(1 * 500);

                }
                else
                {
                    Thread.Sleep(1 * 1500);
                    resetTries = 0;
                }
            }

            if (resetTries >= RESET_RETRY_TRESHOLD)
            {
                Console.WriteLine("Error reseting " + pjName);
            }

        }
    }

    private static int getLevelFromWindowsTitle(string title)
    {

        Regex regex = new Regex(@"[a-zA-Z0-9 ] \|\| [a-zA-Z0-9]+ \|\| Level \+ Master Level: (\d+) \|\| Str: \d+ \|\| Agi: \d+ \|\| Vit: \d+ \|\| Ene: \d+");
        Match match = regex.Match(title);
        if (match.Success)
        {
            string value1 = match.Groups[1].Value;
            return int.Parse(value1);
        }
        return 0;
    }

    private static string getPjNameFromWindowsTitle(string title)
    {
        Regex regex = new Regex(@"[a-zA-Z0-9 ] \|\| ([a-zA-Z0-9]+) \|\| Level \+ Master Level: \d+ \|\| Str: \d+ \|\| Agi: \d+ \|\| Vit: \d+ \|\| Ene: \d+");
        Match match = regex.Match(title);
        if (match.Success)
        {
            return match.Groups[1].Value;

        }
        return "unknon";
    }
}