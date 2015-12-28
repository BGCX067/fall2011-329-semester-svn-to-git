using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MakefileParser
{
    public class Utils
    {
        public static int UNRESOLVED_ID = 100000;
        public static int TARGET_ID = 100000;

        public static bool isblank(char s)
        {
            return (s == ' ' || s == '\t');
        }

        public static bool isSymbolic(string name)
        {
            return ((name.Length == "UNRESOLVED".Length + 6) && name.StartsWith("UNRESOLVED"));
        }

        public static bool isAutomaticVar(string name)
        {
            return ((name == "<") || (name == "^") || (name == "@") || (name == "+") || (name == "*") || (name == "?"));
        }

        public static string next_token(string s)
        {
            int i = 0;
            for (; i < s.Length; i++)
                if (!isblank(s[i]))
                    return s.Substring(i);
            return s.Substring(i);
        }

        public static string end_of_token(string s)
        {
            int i = 0;
            for (; i < s.Length; i++)
                if (s[i] == '\0' || isblank(s[i]))
                    return s.Substring(i);
            return s.Substring(i);
        }

        public static string GetUnresolvedName()
        {
            return "UNRESOLVED" + (++UNRESOLVED_ID);
        }

        public static string GetTargerName(string target)
        {
            return target + "_TARGET" + (++TARGET_ID);
        }

        /* Find the next token in PTR; return the address of it, and store the length of the token into *LENGTHPTR if LENGTHPTR is not nil.  Set *PTR to the end
         * of the token, so this function can be called repeatedly in a loop.  */
        public static string find_next_token (ref string ptr)
        {
	        string p = next_token(ptr);
	        if (p.Length == 0)
		        return null;
	        ptr = end_of_token(p);
            return p;
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

        public static int find_char_unquote(ref string strings, char stop1, char stop2, bool blank, char ignorevars)
        {
            int string_len = 0;
            int index = 0;
            string p = strings;
            
            if (ignorevars.ToString().Length != 0)
                ignorevars = '$';

            while (true)
            {
                if (stop2.ToString().Length != 0 && blank)
                    while (index < strings.Length && p[index] != ignorevars && p[index] != stop1 && p[index] != stop2 && ! isblank(p[index]))
                        index++;
                else if (stop2.ToString().Length != 0)
                    while (index < strings.Length && p[index] != ignorevars && p[index] != stop1 && p[index] != stop2)
                        index++;
                else if (blank)
                    while (index < strings.Length && p[index] != ignorevars && p[index] != stop1 && !isblank (p[index]))
                        index++;
                else
                    while (index < strings.Length && p[index] != ignorevars && p[index] != stop1)
                        index++;
                
                if (index == strings.Length)
                    break;
                
                // If we stopped due to a variable reference, skip over its contents.
                if (p[index] == ignorevars)
                {
                    char openparen = p[index + 1];
                    index += 2;
                    
                    // Skip the contents of a non-quoted, multi-char variable ref.
                    if (openparen == '(' || openparen == '{')
                    {
                        int pcount = 1;
                        char closeparen = (openparen == '(' ? ')' : '}');

                        while (index < p.Length)
                        {
                            if (p[index] == openparen)
                                ++pcount;
                            else if (p[index] == closeparen)
                                if (--pcount == 0)
                                {
                                    index++;
                                    break;
                                }
                            index++;
                        }
                    }
                    // Skipped the variable reference: look for STOPCHARS again.
                    continue;
                }
                if (index > 0 && p[index - 1] == '\\')
                {
                    // Search for more backslashes.
                    int i = index - 2;
                    while (p[i] >= 0 && p[i] == '\\')
                        --i;
                    ++i;
                    // Only compute the length if really needed.
                    if (string_len == 0)
                        string_len = strings.Length;
                    // The number of backslashes is now -I. Copy P over itself to swallow half of them.
                    char [] d = p.ToCharArray();
                    d[i] = d[i/2];
                    strings = p = d.ToString();
                    index += i/2;
                    if (i % 2 == 0)
                        // All the backslashes quoted each other; the STOPCHAR was unquoted.
                        return index;

                    // The STOPCHAR was quoted by a backslash.  Look for another.
                }
                else
                    // No backslash in sight.
                    return index;
            }
            // Never hit a STOPCHAR or blank (with BLANK nonzero).
            return -1;
        }







    }
}
