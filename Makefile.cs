using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace TinyMake
{
    public class Makefile
    {
        private const string PATH_MAKEFILE = "Makefile";
        private const string PATH_TIMESTAMPS = "Makefile.dat";

        private const string FILEPATH = @"[\w\.\-\(\)\[\]\{\}\\/]+";

        private static readonly Regex REGEX_EMPTY_LINE = new Regex(@"^\s*(?:#.*)?$");
        private static readonly Regex REGEX_RULE_HEAD = new Regex($@"^({FILEPATH})\s*:\s*((?:{FILEPATH} )*{FILEPATH})?\s*(?:#.*)?$");
        private static readonly Regex REGEX_COMMAND = new Regex(@"^(?:\t| {4})(.*)$");
        private static readonly Regex REGEX_TIMESTAMP_RECORD = new Regex($@"^({FILEPATH}):(\d+)$");

        private readonly string timestampsPath;

        private List<Rule> rules;

        private Dictionary<string, DateTime> oldTimestamps;
        private Dictionary<string, DateTime> newTimestamps;

        public Makefile(string basePath)
        {
            // Create a list for rules
            rules = new List<Rule>();

            // Get the path of the Makefile
            string makefilePath = Path.Combine(basePath, PATH_MAKEFILE);

            // Exit with an error if there is no Makefile in the currect directory
            if (!File.Exists(makefilePath))
            {
                Program.ExitWithError($"No Makefile in directory ({basePath})");
            }

            // Read all lines from the Makefile into an array
            string[] makefileLines = File.ReadAllLines(makefilePath);

            // Parse the Makefile
            Parse(makefileLines);

            // Get the path of the file containing last file modification timestamps
            // and store it for later use
            timestampsPath = Path.Combine(basePath, PATH_TIMESTAMPS);

            // Load file modification timestamps
            LoadTimestamps();
        }

        public void Execute(string[] targets)
        {
            // No target specified
            // Execute the first rule
            if (targets.Length == 0)
            {
                ExecuteIfChanged(0);
            }
            // At least one target has been specified
            else
            {
                // Indices of rules to be executed
                List<int> ruleIds = new List<int>();

                // Iterate through user-specified targets and
                // find a rule for each one of them
                foreach (string target in targets)
                {
                    // Find a rule with the specified target
                    int id = rules.FindIndex(x => x.Target == target);

                    // There is no rule with this target
                    if (id < 0)
                    {
                        Program.ExitWithError($"No rule defined for making target \"{target}\"");
                    }

                    ruleIds.Add(id);
                }

                // Execute rules for each target one by one
                foreach (int id in ruleIds)
                {
                    ExecuteIfChanged(id);
                }
            }

            // Save the new timestamps
            SaveTimestamps();
        }

        private void Parse(string[] makefileLines)
        {
            // Indicates whethere there is at least one rule head
            bool atLeastOneHead = false;

            // Temporary rule properties
            string target = string.Empty;
            string[] dependencies = new string[0];
            List<string> commands = new List<string>();

            // Iterate through lines
            for (int i = 0; i < makefileLines.Length; i++)
            {
                // Make the current line easily accessible
                string ln = makefileLines[i];

                // Skip empty lines
                if (REGEX_EMPTY_LINE.IsMatch(ln))
                {
                    continue;
                }

                // Rule head
                Match matchHead = REGEX_RULE_HEAD.Match(ln);
                if (matchHead.Success)
                {
                    // This is not the first rule head
                    if (atLeastOneHead)
                    {
                        // Add the previous rule to the list of rules
                        rules.Add(new Rule(target, dependencies, commands.ToArray()));

                        // Empty the command list
                        commands = new List<string>();
                    }

                    // Extract the rule target from the head
                    target = matchHead.Groups[1].Value;

                    // A rule with the specified target already exists
                    if (rules.Exists(x => x.Target == target))
                    {
                        Program.ExitWithError($"Redefining rule for target \"{target}\" on line {i + 1}");
                    }

                    // Extract rule dependencies from the head
                    dependencies = matchHead.Groups[2].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    // Duplicit dependencies
                    if (dependencies.Length > dependencies.Distinct().Count())
                    {
                        Program.ExitWithError($"Rule defined on line {i + 1} contains duplicit dependencies: \"{ln}\"");
                    }

                    // There has definitely been at least one rule head now
                    atLeastOneHead = true;
                    continue;
                }

                // Command
                // There must be at least one rule head before a command
                Match matchCmd = REGEX_COMMAND.Match(ln);
                if (atLeastOneHead && matchCmd.Success)
                {
                    // Add the command to the current rule
                    commands.Add(matchCmd.Groups[1].Value);
                    continue;
                }

                // Invalid syntax
                Program.ExitWithError($"Unexpected syntax on line {i + 1}: \"{ln}\"");
            }

            // No rule has been defined
            if (!atLeastOneHead)
            {
                Program.ExitWithError("Makefile is empty");
            }

            // Add the last rule to the list
            rules.Add(new Rule(target, dependencies, commands.ToArray()));
        }

        private void LoadTimestamps()
        {
            oldTimestamps = new Dictionary<string, DateTime>();
            newTimestamps = new Dictionary<string, DateTime>();

            if (File.Exists(timestampsPath))
            {
                string[] lines = File.ReadAllLines(timestampsPath);

                foreach (string ln in lines)
                {
                    Match m = REGEX_TIMESTAMP_RECORD.Match(ln);

                    if (m.Success)
                    {
                        string path = m.Groups[1].Value;
                        DateTime time = new DateTime(long.Parse(m.Groups[2].Value));

                        newTimestamps[path] = oldTimestamps[path] = time;
                    }
                }
            }
        }

        private void SaveTimestamps()
        {
            string timestampsStr = string.Empty;

            foreach (KeyValuePair<string, DateTime> pair in newTimestamps)
            {
                timestampsStr += $"{pair.Key}:{pair.Value.Ticks}" + Environment.NewLine;
            }

            File.WriteAllText(timestampsPath, timestampsStr);
        }

        private bool ExecuteIfChanged(int ruleId)
        {
            string[] deps = rules[ruleId].Dependencies;

            // See if any of the dependencies have changed
            bool changed = deps.Any(x => ExecuteIfChanged(x));

            // If one of the dependencies has changed or if there are no dependencies
            if (changed || deps.Count() == 0)
            {
                // Let the user know what rule is being executed
                Console.WriteLine($"INFO: Making target \"{rules[ruleId].Target}\"");
                // Execute the rule commands
                rules[ruleId].ExecuteCommands();
            }

            return changed;
        }

        private bool ExecuteIfChanged(string dependency)
        {
            // Try to find a rule with the dependency as its target
            int ruleId = rules.FindIndex(x => x.Target == dependency);

            // No such rule
            if (ruleId < 0)
            {
                if (File.Exists(dependency))
                {
                    // Get file info
                    FileInfo info = new FileInfo(dependency);

                    // Store the current timestamp
                    newTimestamps[dependency] = info.LastWriteTime;

                    // This file has no timestamp record
                    if (!oldTimestamps.ContainsKey(dependency))
                    {
                        return true;
                    }
                    // This file has a timestamp record
                    else
                    {
                        // Return a value based on whether the timestamp has changed
                        return !oldTimestamps[dependency].Equals(info.LastWriteTime);
                    }
                }
                else
                {
                    Program.ExitWithError($"Rule depends on non-existing file: \"{dependency}\"");
                }
            }
            // There is such a rule
            else
            {
                return ExecuteIfChanged(ruleId);
            }

            // This should never happen
            throw new Exception();
        }
    }
}
