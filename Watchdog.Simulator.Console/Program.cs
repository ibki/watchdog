using System;

namespace Watchdog.Simulator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine($"[{DateTime.Now}] Hello World!");
            System.Console.WriteLine($"[{DateTime.Now}] Environment.OSVersion.VersionString = {Environment.OSVersion.VersionString}");
            System.Console.WriteLine($"[{DateTime.Now}] Environment.Is64BitOperatingSystem = {Environment.Is64BitOperatingSystem}");
            System.Console.WriteLine($"[{DateTime.Now}] Environment.Is64BitProcess = {Environment.Is64BitProcess}");
            System.Console.WriteLine($"[{DateTime.Now}] Press any key to stop console...");
            System.Console.ReadKey();
        }
    }
}
