using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ScannerBot.Services
{
    class ShellCommandRunner
    {
        public string RunCommand(string cmd)
        {
            Process process;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var escapedArgs = cmd.Replace("\"", "\\\"");

                Console.WriteLine(escapedArgs);

                process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
            }
            else
            {
                var escapedArgs = cmd.Replace("\"", "\\\"");

                Console.WriteLine(escapedArgs);

                process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
            }
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(result);
            return result;
        }
    }
}
