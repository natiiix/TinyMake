using System;
using System.IO;

namespace TinyMake
{
    /// <summary>
    /// Main class of the program.
    /// Contains core methods.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Name of the Makefile file.
        /// </summary>
        private const string MAKEFILE_NAME = "Makefile";

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments supplied to the program.</param>
        private static void Main(string[] args)
        {
            // Get the current directory path
            string currentPath = Directory.GetCurrentDirectory();
            // Get the path of the Makefile
            string makefilePath = Path.Combine(currentPath, MAKEFILE_NAME);

            // Exit with an error if there is no Makefile in the currect directory
            if (!File.Exists(makefilePath))
            {
                ExitWithError($"No Makefile in current directory ({currentPath})");
            }

            // Read the whole Makefile into a string
            string makefileStr = File.ReadAllText(makefilePath);

            // Construct a Makefile object from the string
            Makefile makefile = new Makefile(makefileStr);
            // Execute the rules specified by the user
            makefile.Execute(args);
        }

        /// <summary>
        /// Prints an error message and exits the program.
        /// </summary>
        /// <param name="errorMsg">Error message to be printed.</param>
        /// <param name="exitCode">Exit code to be used. (-1 by default)</param>
        public static void ExitWithError(string errorMsg, int exitCode = -1)
        {
            Console.Error.WriteLine("ERROR: " + errorMsg);
            Environment.Exit(exitCode);
        }
    }
}
