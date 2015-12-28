using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MakefileParser
{
    public class Variable
    {
        public static string variable_buffer;
        public static int variable_buffer_length;
        public static Dictionary<string, variable> current_variable_set_list = Form1.variables;
        public class variable
        {
            public string name; // Variable name.
            public string value; // Variable value.
            public string unresolved_value; // Variable value if this variable is unresolved
            public string originalvar;
            public bool unresolved = false;  // Whether this variable is a unresolved variable that has a value equals to its name and the name points to the value
            public bool isvaluegraph = false;
            public bool printed = false; // Is it printed in the graph
            public bool recursive;	// Gets recursively re-evaluated.
            public bool append;	// Nonzero if an appending target-specific variable.
            public bool conditional; // Nonzero if set with a ?=.
            public bool per_target;	// Nonzero if a target-specific variable.
            public bool special;     // Nonzero if this is a special variable.
            public bool exportable;  // Nonzero if the variable _could_ be exported.
            public bool expanding; // Nonzero if currently being expanded.
            public bool private_var; // Nonzero avoids inheritance of this target-specific variable.
            public int exp_count; // If >1, allow this many self-referential expansions.
            public variable_flavor flavor;	// Variable flavor.
            public variable_origin origin; // Variable origin.
            public variable_export export;
          }

        public enum variable_origin
        {
            o_default,		/* Variable from the default set.  */
            o_env,		/* Variable from environment.  */
            o_file,		/* Variable given in a makefile.  */
            o_env_override,	/* Variable from environment, if -e.  */
            o_command,		/* Variable given by user.  */
            o_override, 	/* Variable from an `override' directive.  */
            o_automatic,	/* Automatic variable -- cannot be set.  */
            o_invalid		/* Core dump time.  */
        }
        
        public enum variable_export
        {
	        v_export,		/* Export this variable.  */
	        v_noexport,		/* Don't export this variable.  */
	        v_ifset,		/* Export it if it has a non-default value.  */
	        v_default		/* Decide in target_environment.  */
        }

        public enum variable_flavor
        {
            f_bogus,            /* Bogus (error) */
            f_simple,           /* Simple definition (:=) */
            f_recursive,        /* Recursive definition (=) */
            f_append,           /* Appending definition (+=) */
            f_conditional       /* Conditional definition (?=) */
        }

        public class variable_set_list
        {
            public Dictionary<string, string> set;
            public variable_set_list()
            {
                set = new Dictionary<string,string>();
            }
        }

        public class variable_set
        {
            public Dictionary<string, string> set;
            public variable_set()
            {
                set = new Dictionary<string,string>();
            }
        }

        // Execute a `undefine' directive. The undefine line has already been read, and NAME is the name of the variable to be undefined.
        public static void do_undefine (string name, variable_origin origin)
        {
	        string p = "";
            string var = "";
	        
            // Expand the variable name and find the beginning (NAME) and end.
            var = ExpandString(name);
	        name = Utils.next_token(var);

            if (name == null || name.Length == 0)
                MessageBox.Show("Empty variable name at line(" + Read.LINENO + ")");

            p = name.TrimEnd(new char[]{' '});
            undefine_variable(name, null);
        }

        // Remove variable from the current variable set.
        public static void undefine_variable(string name, Dictionary<string, variable> set)
        {   
            if (set == null)
                set = current_variable_set_list;
            set.Remove(name);
        }

        /* Scan STRING for variable references and expansion-function calls.  Only LENGTH bytes of STRING are actually scanned.  If LENGTH is -1, scan until
         * a null byte is found.
         * Write the results to LINE, which must point into `variable_buffer'.  If LINE is NULL, start at the beginning of the buffer. 
         * Return a pointer to LINE, or to the beginning of the buffer if LINE is NULL. */
        public static string ExpandString(string line)
        {
            string result = "";
            variable v = new variable();
            if (line.Trim().Length == 0)
                return "";
            for (int i = 0; i < line.Length; i++)
            {
                // Copy all following uninteresting chars all at once, and skip them.  Uninteresting chars end at the next $ or the end of the input.
                int index = line.IndexOf('$', i);
                if (index >= 0)
                    result += line.Substring(i, index - i);
                else
                {
                    result += line;
                    break;
                }
                i = index + 1;
                
                // Dispatch on the char that follows the $.
                switch (line[i])
                {
                    case '$':
                        // $$ seen means output one $ to buffer.
                        result += line[i];
                        break;
                    case '(':
                    case '{':
                        // $(...) or ${...} is the general case of substitution.
                        {
                            char openparen = line[i];
                            char closeparen = (openparen == '(') ? ')' : '}';

                            string funcout = "";
                            string funcline = line.Substring(i);
                            if (Functions.handle_function(ref funcout, ref funcline))
                            {
                                result += funcout;
                                line = funcline;
                                i = -1; // because its gonna be 0, when breaking
                                break;
                            }

                            // Is there a variable reference inside the parens or braces? If so, expand it before expanding the entire reference.
                            int closeparenIndex = line.IndexOf(closeparen, i);
                            if (closeparenIndex < 0)
                                MessageBox.Show("Unterminated variable reference at line(" + Read.LINENO + ") should not be NULL");
                            string expandedString = "";
                            bool stringExpanded = false;
                            if (line.Substring(i, closeparenIndex - i).IndexOf("$") > 0)
                            {
                                // BEG now points past the opening paren or brace. Count parens or braces until it is matched.
                                int count = 0;
                                int j;
                                for (j = i + 1; j < line.Length; j++)
                                {
                                    if (line[j] == openparen)
                                        ++count;
                                    else if (line[j] == closeparen)
                                    {
                                        if (--count < 0)
                                            break;
                                    }
                                }
                                // If COUNT is >= 0, there were unmatched opening parens or braces, so we go to the simple case of a variable name such as `$($(a)'. 
                                if (count < 0)
                                {
                                    // Exapnd the name
                                    expandedString = ExpandString(line.Substring(i + 1, j - (i + 1)));
                                    line = line.Substring(0, i + 1) + expandedString + line.Substring(j);
                                    closeparenIndex = line.IndexOf(closeparen, i);
                                    stringExpanded = true;
                                }
                            }

                            /* This is not a reference to a built-in function and any variable references inside are now expanded.
                             * Is the resultant text a substitution reference? */
                            int colonIndex = line.IndexOf(':', i); 
                            if (colonIndex >= 0 && colonIndex <= closeparenIndex)
                            {
                                // This looks like a substitution reference: $(FOO:A=B).
                                int equalIndex = line.IndexOf('=', colonIndex);
                                if (equalIndex < 0)
                                {
                                    // This is an ordinary variable reference. Look up the value of the variable.
                                    if (stringExpanded)
                                        result += reference_variable(expandedString);
                                    else
                                        result += reference_variable(line.Substring(i + 1, colonIndex - i - 1));

                                    if (closeparenIndex >= line.Length - 1)
                                        line = "";
                                    else
                                        line = line.Substring(closeparenIndex + 1);
                                    i = -1; // because its gonna be 0, when breaking
                                }
                                else
                                {
                                    // Extract the variable name before the colon and look up that variable.
                                    v = lookup_variable(line.Substring(i + 1, colonIndex - i - 1));

                                    if (v == null)
                                    {
                                        Form1.UpdateVariablesTable(line.Substring(i + 1, colonIndex - i - 1), true);
                                    }
                                    else
                                    {
                                        // Here, we refer to a variable, but without a reference
                                        Form1.UpdateVariablesTable(v.name, true);
                                    }
                                    if (v == null)
                                    {
                                        MessageBox.Show("Undefined variable at line(" + Read.LINENO + ") should not be NULL");
                                        if (closeparenIndex >= line.Length - 1)
                                            line = "";
                                        else
                                            line = line.Substring(closeparenIndex + 1);
                                        i = -1; // because its gonna be 0, when breaking
                                    }
                                    // If the variable is not empty, perform the substitution.
                                    if (v != null && v.value != null && v.value != "")
                                    {
                                        string pattern = "";
                                        string replace = "";

                                        string value = (v.recursive ? recursively_expand(v) : v.value);

                                        // If we have $(foo:o=c) and foo variables resolves to UNRESOLVED variable
                                        if (value.StartsWith("UNRESOLVED"))
                                        {
                                            variable tmp = define_unresolved_variable(Utils.GetUnresolvedName(), "$(" + v.value + line.Substring(colonIndex, closeparenIndex - colonIndex + 1));
                                            result += tmp.name;
                                            if (closeparenIndex >= line.Length - 1)
                                                line = "";
                                            else
                                                line = line.Substring(closeparenIndex + 1);
                                            i = -1; // because its gonna be 0, when breaking
                                            break;
                                        }
                                        // Copy the pattern and the replacement.  Add in an extra % at the beginning to use in case there isn't one in the pattern.
                                        pattern = line.Substring(colonIndex + 1, line.IndexOf('=', colonIndex) - colonIndex - 1);
                                        replace = line.Substring(equalIndex + 1, closeparenIndex - equalIndex - 1);

                                        /*AHMED: Here we do the substituation $(foo:o=c) */
                                        string[] values = value.Split(new char[] { ' ' });
                                        for (int l = 0; l < values.Length; l++)
                                        {
                                            result += PatternReplace(values[l], pattern, replace);
                                            if (l < values.Length - 1)
                                                result += " ";
                                        }
                                        if (closeparenIndex >= line.Length - 1)
                                            line = "";
                                        else
                                            line = line.Substring(closeparenIndex + 1);
                                        i = -1; // because its gonna be 0, when breaking
                                    }
                                }
                            }
                            else
                            {
                                // This is an ordinary variable reference. Look up the value of the variable.
                                result += reference_variable(line.Substring(i + 1, closeparenIndex - i - 1));
                                if (closeparenIndex >= line.Length - 1)
                                    line = "";
                                else
                                    line = line.Substring(closeparenIndex + 1);
                                i = -1; // because its gonna be 0, when breaking
                            }
                            break;
                        }
                    default:
                        {
                            // A $ followed by a random char is a variable reference: $a is equivalent to $(a).
                            result += reference_variable(line[i] + "");
                            if (i >= line.Length - 1)
                                line = "";
                            else
                                line = line.Substring(i + 1);
                            i = -1; // because its gonna be 0, when breaking
                        }

                        break;
                }
            }
            return result;
        }

        public static string PatternReplace(string str, string pattern, string replace)
        {
            pattern = pattern.Replace(".", "[.]");
            int percentIndex = pattern.IndexOf('%');
            if (percentIndex >= 0)
            {
                if (percentIndex == pattern.Length - 1)
                    pattern = "^" + pattern.Substring(0, percentIndex) + "(.*)$";
                else
                    pattern = "^" + pattern.Substring(0, percentIndex) + "(.*)" + pattern.Substring(percentIndex + 1) + "$";
            }
            else
                pattern = "^(.*)" + pattern + "$";

            percentIndex = replace.IndexOf('%');
            if (percentIndex >= 0)
            {
                if (percentIndex == replace.Length - 1)
                    replace = replace.Substring(0, percentIndex) + "###$1###";
                else
                    replace = replace.Substring(0, percentIndex) + "###$1###" + replace.Substring(percentIndex + 1);
            }
            else
                replace = "###$1###" + replace;

            return Regex.Replace(str, pattern, replace).Replace("###", "");
        }

        /* Lookup a variable whose name is a string starting at NAME and with LENGTH chars.  NAME need not be null-terminated.
         * Returns address of the `struct variable' containing all info on the variable, or nil if no such variable is defined. */
        public static variable lookup_variable (string name)
        {
            //Form1.UpdateVariablesTable(name, true);
            if (Form1.variables.ContainsKey(name))
                return Form1.variables[name];
            return null;
        }

        public static string recursively_expand(variable v)
        {
            return recursively_expand_for_file(v);
        }

        // Recursively expand V.  The returned string is malloc'd.
        public static string recursively_expand_for_file (variable v)
        {
	        string value = "";

	        if (v.expanding)
            {
		        if (v.exp_count == 0)
			        // Expanding V causes infinite recursion.  Lose.
                    MessageBox.Show("Recursive variable (" + v.name + ") references itself (eventually) line(" + Read.LINENO + ")");
		        --v.exp_count;
            }

	        v.expanding = true;
	        if (v.append)
		        value = allocated_variable_append(v);
	        else
                value = ExpandString(v.value);
	        v.expanding = false;
            
            return value;
        }

        public static string allocated_variable_append(variable v)
        {
            string val = "";
            
            // Construct the appended variable value.
            string obuf = variable_buffer;
            int olen = variable_buffer_length;
            variable_buffer = "";
            val = variable_append (v.name, v.name.Length, current_variable_set_list);
            val = variable_buffer;
            variable_buffer = obuf;
            variable_buffer_length = olen;
            return val;
        }


        /* Like allocated_variable_expand, but for += target-specific variables. First recursively construct the variable value from its appended parts in
         * any upper variable sets.  Then expand the resulting value.  */

        public static string variable_append (string name, int length, Dictionary<string, variable> set)
        {
              variable v;
              string buf = "";

              // If there's nothing left to check, return the empty buffer.
              if (set == null)
                return "";

              // Try to find the variable in this variable set.
              v = lookup_variable_in_set (name, length, set);

              // If there isn't one, look to see if there's one in a set above us.
              if (v == null)
                  return "";

              // If this variable type is append, first get any upper values. If not, initialize the buffer.
              if (v.append)
                  buf = variable_append(name, length, set);
              else
                  buf = "";

              // Append this value to the buffer, and return it. If we already have a value, first add a space.
              if (buf.Length > 0)
                  buf += " ";

              // Either expand it or copy it, depending.
              if (!v.recursive)
                  return (buf + v.value);

              //buf = ExpandString(buf, v.value, v.value.Length);//TODO: CHECK this for appending
              return (buf.Substring(buf.Length));
        }

        /* Lookup a variable whose name is a string starting at NAME and with LENGTH chars in set SET.  NAME need not be null-terminated.
         * Returns address of the `struct variable' containing all info on the variable, or nil if no such variable is defined.  */
        public static variable lookup_variable_in_set (string name, int length, Dictionary<string, variable> set)
        {
            if(set.ContainsKey(name))
                return set[name];
            return null;
        }

        // Expand a simple reference to variable NAME, which is LENGTH chars long.
        public static string reference_variable(string name)
        {
            Form1.UpdateVariablesTable(name, true);
            variable v = lookup_variable(name);
            if (v == null)
            {
                v = do_variable_definition(name, "", variable_origin.o_file, variable_flavor.f_simple, 0);
                //MessageBox.Show("Undefined variable (" + name + ") line [" + Read.LINENO + "]!");
            }

            // If there's no variable by that name or it has no value, stop now.
            if (v == null || (v.value == "" && !v.append))
                return "";

            return (v.recursive? recursively_expand (v) : v.value);
        }

        public static string ExpandRecipeString(string recipe, Rule rule)
        {
            if (recipe.Contains("$@"))
                recipe = recipe.Replace("$@", rule.target);
            if (recipe.Contains("$^"))
            {
                string preqs = "";
                foreach (string preq in rule.prerequisites)
                    preqs += preq + " ";
                recipe = recipe.Replace("$^", preqs.Trim());
            }
            if (recipe.Contains("$<"))
                recipe = recipe.Replace("$<", rule.prerequisites.Count >= 1 ? rule.prerequisites[0] : "");
            return ExpandString(recipe);

        }

        public static variable define_unresolved_variable (string name, string value)
        {
	        variable v = define_variable_in_set(name, name, variable_origin.o_file, false, current_variable_set_list);
	        v.unresolved = true;
	        v.printed = false;
	        v.unresolved_value = value;
	        return v;
        }

        /* Define variable named NAME with value VALUE in SET.  VALUE is copied. LENGTH is the length of NAME, which does not need to be null-terminated.
         * ORIGIN specifies the origin of the variable (makefile, command line or environment). 
         * If RECURSIVE is nonzero a flag is set in the variable saying that it should be recursively re-expanded.  */
        public static variable define_variable_in_set (string name, string value, variable_origin origin, bool recursive, Dictionary<string, variable> set)
        {
            Form1.UpdateVariablesTable(name, false);
            variable v;
            if (set == null)
                set = current_variable_set_list;

            if(set.ContainsKey(name))
                v = set[name];
            else
            {
                v = new variable();
                v.name = name;
                set.Add(v.name, v);
                v.value = value;
                v.special = false;
                v.expanding = false;
                v.exp_count = 0;
                v.per_target = false;
                v.append = false;
                v.private_var = false;
                v.export = variable_export.v_default;
                v.exportable = true;
                if (name[0] != '_' && (name[0] < 'A' || name[0] > 'Z') && (name[0] < 'a' || name[0] > 'z'))
                    v.exportable = false;
                else
                {
                    int index = 1;
                    int temp;
                    for (; index < name.Length; index++)
                    {
                        if (name[index] != '_' && (name[index] < 'a' || name[index] > 'z') && (name[index] < 'A' || name[index] > 'Z') && !int.TryParse(name[index] + "", out temp))
                            break;
                    }
                    if (index != name.Length)
                        v.exportable = false;
                }
            }
            v.value = value;
            v.origin = origin;
            v.recursive = recursive;
            v.unresolved = false;
            v.isvaluegraph = false;
            v.printed = false;
            return v;
        }

        /* Parse P (a null-terminated string) as a variable definition. If it is not a variable definition, return NULL.
         * If it is a variable definition, return a pointer to the char after the assignment token and set *FLAVOR to the type of variable assignment.  */
        public static string parse_variable_definition(string p, ref variable_flavor flavor)
        {
            bool wspace = false;
            int i;
            for (i = 0; i < p.Length; i++)
            {
                if (p[i] == '$')
                {
                    char closeparen;
                    char openparen = p[i + 1];
                    if (openparen == '(')
                        closeparen = ')';
                    else if (openparen == '{')
                        closeparen = '}';
                    else
                        // '$$' or '$X'.  Either way, nothing special to do here.
                        continue;

                    int count = 0;
                    for (; i < p.Length; i++)
                    {
                        if (p[i] == openparen)
                            ++count;
                        else if (p[i] == closeparen)
                        {
                            if (--count < 0)
                            {
                                i++;
                                break;
                            }
                        }
                    }
                    continue;
                }

                if (Utils.isblank(p[i]))
                {
                    wspace = true;
                    continue;
                }

                if (p[i] == '=')
                {
                    flavor = variable_flavor.f_recursive;
                    return p.Substring(i + 1);
                }

                // Match assignment variants (:=, +=, ?=)
                if ((i + 1) < p.Length && p[i + 1] == '=')
                {
                    switch (p[i])
                    {
                        case ':':
                            flavor = variable_flavor.f_simple;
                            break;
                        case '+':
                            flavor = variable_flavor.f_append;
                            break;
                        case '?':
                            flavor = variable_flavor.f_conditional;
                            break;
                        default:
                            // If we skipped whitespace, non-assignments means no var.
                            if (wspace)
                                return null;

                            // Might be assignment, or might be $= or #=.  Check.
                            continue;
                    }
                    return p.Substring(i + 2);
                }
                else if (p[i] == ':')
                    return null;
                // If we skipped whitespace, non-assignments means no var.
                if (wspace)
                    return null;
            }
            if (i >= p.Length)
                return null;
            return p.Substring(i);
        }

        // Given a variable, a value, and a flavor, define the variable. See the try_variable_definition() function for details on the parameters.
        public static variable do_variable_definition (string varname, string value, variable_origin origin, variable_flavor flavor, int target_var)
        {
            string p = "";
            string alloc_value = "";
            variable v;
            bool append = false;
            bool conditional = false;

            // Calculate the variable's new value in VALUE.
            switch (flavor)
            {
            default:
            case variable_flavor.f_bogus:
                //Should not be possible.
                MessageBox.Show("Abort!");
                break;
            case variable_flavor.f_simple:
                /* A simple variable definition "var := value".  Expand the value. We have to allocate memory since otherwise it'll clobber the
                 * variable buffer, and we may still need that if we're looking at a target-specific variable.  */
                p = alloc_value = ExpandString(value);
                break;
            case variable_flavor.f_conditional:
                // A conditional variable definition "var ?= value". The value is set IFF the variable is not defined yet.
                v = lookup_variable(varname);
                if (v != null)
                    return v.special? set_special_var(v) : v;
                conditional = true;
                flavor = variable_flavor.f_recursive;
                p = value;
                break;
            case variable_flavor.f_recursive:
                // A recursive variable definition "var = value". The value is used verbatim.
                p = value;
                break;
            case variable_flavor.f_append:
            {
                // If we have += but we're in a target variable context, we want to append only with other variables in the context of this target.
                if (target_var == 1)
                {
                    append = true;
                    v = lookup_variable_in_set(varname, varname.Length, current_variable_set_list);
                    
                    // Don't append from the global set if a previous non-appending target-specific variable definition exists.
                    if (v != null && !v.append)
                        append = false;
                }
                else
                    v = lookup_variable(varname);

                if (v == null)
                {
                    // There was no old value. This becomes a normal recursive definition.
                    p = value;
                    flavor = variable_flavor.f_recursive;
                }
                else
                {
                    // Paste the old and new values together in VALUE.
                    string val = "";
                    string tp = "";
                    val = value;
                    if (v.recursive)
                        // The previous definition of the variable was recursive. The new value is the unexpanded old and new values.
                        flavor = variable_flavor.f_recursive;
                    else
                        /* The previous definition of the variable was simple. The new value comes from the old value, which was expanded
                         * when it was set; and from the expanded new value.  Allocate memory for the expansion as we may still need the rest of the
                         * buffer if we're looking at a target-specific variable.  */
                        val = tp = ExpandString(val);

                    p = alloc_value = v.value + " " + val;
                }
                break;
            }
            }
            /* If we are defining variables inside an $(eval ...), we might have a 
             * different variable context pushed, not the global context (maybe we're inside a $(call ...) or something.  Since this function is only ever
             * invoked in places where we want to define globally visible variables, make sure we define this variable in the global set.  */
            
            v = define_variable_in_set (varname, p, origin, (flavor == variable_flavor.f_recursive), (target_var == 1 ? current_variable_set_list : null));
            v.append = append;
            v.conditional = conditional;
            return (v.special ? set_special_var(v) : v);
        }

        public static variable set_special_var(variable var)
        {
            if (var.name.Equals(Read.RECIPEPREFIX_NAME))
            {
                // The user is resetting the command introduction prefix.  This has to happen immediately, so that subsequent rules are interpreted properly.
                Read.TAB_CHAR = (var.value.Length == 0 ? Read.RECIPEPREFIX_DEFAULT : var.value[0]);
            }
            return var;
        }

        /* Try to interpret LINE (a null-terminated string) as a variable definition.
         * ORIGIN may be o_file, o_override, o_env, o_env_override, or o_command specifying that the variable definition comes 
         * from a makefile, an override directive, the environment with or without the -e switch, or the command line.
         * 
         * See the comments for assign_variable_definition().
         * If LINE was recognized as a variable definition, a pointer to its `struct variable' is returned.
         * If LINE is not a variable definition, NULL is returned.  */
        public static variable try_variable_definition (string line, variable_origin origin, int target_var)
        {
            variable v = new variable();

	        if (assign_variable_definition (ref v, line) == null)
		        return null;

	        return do_variable_definition (v.name, v.value, origin, v.flavor, target_var);
        }

        /* Try to interpret LINE (a null-terminated string) as a variable definition.
           If LINE was recognized as a variable definition, a pointer to its `struct
           variable' is returned.  If LINE is not a variable definition, NULL is returned.  */

        public static variable assign_variable_definition (ref variable v, string line)
        {
            string beg = "";
            variable_flavor flavor = new variable_flavor();
            string name = "";

            beg = Utils.next_token(line);
            line = parse_variable_definition (beg, ref flavor);
            if (line == null)
                return null;

            int endindex = 0;
            if (flavor == variable_flavor.f_append)
                endindex = beg.IndexOf("+=");
            else
            {
                if (flavor == variable_flavor.f_recursive)
                    endindex = beg.IndexOf("=");
                else
                {
                    endindex = beg.IndexOf(":=");
                    if (endindex < 0)
                        endindex = beg.IndexOf("?=");
                }
            }
            name = beg.Substring(0, endindex).Trim();


            line = line.Trim();
            v.value = line;
            v.flavor = flavor;

            // Expand the name, so "$(foo)bar = baz" works.
            v.name = ExpandString(name);

            if (v.name.Length == 0)
                MessageBox.Show("Empty variable name - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");
            return v;
        }
    }
}
