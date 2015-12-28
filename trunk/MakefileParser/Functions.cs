using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MakefileParser
{
    public class Functions
    {

        public class function
        {
            public string keyword;
            public string functionName;
            public int minArgs;
            public int maxArgs;
            public bool expand_args;

            public function(string k, int min, int max, bool e, string f)
            {
                keyword = k;
                functionName = f;
                minArgs = min;
                maxArgs = max;
                expand_args = e;
            }
        }

        // Look up a function by name.
        public static function lookup_function (string s)
        {
            foreach(function func in Form1.functions)
            {
                if(s.StartsWith(func.keyword))
                {
                    if(s.Length == func.keyword.Length || Utils.isblank(s[func.keyword.Length]))
                    {
                        return func;
                    }
                }
            }
            return null;
        }

        public static bool handle_function (ref string funcout, ref string line)
        {
	        char openparen = line[0];
	        char closeparen = openparen == '(' ? ')' : '}';
            string text = line.Substring(1);
            
            function func = lookup_function(text);
            if (func == null)
                return false;

            // We found a builtin function.  Find the beginning of its arguments (skip whitespace after the name).
            string args = Utils.next_token(text.Substring(func.keyword.Length));

            /* Find the end of the function invocation, counting nested use of  whichever kind of parens we use.  Since we're looking, count commas
             * to get a rough estimate of how many arguments we might have.  The count might be high, but it'll never be low.  */
            int nargs = 1;
            int count = 0;
            int i;
            for (i = 0; i < args.Length; i++)
            {
                if (args[i] == ',')
                    ++nargs;
                else if (args[i] == openparen)
                    ++count;
                else if (args[i] == closeparen)
                {
                    if(--count < 0)
                        break;
                }
            }

            if (count >= 0)
                MessageBox.Show("Unterminated call to function '" + func.keyword + "': missing '" + closeparen + "' - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");


            /* Chop the string into arguments, then a null.  As soon as we hit MAXIMUM_ARGS (if it's >0) assume the rest of the string is part of the
             * last argument. If we're expanding, store pointers to the expansion of each one.  If 
             * not, make a duplicate of the string and point into that, nul-terminating each argument.  */
            List<string> arguments = new List<string>();
            int nextArgIndex = -1;
            nargs = 0;
            for (int j = 0; j <= i;)
            {
                nargs++;
                if (nargs == func.maxArgs || ((nextArgIndex = find_next_argument(openparen, closeparen, args.Substring(j), i)) == -1))
                    nextArgIndex = i - j;
                if (func.expand_args)
                    arguments.Add(Variable.ExpandString(args.Substring(j, nextArgIndex)));
                else
                    arguments.Add(args.Substring(j, nextArgIndex));
                j += nextArgIndex + 1;
            }

            // Finally!  Run the function...
            funcout = expand_builtin_function(funcout, arguments, func);
            if (i == args.Length - 1)
                line = "";
            else
                line = args.Substring(i + 1);
	        return true;
        }

        // These must come after the definition of function_table.
        public static string expand_builtin_function (string funcout, List<string> arguments, function func)
        {
            if (arguments.Count < func.minArgs)
                MessageBox.Show("Insufficient number of arguments (" + arguments.Count + ") to function '" + func.keyword + "' - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");

	        /* I suppose technically some function could do something with no arguments, but so far none do, so just test it for all functions here 
             * rather than in each one.  We can change it later if necessary.  */
            if (arguments.Count == 0)
                return funcout;

            if (func.functionName == "")
                MessageBox.Show("Unimplemented on this platform: function '" + func.keyword + "' - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");

            Functions inst = new Functions();
            MethodInfo method = inst.GetType().GetMethod(func.functionName);
            if (method == null)
                MessageBox.Show("Unimplemented on this platform: function '" + func.keyword + "' - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");
            return method.Invoke(inst, new object[] { arguments }).ToString();
        }

        /* Find the next comma or ENDPAREN (counting nested STARTPAREN and ENDPARENtheses), starting at PTR before END.  Return a pointer to next character.
         * If no next argument is found, return NULL. */
        public static int find_next_argument (char startparen, char endparen, string ptr, int endIndex)
        {
	        int count = 0;
            for (int i = 0; i < endIndex; i++)
            {
                if (ptr[i] == startparen)
                    ++count;
                else if (ptr[i] == endparen)
                {
                    if (--count < 0)
                        return -1;
                }
                else if (ptr[i] == ',' && count == 0)
                    return i;
            }
	        // We didn't find anything.
	        return -1;
        }

        public static void FillFunctionsLookupTable()
        {
            Form1.functions.Add(new function(("abspath"),       0,  1,  true,  "func_abspath"));//Done
            Form1.functions.Add(new function(("addprefix"),     2,  2,  true,  "func_addprefix"));//Done
            Form1.functions.Add(new function(("addsuffix"),     2,  2,  true,  "func_addsuffix"));//Done
            Form1.functions.Add(new function(("basename"),      0,  1,  true,  "func_basename"));//Done
            Form1.functions.Add(new function(("dir"),           0,  1,  true,  "func_dir"));//Done
            Form1.functions.Add(new function(("notdir"),        0,  1,  true,  "func_notdir"));//Done
            Form1.functions.Add(new function(("subst"),         3,  3,  true,  "func_subst"));//Done
            Form1.functions.Add(new function(("suffix"),        0,  1,  true,  "func_suffix"));//Done
            Form1.functions.Add(new function(("filter"),        2,  2,  true,  "func_filter"));//Done
            Form1.functions.Add(new function(("filter-out"),    2,  2,  true,  "func_filterout"));//Done
            Form1.functions.Add(new function(("findstring"),    2,  2,  true,  "func_findstring"));//Done
            Form1.functions.Add(new function(("firstword"),     0,  1,  true,  "func_firstword"));//Done
            Form1.functions.Add(new function(("flavor"),        0,  1,  true,  "func_flavor"));//Done
            Form1.functions.Add(new function(("join"),          2,  2,  true,  "func_join"));//Done
            Form1.functions.Add(new function(("lastword"),      0,  1,  true,  "func_lastword"));//Done
            Form1.functions.Add(new function(("patsubst"),      3,  3,  true,  "func_patsubst"));//Done
            Form1.functions.Add(new function(("realpath"),      0,  1,  true,  "func_realpath"));//Done
            Form1.functions.Add(new function(("shell"),         0,  1,  true,  "func_shell"));//Done
            Form1.functions.Add(new function(("sort"),          0,  1,  true,  "func_sort"));//Done
            Form1.functions.Add(new function(("strip"),         0,  1,  true,  "func_strip"));//Done
            Form1.functions.Add(new function(("wildcard"),      0,  1,  true,  "func_wildcard"));//Done
            Form1.functions.Add(new function(("word"),          2,  2,  true,  "func_word"));//Done
            Form1.functions.Add(new function(("wordlist"),      3,  3,  true,  "func_wordlist"));//Done
            Form1.functions.Add(new function(("words"),         0,  1,  true,  "func_words"));//Done
            Form1.functions.Add(new function(("origin"),        0,  1,  true,  "func_origin"));//Done
            Form1.functions.Add(new function(("foreach"),       3,  3,  false,  "func_foreach"));//Done
            Form1.functions.Add(new function(("call"),          1,  0,  true,  "func_call"));//Done
            Form1.functions.Add(new function(("info"),          0,  1,  true,  "func_error"));//Done
            Form1.functions.Add(new function(("error"),         0,  1,  true,  "func_error"));//Done
            Form1.functions.Add(new function(("warning"),       0,  1,  true,  "func_error"));//Done
            Form1.functions.Add(new function(("if"),            2,  3,  false,  "func_if"));//Done
            Form1.functions.Add(new function(("or"),            1,  0,  false,  "func_or"));//Done
            Form1.functions.Add(new function(("and"),           1,  0,  false,  "func_and"));//Done
            Form1.functions.Add(new function(("value"),         0,  1,  true,  "func_value"));//Done
            Form1.functions.Add(new function(("eval"),          0,  1,  true,  "func_eval"));
        }

        // Chop argument into strings and return a list of them
        public static List<string> ChopString(string str)
        {
            List<string> result = new List<string>();
            string[] array = str.Split(new char[] { ' ' });
            string tmp = "";
            for (int i = 0; i < array.Length; i++)
            {
                tmp = array[i].Trim();
                if (tmp != "")
                    result.Add(tmp);
            }
            return result;
        }

        public static string func_abspath(List<string> args)
        {
            List<string> strs = ChopString(args[0]);
            string result = "";
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(abspath " + strs[i] + ")");
                    result += v.name;
                }
                else
                    result += strs[i].Replace(".", "").Replace("//", "/");
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_addprefix(List<string> args)
        {
            string result = "";
            string prefix = args[0];
            List<string> strs = ChopString(args[1]);
            Variable.variable v;
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(addprefix " + prefix + "," + strs[i] + ")");
                    result += v.name;
                }
                else
                    result += prefix + strs[i];
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_addsuffix(List<string> args)
        {
            string result = "";
            string suffix = args[0];
            List<string> strs = ChopString(args[1]);
            Variable.variable v;
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(addsuffix " + suffix + "," + strs[i] + ")");
                    result += v.name;
                }
                else
                    result += strs[i] + suffix;
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_basename(List<string> args)
        {
            string result = "";
            List<string> strs = ChopString(args[0]);
            Variable.variable v;
            int sepIndex;
            int dotIndex;
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(basename " + strs[i] + ")");
                    result += v.name;
                }
                else
                {
                    sepIndex = strs[i].LastIndexOf('/');
                    if (sepIndex < 0)
                        dotIndex = strs[i].LastIndexOf('.');
                    else
                        dotIndex = strs[i].LastIndexOf('.', sepIndex);
                    if (dotIndex < 0)
                        result += strs[i];
                    else
                        result += strs[i].Substring(0, dotIndex);
                }
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_dir(List<string> args)
        {
            string result = "";
            List<string> strs = ChopString(args[0]);
            Variable.variable v;
            int sepIndex;
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(dir " + strs[i] + ")");
                    result += v.name;
                }
                else
                {
                    sepIndex = strs[i].LastIndexOf('/');
                    if (sepIndex < 0)
                        result += "./";
                    else
                        result += strs[i].Substring(0, sepIndex + 1);
                }
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_notdir(List<string> args)
        {
            string result = "";
            Variable.variable v;
            List<string> strs = ChopString(args[0]);
            int sepIndex;
            bool skip = false;
            for (int i = 0; i < strs.Count; i++)
            {
                skip = false;
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(notdir " + strs[i] + ")");
                    result += v.name;
                }
                else
                {
                    sepIndex = strs[i].LastIndexOf('/');
                    if (sepIndex < 0)
                        result += strs[i];
                    else
                    {
                        if (sepIndex == strs[i].Length - 1)
                            skip = true;
                        else
                            result += strs[i].Substring(sepIndex + 1);
                    }
                }
                if (i < strs.Count - 1 && !skip)
                    result += " ";
            }
            return result;
        }

        public static string func_suffix(List<string> args)
        {
            string result = "";
            Variable.variable v;
            List<string> strs = ChopString(args[0]);
            int sepIndex;
            int dotIndex;
            bool skip = false;
            for (int i = 0; i < strs.Count; i++)
            {
                skip = false;
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(suffix " + strs[i] + ")");
                    result += v.name;
                }
                else
                {
                    sepIndex = strs[i].LastIndexOf('/');
                    if (sepIndex < 0)
                        dotIndex = strs[i].LastIndexOf('.');
                    else
                        dotIndex = strs[i].LastIndexOf('.', sepIndex);
                    if (dotIndex < 0)
                        skip = true;
                    else
                        result += strs[i].Substring(dotIndex);
                }
                if (i < strs.Count - 1 && !skip)
                    result += " ";
            }
            return result;
        }

        public static string func_subst(List<string> args)
        {
            Variable.variable v;
            string from = args[0];
            string to = args[1];
            string text = args[2];
            List<string> strs = ChopString(args[2]);
            if (args[0].StartsWith("UNRESOLVED") || args[1].StartsWith("UNRESOLVED"))
            {
                v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(subst " + from + "," + to + "," + text + ")");
                return v.name;
            }
            string result = "";
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(subst " + from + "," + to + "," + strs[i] + ")");
                    result += v.name;
                }
                else
                {
                    result += strs[i].Replace(from, to);
                }
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_filter(List<string> args)
        {
            Variable.variable v;
            List<string> patterns = ChopString(args[0]);
            for (int i = 0; i < patterns.Count; i++)
            {
                patterns[i] = patterns[i].Replace("%", ".*");
            }

            List<string> words = ChopString(args[1]);
            string result = "";
            bool skip = false;
            for (int i = 0; i < words.Count; i++)
            {
                skip = false;
                if (words[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(filter " + args[0] + "," + words[i] + ")");
                    result += v.name;
                }
                else
                {
                    bool match = false;
                    foreach (string pat in patterns)
                    {
                        if (Regex.IsMatch(words[i], pat))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (match)
                        result += words[i];
                    else
                        skip = true;
                }
                if (i < args.Count - 1 && !skip)
                    result += " ";
            }
            return result;
        }

        public static string func_filterout(List<string> args)
        {
            Variable.variable v;
            List<string> patterns = ChopString(args[0]);
            for (int i = 0; i < patterns.Count; i++)
            {
                patterns[i] = patterns[i].Replace("%", ".*");
            }

            List<string> words = ChopString(args[1]);
            string result = "";
            bool skip = false;
            for (int i = 0; i < words.Count; i++)
            {
                skip = false;
                if (words[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(filter-out " + args[0] + "," + words[i] + ")");
                    result += v.name;
                }
                else
                {
                    bool match = false;
                    foreach (string pat in patterns)
                    {
                        if (Regex.IsMatch(words[i], pat))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (match)
                        skip = true;
                    else
                        result += words[i];
                }
                if (i < args.Count - 1 && !skip)
                    result += " ";
            }
            return result;
        }

        public static string func_findstring(List<string> args)
        {
            if (args[1].Contains(args[0]))
                return args[1];
            else
                return "";
        }

        public static string func_firstword(List<string> args)
        {
            List<string> strs = ChopString(args[0]);
            if (strs[0].StartsWith("UNRESOLVED"))
            {
                Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(firstword " + strs[0] + ")");
                return v.name;
            }
            else
                return strs[0];
        }

        public static string func_lastword(List<string> args)
        {
            List<string> strs = ChopString(args[0]);
            if (strs[strs.Count - 1].StartsWith("UNRESOLVED"))
            {
                Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(lastword " + strs[strs.Count - 1] + ")");
                return v.name;
            }
            else
                return strs[strs.Count - 1];
        }

        public static string func_flavor(List<string> args)
        {
            Variable.variable v = Variable.lookup_variable(args[0]);
            if (v == null)
                return "undefined";
            if (v.flavor == Variable.variable_flavor.f_recursive)
                return "recursive";
            else
                return "simple";
        }

        public static string func_join(List<string> args)
        {
            string result = "";
            List<string> list1 = ChopString(args[0]);
            List<string> list2 = ChopString(args[1]);
            if (list1.Count < list2.Count)
            {
                for (int i = 0; i < list1.Count; i++)
                    result += list1[i] + list2[i] + " ";
                for (int i = list1.Count; i < list2.Count; i++)
                {
                    result += list2[i];
                    if (i < list2.Count - 1)
                        result += " ";
                }
                return result;

            }
            if (list1.Count > list2.Count)
            {
                for (int i = 0; i < list2.Count; i++)
                    result += list1[i] + list2[i] + " ";
                for (int i = list2.Count; i < list1.Count; i++)
                {
                    result += list1[i];
                    if (i < list1.Count - 1)
                        result += " ";
                }
                return result;
            }
            if (list1.Count == list2.Count)
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    result += list1[i] + list2[i] + " ";
                    if (i < list1.Count - 1)
                        result += " ";
                }
                return result;
            }
            return result;
        }

        public static string func_patsubst(List<string> args)
        {
            Variable.variable v;
            string pattern = args[0];
            string replace = args[1];
            List<string> words = ChopString(args[2]);
            string result = "";
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(patsubst " + args[0] + "," + args[1] + "," + words[i] + ")");
                    result += v.name;
                }
                else
                    result += Variable.PatternReplace(words[i], pattern, replace);

                if (i < args.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_realpath(List<string> args)
        {
            string result = "";
            List<string> strs = ChopString(args[0]);
            Variable.variable v;
            for (int i = 0; i < strs.Count; i++)
            {
                v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(realpath " + strs[i] + ")");
                result += v.name;
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_shell(List<string> args)
        {
            Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(shell " + args[0] + ")");
            return v.name;
        }

        public static string func_sort(List<string> args)
        {
            Variable.variable v;
            List<string> strs = ChopString(args[0]);
            for (int i = 0; i < strs.Count; i++)
            {
                if (strs[i].StartsWith("UNRESOLVED"))
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(sort " + args[0] + ")");
                    return v.name;
                }
            }
            List<string> final = new List<string>();
            for (int i = 0; i < strs.Count; i++)
            {
                if (!final.Contains(strs[i]))
                    final.Add(strs[i]);
            }
            final.Sort();

            string result = "";
            for (int i = 0; i < final.Count; i++)
            {
                result += final[i];
                if (i < final.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_strip(List<string> args)
        {
            string result = "";
            List<string> strs = ChopString(args[0]);
            for (int i = 0; i < strs.Count; i++)
            {
                result += strs[i];
                if (i < strs.Count - 1)
                    result += " ";
            }
            return result;
        }

        public static string func_wildcard(List<string> args)
        {
            Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(wildcard " + args[0] + ")");
            return v.name;
        }

        public static string func_word(List<string> args)
        {
            Variable.variable v;
            List<string> strs = ChopString(args[1]);
            if (args[0].StartsWith("UNRESOLVED"))
            {
                v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(word " + args[0] + "," + args[1] + ")");
                return v.name;
            }
            int index = int.Parse(args[0]) - 1;
            int unresolvedIndex = -1;
            for (int i = 0; i < strs.Count; i++)
            {
                if(strs[i].StartsWith("UNRESOLVED"))
                {
                    unresolvedIndex = i;
                    break;
                }
            }

            if (index >= strs.Count)
            {
                if (unresolvedIndex == -1)
                    return "";
                else
                {
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(word " + args[0] + "," + args[1] + ")");
                    return v.name;
                }

            }
            else
            {
                if (unresolvedIndex == -1)
                    return strs[index];
                else
                {
                    if (index < unresolvedIndex)
                        return strs[index];
                    else
                    {
                        v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(word " + args[0] + "," + args[1] + ")");
                        return v.name;
                    }
                }
            }
        }

        public static string func_wordlist(List<string> args)
        {
            Variable.variable v;
            if (args[0].StartsWith("UNRESOLVED") || args[1].StartsWith("UNRESOLVED"))
            {
                v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(wordlist " + args[0] + "," + args[1] + "," + args[2] + ")");
                return v.name;
            }
            int start = int.Parse(args[0]) - 1;
            int end = int.Parse(args[1]) - 1;
            List<string> strs = ChopString(args[2]);
            string result = "";
            for (int i = start; i <= end; i++)
            {
                result += strs[i];
                if (i < end)
                    result += " ";
            }
            if(result.Contains("UNRESOLVED"))
            {
                v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(wordlist " + args[0] + "," + args[1] + "," + args[2] + ")");
                return v.name;
            }
            return result;
        }

        public static string func_words(List<string> args)
        {
            if (args[0].StartsWith("UNRESOLVED"))
            {
                Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(words " + args[0] + ")");
                return v.name;
            }
            return ChopString(args[0]).Count.ToString();
        }

        public static string func_origin(List<string> args)
        {
            Variable.variable v = Variable.lookup_variable(args[0]);
            if(v == null)
                return "undefined";
            switch(v.origin)
            {
                default:
                case Variable.variable_origin.o_invalid:
                    return "invalid";
                case Variable.variable_origin.o_default:
                    return "default";
                case Variable.variable_origin.o_env:
                    return "environment";
                case Variable.variable_origin.o_file:
                    return "file";
                case Variable.variable_origin.o_env_override:
                    return "environment override";
                case Variable.variable_origin.o_command:
                    return "command line";
                case Variable.variable_origin.o_override:
                    return "override";
                case Variable.variable_origin.o_automatic:
                    return "automatic";
            }
        }

        public static string func_error(List<string> args)
        {
            return "";
        }

        public static string func_if(List<string> args)
        {
            Variable.variable v = null;
            if (args[0].StartsWith("UNRESOLVED"))
            {
                if(args.Count == 2)
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(if " + args[0] + "," + args[1] + ")");
                else if(args.Count == 3)
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(if " + args[0] + "," + args[1] + "," + args[2] + ")");
                return v.name;
            }
            if (Variable.ExpandString(args[0].Trim()) == "")
            {
                if (args.Count == 3)
                {
                    // Evaluate else part
                    return Variable.ExpandString(args[2].Trim());
                }
                return "";
            }
            else
            {
                // Evaluate then part
                return Variable.ExpandString(args[1].Trim());
            }
        }

        public static string func_or(List<string> args)
        {
            Variable.variable v;
            string expansion = "";
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].StartsWith("UNRESOLVED"))
                {
                    string statement = "";
                    for (int j = 0; j < args.Count; j++)
                    {
                        statement += args[j];
                        if (j < args.Count - 1)
                            statement += " ";
                    }
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(or " + statement + ")");
                    return v.name;
                }
                expansion = Variable.ExpandString(args[i].Trim());
                if (expansion != "")
                    return expansion;
            }
            return "";
        }

        public static string func_and(List<string> args)
        {
            Variable.variable v;
            string expansion = "";
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].StartsWith("UNRESOLVED"))
                {
                    string statement = "";
                    for (int j = 0; j < args.Count; j++)
                    {
                        statement += args[j];
                        if (j < args.Count - 1)
                            statement += " ";
                    }
                    v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(and " + statement + ")");
                    return v.name;
                }
                expansion = Variable.ExpandString(args[i].Trim());
                if (expansion == "")
                    return expansion;
            }
            return expansion;
        }

        public static string func_value(List<string> args)
        {
            Variable.variable v = Variable.lookup_variable(args[0]);
            if (v == null)
                return "";
            else
                return v.value;
        }

        public static string func_foreach(List<string> args)
        {
	        string varname = Variable.ExpandString(args[0]);
            List<string> list = ChopString(Variable.ExpandString(args[1]));
	        string body = args[2];
            /*
            foreach(string item in list)
            {
                if(item.StartsWith("UNRESOLVED"))
	            {
                    Variable.variable v = Variable.define_unresolved_variable(Utils.GetUnresolvedName(), "$(foreach " + args[0] + "," + args[1] + "," + args[2] +  ")", null);
                    return v.name;
                }
            }
            */
            string result = "";
            Variable.variable saveVar = Variable.lookup_variable(varname);
            if (saveVar != null)
                Variable.undefine_variable(varname, null);

	        Variable.variable var = Variable.define_variable_in_set(varname, "", Variable.variable_origin.o_automatic, false, Variable.current_variable_set_list);
            for(int i = 0; i < list.Count; i++)
            {
                var.value = list[i];
                result += Variable.ExpandString(body);
                if(i < list.Count - 1)
                    result += " ";
            }

            Variable.undefine_variable(varname, null);
            if (saveVar != null)
                Variable.current_variable_set_list.Add(saveVar.name, saveVar);

            return result;
        }

        public static string func_call(List<string> args)
        {
            if(args.Count == 0)
                return "";
            // There is no way to define a variable with a space in the name, so strip leading and trailing whitespace as a favor to the user
            string fname = args[0].Trim();
            if(fname == "")
                return "";

            // Are we invoking a builtin function?
            function func = lookup_function(fname);
            if(func != null)
            {
		        // How many arguments do we have?
                List<string> subargs = new List<string>();
                for(int j = 1; j < args.Count; j++)
                    subargs.Add(args[j]);
                return expand_builtin_function("", subargs, func);
            }

	        // Not a builtin, so the first argument is the name of a variable to be expanded and interpreted as a function.  Find it
            Variable.variable v = Variable.lookup_variable(fname);
            if (v == null)
            {
                MessageBox.Show("Undefined Variable in Call '" + fname + "' - (" + (new StackFrame()).GetFileName() + " - " + (new StackFrame()).GetFileLineNumber() + ")");
                return "";
            }
            if(v.value == "")
                return "";
            
            // Set up arguments $(1) .. $(N).  $(0) is the function name
            for(int i = 0; i < args.Count; i++)
                Variable.define_variable_in_set(i.ToString(), args[i], Variable.variable_origin.o_automatic, false, Variable.current_variable_set_list);

            // Expand the body in the context of the arguments, adding the result to the variable buffer
	        string result = Variable.ExpandString("$(" + fname + ")");

            for (int i = 0; i < args.Count; i++)
                Variable.undefine_variable(i.ToString(), null);

	        return result;
        }
        
        public static string func_eval(List<string> args)
        {
            Read.ParseSegment(args[0]);
            return "";
        }


    }
}
