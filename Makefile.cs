using System.Collections.Generic;

namespace TinyMake
{
    public class Makefile
    {
        private List<Rule> rules;

        public Makefile(string makefile)
        {
            Parse(makefile);
        }

        public void Execute(string[] rulesToExec)
        {
            // TODO
        }

        private void Parse(string makefile)
        {
            // TODO

            // foreach (char c in makefile)
            // {

            // }
        }
    }
}
