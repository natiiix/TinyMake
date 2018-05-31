using System;
using System.IO;

namespace TinyMake
{
    public class Program
    {
        private const string DEFAULT_MAKEFILE = "Makefile";

        private static void Main(string[] args)
        {
            // Get the current directory path
            string currentPath = Directory.GetCurrentDirectory();
            // Get the path of the Makefile
            string makefilePath = Path.Combine(currentPath, DEFAULT_MAKEFILE);

            if (!File.Exists(makefilePath))
            {
                ExitWithError($"No Makefile in current directory ({currentPath})");
            }

            // Read the whole Makefile into a string
            string makefile = File.ReadAllText(makefilePath);

            // TODO: Process the Makefile
        }

        public static void ExitWithError(string errorMsg)
        {
            Console.Error.WriteLine("ERROR: " + errorMsg);
            Environment.Exit(-1);
        }
    }
}
