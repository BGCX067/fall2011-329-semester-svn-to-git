using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MakefileParser
{
    public class VariableEntry
    {
        public string name;
        public List<VariableInstance> instances = new List<VariableInstance>();

        public VariableEntry(string varname)
        {
            name = varname;
        }

        public class VariableInstance
        {
            public bool isReference;
            public int lineno;
            public string tmodel = "";
            
            public VariableInstance(bool isRef, int no)
            {
                isReference = isRef;
                lineno = no;
            }
        }
    }
}
