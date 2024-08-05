using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Controller
{
    const uint CTRL_C_EVENT = 0;
    const uint CTRL_BREAK_EVENT = 1;
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AttachConsole(uint dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FreeConsole();
    [DllImport("kernel32.dll")]
    static extern uint GetLastError();

    const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;
    static void Main(string[] args)
    {
        int id = Process.GetCurrentProcess().Id;

        Process process = new Process();
        process.StartInfo.FileName = "C:\\Users\\karak\\OneDrive\\Belgeler\\GitHub\\C\\SigReceiver101\\x64\\Debug\\SigReceiver101.exe";
        process.StartInfo.WorkingDirectory = "C:\\Users\\karak\\OneDrive\\Belgeler\\GitHub\\C\\SigReceiver101\\x64\\Debug";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.OutputDataReceived += (sender, e) => LogOutput(e.Data);
        process.ErrorDataReceived += (sender, e) => LogOutput(e.Data);

        process.Start();
        //senderProcess.BeginOutputReadLine();
        //senderProcess.BeginErrorReadLine();

        string processName = "SigReceiver101";

        Process[] processes = Process.GetProcessesByName(processName);
        if (processes.Length > 0)
        {
            int firstProcessId = processes[0].Id;
            Console.WriteLine($"First process with name '{processName}' has PID: {firstProcessId}");
            Console.WriteLine("Press Ctrl+Break to send the signal...");

            //sender: eventi tetikleyen nesne.
            //e: ConsoleCancelEventArgs -> olay hakkında bilgi. - e.SpecialKey olayı başlayan tuş 
            // e.Cancel = true işlem iptal et sonlanmasın
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                FreeConsole();
                if (AttachConsole((uint)firstProcessId))
                {
                    Thread.Sleep(100);
                    bool result = GenerateConsoleCtrlEvent(CTRL_BREAK_EVENT, (uint)firstProcessId);
                    if (result) 
                    {
                        Console.WriteLine("CTRL+BREAK signal sent successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send CTRL+BREAK signal. Error code: {Marshal.GetLastWin32Error()}");

                    }
                }
                else
                {
                    Console.WriteLine($"Failed to attach to console of process {firstProcessId}. Error code: {Marshal.GetLastWin32Error()}");
                }
            };

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
        else
        {
            Console.WriteLine("Process not found.");
        }
    }

    private static void LogOutput(string? data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            LogEvent(data);
        }
    }

    private static void LogEvent(string message)
    {
        string logFilePath = "log.txt";
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
