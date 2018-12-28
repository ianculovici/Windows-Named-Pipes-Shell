using System;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;

namespace NamedPipes
{
    class NamedPipeShell
    {
        static string pipeName = "ProcessPipe";
        static void Main(string[] args)
        {
            StartServer();
            Task.Delay(1000).Wait();
            RunClient();

        }

        static void StartServer()
        {
            Task.Factory.StartNew(() =>
            {
                var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 4);
                server.WaitForConnection();
                try
                {
                    StreamWriter writer = new StreamWriter(server);
                    writer.AutoFlush = true;
                    string rowText = "40,Test Detail Value,2018-11-21 9:01:19.012345,14.97";
                    writer.WriteLine(rowText);
                    writer.Close();
                }
                catch (IOException e)
                {
                    Console.WriteLine("[ECHO DAEMON]ERROR: {0}", e.Message);
                }
                server.Close();
            });
        }

        private static void RunClient()
        {
            var lineCommand = "more \\\\.\\pipe\\" + pipeName;
            Console.WriteLine("Command: " + lineCommand);
            var processInfo = new ProcessStartInfo("CMD.exe", "/c " + lineCommand);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("output>>  " + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>  " + e.Data);
            process.BeginErrorReadLine();
            process.WaitForExit();
            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();

            Console.WriteLine("");
            Console.WriteLine("Processing Completed. Click <Enter> to finish.");
            Console.Read();
        }

    }
}
