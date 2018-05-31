using System.Diagnostics;
using System.IO;

namespace TinyMake
{
    public class Rule
    {
        public string Target { get; private set; }
        public string[] Dependencies { get; private set; }
        public string[] Commands { get; private set; }

        public Rule(string target, string[] dependencies, string[] commands)
        {
            Target = target;
            Dependencies = dependencies;
            Commands = commands;
        }

        public void ExecuteCommands()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                CreateNoWindow = false,
                RedirectStandardInput = true,
                UseShellExecute = false
            };

            Process proc = Process.Start(startInfo);

            using (StreamWriter sw = proc.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    foreach (string cmd in Commands)
                    {
                        sw.WriteLine(cmd);
                    }
                }
            }

            proc.WaitForExit();
        }
    }
}
