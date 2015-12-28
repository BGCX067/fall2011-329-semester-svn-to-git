using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MakefileParser
{
    public class Rule
    {
        public string target;
        public List<string> prerequisites = new List<string>();
        public string recipe;
        public bool singleColon;
        public int lineno;
    }
}
