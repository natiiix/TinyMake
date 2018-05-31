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
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments supplied to the program.</param>
        private static void Main(string[] args)
        {
            // Get the current directory path
            string currentPath = Directory.GetCurrentDirectory();

            // Construct a Makefile object with current directory as the base directory
            Makefile makefile = new Makefile(currentPath);
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
