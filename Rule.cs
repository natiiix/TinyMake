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
    }
}
