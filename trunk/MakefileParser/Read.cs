using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace MakefileParser
{
    public class Read
    {
        public static char TAB_CHAR = '\t';
        public static int LINENO = 0;
        public static int IF_COUNTS = 0;
        public static string LINE;
        public static string RECIPEPREFIX_NAME = ".RECIPEPREFIX";
        public static char RECIPEPREFIX_DEFAULT = '\t';
        public static string specialString = "!@#$%SPECIAL%$#@!";
        public static bool export_all_variables = false;
        
        public class vmodifiers
        {
            public bool assign_v = false;
            public bool define_v = false;
            public bool undefine_v = false;
            public bool export_v = false;
            public bool override_v = false;
            public bool private_v = false;
        }

        public static string ReadLine()
        {
            string line = "";
            string buffer = "";
            while((line = Form1.makefileReader.ReadLine()) != null)
            {
                if (line.Length == 0)
                    continue;
                if (line[0] == TAB_CHAR)
                {
                    line = line.Trim();
                    line = "\t" + line;
                }
                else
                    line = line.Trim();
                if (line.Length == 0)
                    continue;
                if(line.StartsWith("#"))
                    continue;
                else if(line[line.Length - 1] == '\\')
                {
                    // line ends with continuation, so continue to the next line
                    buffer += line.Substring(0, line.Length - 1);
                    continue;
                }
                else
                {
                    buffer += line;
                    break;
                }
            }
            LINE = buffer;
            Form1.makefileTextControl.Text += buffer + "\n";
            ++LINENO;
            return buffer;
        }
        
        public static string parse_var_assignment (string line, ref vmodifiers vmod)
        {
            string p = "";
            line = Utils.next_token(line);
            if (line.Length == 0)
                return "";
            p = line;
	        while (true)
            {
		        string p2 = "";
		        Variable.variable_flavor flavor = new Variable.variable_flavor();
                p2 = Variable.parse_variable_definition (p, ref flavor);

                // If this is a variable assignment, we're done.
		        if (p2 != null)
			        break;

		        // It's not a variable; see if it's a modifier.
                p2 = Utils.end_of_token (p);

		        if (p.StartsWith("export"))
			        vmod.export_v = true;
		        else if (p.StartsWith("override"))
			        vmod.override_v = true;
		        else if (p.StartsWith("private"))
			        vmod.private_v = true;
		        else if (p.StartsWith("define"))
                {
			        // We can't have modifiers after 'define'
			        vmod.define_v = true;
			        p = Utils.next_token (p2);
			        break;
                }
		        else if (p.StartsWith ("undefine"))
                {
			        /* We can't have modifiers after 'undefine' */
			        vmod.undefine_v = true;
			        p = Utils.next_token (p2);
			        break;
                }
		        else
			        /* Not a variable or modifier: this is not a variable assignment.  */
			        return line;

		        /* It was a modifier.  Try the next word.  */
		        p = Utils.next_token (p2);
		        if (p == null || p == "")
			        return line;
            }

	        /* Found a variable assignment or undefine.  */
	        vmod.assign_v = true;
	        return p;
        }

        public static void ParseMakeFile()
        {
            Form1.makefileTextControl.Text = "";
            Form1.makefileReader = new StreamReader(Form1.makefileName);

            int ignoring = 0;
            int in_ignored_define = 0;
            List<string> filenames = new List<string>(); // nameseq
            List<string> targets = new List<string>();
            List<string> prerequisites = new List<string>();

            while (true)
            {
                string line = "";
                int wlen;
                string p = "";
                string p2 = "";
                vmodifiers vmod = new vmodifiers();

                // If line is equal to null, then we are done parsing!
                if ((line = ReadLine()) == "")
                    break;

                if (line[0] == TAB_CHAR) // This is a start of a recipe line
                {
                    foreach (string target in targets)
                        Form1.rules[target].recipe += Variable.ExpandRecipeString(line.Substring(1), Form1.rules[target]);
                    Form1.UpdateRulesTree();
                    continue;
                }
                    
                // See if this is a variable assignment.  We need to do this early, to allow variables with names like 'ifdef', 'export', 'private', etc.
                p = parse_var_assignment(line, ref vmod);
                if (vmod.assign_v)
                {
                    Variable.variable v = new Variable.variable();
                    Variable.variable_origin origin = vmod.override_v? Variable.variable_origin.o_override : Variable.variable_origin.o_file;

				    // If we're ignoring then we're done now.
                    if (ignoring == 1)
                    {
                            in_ignored_define = 1;
                        continue;
                    }
                    if (vmod.undefine_v)
                    {
                        do_undefine (p, origin);
                        continue;
                    }
				    else if (vmod.define_v)
					    v = do_define(p, origin);
				    else
					    v = Variable.try_variable_definition(p, origin, 0);


                    if(v == null)
                        MessageBox.Show("Variable at line(" + LINENO + ") should not be NULL");

				    if (vmod.export_v)
					    v.export = Variable.variable_export.v_export;
				    if (vmod.private_v)
					    v.private_var = true;
				
				    // This line has been dealt with.
                    continue;
			    }

                // If this line is completely empty, ignore it.
                if (p.Length == 0)
                    continue;

                p2 = Utils.end_of_token(p);
                wlen = p.IndexOf(p2);
                p2 = Utils.next_token(p2);

                // If we're in an ignored define, skip this line (but maybe get out).
                if (in_ignored_define == 1)
                {
                    // See if this is an endef line (plus optional comment).
                    if (p.Equals("endef") && (p2.Length == 0 || p2[0] == '#'))
                        in_ignored_define = 0;
                    continue;
                }

                if (line.StartsWith("ifeq") || line.StartsWith("ifneq") || line.StartsWith("ifdef") || line.StartsWith("ifndef"))
                    IF_COUNTS++;
                // Check for conditional state changes.
                /*{
                    int i = conditional_line(p, wlen, fstart);
                    if (i != -2)
                    {
                        if (i == -1)
                            MessageBox.Show("Invalid syntax in conditional - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");

                        ignoring = i;
                        continue;
                    }
                }*/

                // Manage the "export" keyword used outside of variable assignment as well as "unexport".
                if (p.StartsWith("export") || p.StartsWith("unexport"))
			    {
				    bool exporting = p[0] == 'u';
				    
                    // (un)export by itself causes everything to be (un)exported.
				    if (p2.Length == 0)
					    export_all_variables = exporting;
				    else
				    {
					    string cp = "";
					    string ap = "";

					    // Expand the line so we can use indirect and constructed variable names in an (un)export command.
					    cp = ap = Variable.ExpandString(p2);

                        for (p = Utils.find_next_token(ref cp); p != null; p = Utils.find_next_token(ref cp))
					    {
                            if (cp != "")
                                p = p.Substring(0, p.IndexOf(cp));
						    Variable.variable v = Variable.lookup_variable(p);
                            if (v == null)
                                v = Variable.do_variable_definition(p, "", Variable.variable_origin.o_file, 0, 0);
                            v.export = exporting ? Variable.variable_export.v_export : Variable.variable_export.v_noexport;
					    }
				    }
                    continue;
			    }

			    // Handle the special syntax for vpath.
                if (p.StartsWith("vpath"))
			    {
				    string cp = "";
				    string vpat = "";
                    cp = Variable.ExpandString(p2);
				    p = Utils.find_next_token(ref cp);
				    if (p != null)
				    {
                        if (cp != "")
                            vpat = p.Substring(0, p.IndexOf(cp));
					    p = Utils.find_next_token(ref cp);
					    // No searchpath means remove all previous selective VPATH's with the same pattern.
				    }
				    else
					    // No pattern means remove all previous selective VPATH's.
					    vpat = "";
				    //construct_vpath_list(vpat, p);
                    continue;
			    }

			    // Handle include and variants.
                if (p.StartsWith("include") || p.StartsWith("-include") || p.StartsWith("sinclude"))
			    {
				    // "-include" (vs "include") says no error if the file does not exist.  "sinclude" is an alias for this from SGI.
				    bool noerror = p[0] != 'i';

                    p = Variable.ExpandString(p2);

				    // If no filenames, it's a no-op.
				    if (p == null || p == "")
					    continue;

				    // Parse the list of file names.  Don't expand archive references!
                    List<string> files = Utils.ChopString(p);

				    // Read each included makefile.
                    foreach (string file in files)
                    {
                        Form1.Includes.Add("[INCLUDE]" + file);
                    }

                    continue;
			    }

                string expandedLine = Variable.ExpandString(line);
                int colonIndex = expandedLine.IndexOf(':');
                if (colonIndex >= 0)
                {
                    bool singleColon = false;
                    if (colonIndex == expandedLine.Length - 1)
                        singleColon = true;
                    else
                    {
                        if (expandedLine[colonIndex + 1] == ':')
                            singleColon = false;
                        else
                            singleColon = true;
                    }
                    
                    int semiColonIndex = expandedLine.IndexOf(';', colonIndex);
                    targets = Utils.ChopString(expandedLine.Substring(0, colonIndex));
                    if (semiColonIndex >= 0)                        
                        prerequisites = Utils.ChopString(expandedLine.Substring(singleColon ? colonIndex + 1 : colonIndex + 2, semiColonIndex - (singleColon ? colonIndex + 1 : colonIndex + 2)));
                    else
                        prerequisites = Utils.ChopString(expandedLine.Substring(singleColon ? colonIndex + 1 : colonIndex + 2));

                    if (singleColon) // Single-Coloned rules are combined with other -if any-
                    {
                        foreach(string target in targets)
                        {
                            Rule rule;
                            if (Form1.rules.ContainsKey(target))
                                rule = Form1.rules[target];
                            else
                            {
                                rule = new Rule();
                                rule.target = target;
                                rule.singleColon = true;
                                rule.lineno = LINENO;
                                Form1.rules.Add(rule.target, rule);
                            }
                            foreach (string prerequisite in prerequisites)
                                rule.prerequisites.Add(prerequisite);
                            if(semiColonIndex >= 0)
                                rule.recipe = expandedLine.Substring(semiColonIndex + 1);
                        }
                    }
                    else // Double-Coloned rules are added directly to SDG
                    {
                        List<string> doubleColonTargets = new List<string>();
                        foreach (string target in targets)
                        {
                            Rule rule = new Rule();
                            rule.target = Utils.GetTargerName(target);
                            doubleColonTargets.Add(rule.target);
                            rule.singleColon = false;
                            Form1.rules.Add(rule.target, rule);
                            foreach (string prerequisite in prerequisites)
                                rule.prerequisites.Add(prerequisite);
                            if(semiColonIndex >= 0)
                                rule.recipe = expandedLine.Substring(semiColonIndex + 1);
                        }
                        targets.Clear();
                        foreach (string target in doubleColonTargets)
                            targets.Add(target);
                    }
                    Form1.UpdateRulesTree();
                    continue;
                }
            }
        }

        // Execute a 'undefine' directive. The undefine line has already been read, and NAME is the name of the variable to be undefined.
        public static void do_undefine(string name, Variable.variable_origin origin)
        {
            string var = "";

	        // Expand the variable name and find the beginning (NAME) and end
            var = Variable.ExpandString(name);
	        name = Utils.next_token(var);
	        if (name.Length == 0)
                MessageBox.Show("Empty variable name - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");
            name = name.Trim();
	        Variable.undefine_variable(name, null);
        }

        /* Execute a `define' directive.
         * The first line has already been read, and NAME is the name of the variable to be defined.  The following lines remain to be read.  */
        public static Variable.variable do_define (string name, Variable.variable_origin origin)
        {
            Variable.variable_flavor flavor = new Variable.variable_flavor();
            int nlevels = 1;
            string definition = "";
            string p = "";
            string var = "";

            p = Variable.parse_variable_definition (name, ref flavor);
            if (p == null)
                // No assignment token, so assume recursive.
                flavor = Variable.variable_flavor.f_recursive;
            else
            {
                if (Utils.next_token(p).Length != 0)
                    MessageBox.Show("Extraneous text after 'define' directive - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");

                name = name.Trim();
                // Chop the string before the assignment token to get the name.
                name = name.Substring(0, name.Length - ((flavor == Variable.variable_flavor.f_recursive) ? 1 : 2));
            }

            // Expand the variable name and find the beginning (NAME) and end.
            var = Variable.ExpandString(name);
            name = Utils.next_token(var);
            
            if (name.Length == 0)
                MessageBox.Show("Empty variable name - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");

            name = name.Trim();

            // Now read the value of the variable.
            while (true)
            {
                int len;
                string line = ReadLine();

                // If there is nothing left to be eval'd, there's no 'endef'!!
                if (line == "")
                    MessageBox.Show("Missing 'endef', unterminated 'define'");

                // If the line doesn't begin with a tab, test to see if it introduces another define, or ends one.  Stop if we find an 'endef'
                if (line[0] != TAB_CHAR)
                {
                    p = Utils.next_token(line);
                    len = p.Length;

                    // If this is another 'define', increment the level count.
                    if ((len == 6 || (len > 6 && Utils.isblank(p[6]))) && (p.Substring(0, 6) == "define"))
                        ++nlevels;
                    // If this is an 'endef', decrement the count.  If it's now 0, we've found the last one.
                    else if ((len == 5 || (len > 5 && Utils.isblank(p[5]))) && (p.Substring(0, 5) == "endef"))
                    {
                        p = p.Substring(5);
                        if (Utils.next_token(p).Length != 0)
                            MessageBox.Show("Extraneous text after 'endef' directive - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");

                        if (--nlevels == 0)
                            break;
                    }
                }
                // Add this line to the variable definition and separate lines with a newline.
                definition += line + "\n";
            }

            // We've got what we need; define the variable.
            return Variable.do_variable_definition (name, definition, origin, flavor, 0);
        }

        public static void ParseSegment(string segment)
        {
        }



















    }
}
