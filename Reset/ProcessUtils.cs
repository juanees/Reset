using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Reset
{
  public static class ProcessUtils
  {
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);


    public static Process? GetForegroundProcess()
    {
      uint processID = 0;
      IntPtr hWnd = GetForegroundWindow(); // Get foreground window handle
      uint threadID = GetWindowThreadProcessId(hWnd, out processID); // Get PID from window handle
      Process fgProc = Process.GetProcessById(Convert.ToInt32(processID)); // Get it as a C# obj.
      // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
      return fgProc;
    }

    public static bool Execute(string command)
    {
      try
      {
        var processInfo = new ProcessStartInfo()
        {
          FileName = "cmd",
          Arguments = "/c "+command,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
        };

        var process = new Process()
        {
          StartInfo = processInfo,
        };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
      }
      catch (System.Exception)
      {
        return false;
      }
    }
  }
}