using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.WinForms;
using System.Text.RegularExpressions;
using System.Collections;

namespace MakefileParser
{
    public partial class Form1 : Form
    {
        public static TextReader makefileReader;
        public static string makefileName;
        public static Dictionary<string, Variable.variable> variables;
        public static Dictionary<string, Rule> rules;
        public static Dictionary<string, VariableEntry> VariablesTable;
        public static List<Functions.function> functions;

        public static List<string> Includes;

        public static TextWriter resultsWriter;

        public static TreeView RulesTreeControl;
        public static TreeView VariablesTreeControl;

        public static TreeView SymbolTreeControl;

        public static RichTextBoxEx makefileTextControl;

        public static string TITLE = "Makefile Parser";

        public static string dotFile;
        public static string pdfFile;


        public Form1()
        {
            InitializeComponent();
            makefileText.WordWrap = false;
         

        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            VariablesTreeControl = this.VariablesTree;
            RulesTreeControl = this.RulesTree;
            SymbolTreeControl = this.SymbolsTree;
            makefileTextControl = this.makefileText;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                makefileReader = new StreamReader(openFileDialog1.FileName);
                makefileTextBox.Text = openFileDialog1.FileName;
                makefileName = openFileDialog1.FileName;
                readFile();
            }
        }
        
        private void readFile()
        {
            makefileText.Text = "";
            string line = "";
            while ((line = makefileReader.ReadLine()) != null)
                makefileText.Text += line + "\n";
            makefileReader.Close();
        }

        private void parseButton_Click(object sender, EventArgs e)
        {
            Includes = new List<string>();
            variables = new Dictionary<string, Variable.variable>();
            rules = new Dictionary<string, Rule>();
            functions = new List<Functions.function>();
            VariablesTable = new Dictionary<string, VariableEntry>();
            Functions.FillFunctionsLookupTable();
            Read.ParseMakeFile();

           UpdateVariablesTree();

            UpdateSymbolTree();
            DrawGraph(makefileName.Substring(0, makefileName.LastIndexOf('\\')));
            this.Refresh();
        }

        public static void UpdateVariablesTree()
        {
            TreeView VariablesTree = VariablesTreeControl;

            foreach (Variable.variable var in variables.Values)
            {
                if (var.unresolved)
                {
                    continue;
                }
                TreeNode n = new TreeNode(var.name);
                foreach (VariableEntry.VariableInstance inst in VariablesTable[var.name].instances)
                {
                    string prefix = "reference";
                    if(!inst.isReference)
                    {
                        prefix = "definition";
                    }
                    n.Nodes.Add(new TreeNode(prefix + " [line " + inst.lineno + "]"));
                }
                VariablesTree.Nodes.Add(n);
            }

        }


        public static void UpdateSymbolTree()
        {
            
            TreeView SymbolTree = SymbolTreeControl;

            ArrayList lineNums = new ArrayList();

            string[] tempString = makefileTextControl.Text.Split('\n');

            
           
            // Update the tree view for variables here
            int i = 0;
            foreach (KeyValuePair<string,Variable.variable> v in variables)
            {
               
                if (v.Value.unresolved){
                    int j = 1;
                    foreach (string s in tempString)
                    {
                        if (s.Contains(v.Value.unresolved_value))
                        {   
                            if (!lineNums.Contains(j))
                            lineNums.Add(j);
                        }
                         j++;
                    }
                         TreeNode temp =  SymbolTree.Nodes.Add(v.Value.name);
                         temp.Nodes.Add(v.Value.unresolved_value +" [line " +lineNums.ToArray()[i]+"]");
                         
                         i++;
                        
                }
            }
        }

        public static void UpdateRulesTree()
        {
            TreeView RulesTree = RulesTreeControl;
            RulesTree.Nodes.Clear();
            // Update the tree view for rules here!
            foreach (KeyValuePair<string, Rule> v in rules)
            {
                TreeNode temp;
                //RulesTree.Nodes.Add(new TreeNode("hello"));
                if (v.Value.singleColon)
                {

                    //single colon and in tree
                    if (RulesTree.Nodes.ContainsKey(v.Value.target))
                    {
                        Console.WriteLine("in single colon");
                        RulesTree.Nodes[RulesTree.Nodes.IndexOf(new TreeNode(v.Value.target))].Nodes.Add(new TreeNode("index temp"));

                    }
                    else
                    {

                        //singlecolor and not in tree
                        temp = new TreeNode(v.Value.target);
                        RulesTree.Nodes.Add(temp);

                        for (int i = 0; i < v.Value.prerequisites.Count; i++)
                        {
                            TreeNode toAdd = new TreeNode("[PREQ]" + v.Value.prerequisites[i] + " [line " + v.Value.lineno + "]");
                            temp.Nodes.Add(toAdd);
                        }
                        TreeNode toAddRec = new TreeNode("[RECIPE]" + v.Value.recipe + " [line " + (v.Value.lineno+1) +"]");
                        temp.Nodes.Add(toAddRec);
                        //RulesTree.Nodes.Add(new TreeNode("hello"));
                        Console.WriteLine(v.Value.lineno);
                    }

                }
                else
                {
                    temp = new TreeNode(v.Value.target);
                    RulesTree.Nodes.Add(temp);

                    for (int i = 0; i < v.Value.prerequisites.Count; i++)
                    {
                        TreeNode toAddPreq = new TreeNode("[PREQ]" + v.Value.prerequisites[i] + " [line " + v.Value.lineno + "]");
                        temp.Nodes.Add(toAddPreq);
                    }
                    TreeNode toAddRec = new TreeNode("[RECIPE]" + v.Value.recipe + " [line " + (v.Value.lineno+1) + "]");
                    temp.Nodes.Add(toAddRec);
                }
                //TreeNode node;
            }
        }

        public static void UpdateVariablesTable(string varname, bool isReference)
        {
            if (Utils.isSymbolic(varname) || Utils.isAutomaticVar(varname))
                return;
            VariableEntry varEntry;
            if (VariablesTable.ContainsKey(varname))
                varEntry = VariablesTable[varname];
            else
            {
                varEntry = new VariableEntry(varname);
                VariablesTable.Add(varname, varEntry);
            }
            VariableEntry.VariableInstance varInstance = new VariableEntry.VariableInstance(isReference, Read.LINENO);
            varEntry.instances.Add(varInstance);
        }

        private void DrawGraph(string graphFile)
        {
            string fileName = DateTime.Now.Ticks.ToString() + ".dot";
            dotFile = graphFile + "/" + fileName;
            pdfFile = graphFile + "/" + fileName.Replace("dot", "pdf");
            TextWriter graphW = new StreamWriter(graphFile + "/" +fileName);
            string rectangle = "rectangle";
            string oval = "oval";
            List<string> graph = new List<string>();
            graph.Add("digraph G {");
            graph.Add("compound=true;");

            int rcpCount = 0;
            // Start of graph
            foreach (Rule rule in rules.Values)
            {
                rcpCount++;
                graph.Add(GetGrapvisNode(rule.target, rectangle));
                graph.Add(GetGrapvisNode("rcp" + rcpCount, oval));
                graph.Add("\"" + rule.target + "\" -> \"rcp" + rcpCount + "\";");
                foreach (string preq in rule.prerequisites)
                {
                    graph.Add(GetGrapvisNode(preq, rectangle));
                    graph.Add("\"rcp" + rcpCount + "\" -> \"" + preq + "\";");
                }
            }
            // End of graph
            graph.Add("}");
            List<string> noDuplicates = new List<string>();
            for (int i = 0; i < graph.Count; i++)
            {
                if (!noDuplicates.Contains(graph[i]))
                    noDuplicates.Add(graph[i]);
            }
            foreach (string line in noDuplicates)
                graphW.WriteLine(line);
            graphW.Flush();
            graphW.Close();
            ConvertToPDF();
        }

        public static void ConvertToPDF()
        {
            // Convert the dot graph into pdf graph
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo stratInfo = new System.Diagnostics.ProcessStartInfo();
            stratInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            stratInfo.FileName = "cmd.exe";
            stratInfo.Arguments = "/C dot -Tpdf " + dotFile + " -o " + pdfFile;
            process.StartInfo = stratInfo;
            process.Start();
        }

        private string GetGrapvisNode(string id, string shape)
        {
            if (Utils.isSymbolic(id))
                return ("\"" + id + "\" [shape=" + shape + ", label=<<I>SYM" + int.Parse(id.Substring(10)) + "</I>>];");
            if(id.StartsWith("UNRESOLVED"))
                return ("\"" + id + "\" [shape=" + shape + ", label=<<I>SYM" + int.Parse(id.Substring(10, 6)) + "</I>>];");
            if(id.StartsWith("Concat"))
                return ("\"" + id + "\" [shape=oval, label=<<B>Concat</B>>];");
            return ("\"" + id + "\" [shape=" + shape + "];");
        }

        private string GetGrapvisNode(string id, string shape, string label)
        {
            if(Utils.isSymbolic(id))
                return ("\"" + id + "\" [shape=" + shape + ", label=<<I>SYM" + int.Parse(id.Substring(10)) + "</I>>];");
            if (id.StartsWith("UNRESOLVED"))
                return ("\"" + id + "\" [shape=" + shape + ", label=<<I>SYM" + int.Parse(id.Substring(10, 6)) + "</I>>];");
            if (id.StartsWith("Concat"))
                return ("\"" + id + "\" [shape=oval, label=<<B>Concat</B>>];");
            if(label == null)
                return ("\"" + id + "\" [shape=" + shape + "];");
            else
                return ("\"" + id + "\" [shape=" + shape + ", label=\"" + label + "\"];");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            GenerateSDG();
        }

        public static void GenerateSDG()
        {
            SDGGraph form = new SDGGraph();
            form.Show();
            form.axAcroPDF1.LoadFile(pdfFile);
            form.axAcroPDF1.Show(); 
        }

      

        private void SymbolsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (!e.Node.Text.ToLower().Contains("unresolved"))
            {
                
                //get line number out
                int lineNumber = int.Parse(e.Node.Text.Split('[')[1].Split(']')[0].Split(' ')[1]);

                //get text out
                String text = e.Node.Text.Split('[')[0];

                int start = makefileTextControl.GetFirstCharIndexFromLine(lineNumber-1);
                int end = makefileTextControl.Lines[lineNumber-1].Length;

                    makefileTextControl.SelectionBackColor = Color.LightBlue;


                makefileTextControl.Select(start, end);
                makefileTextControl.Refresh();
                this.Refresh();
            }

        }

        private void RulesTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Text.ToLower().Contains("line") )
            {

                //get line number out
                int lineNumber = int.Parse(e.Node.Text.Substring((e.Node.Text.LastIndexOf("]") - 2), 2));
                //get text out
                String text = e.Node.Text.Substring(e.Node.Text.IndexOf("]")+e.Node.Text.LastIndexOf("[")-e.Node.Text.IndexOf("]"));

                int start = makefileTextControl.GetFirstCharIndexFromLine(lineNumber - 1);
                int end = makefileTextControl.Lines[lineNumber - 1].Length;

                makefileTextControl.SelectionBackColor = Color.LightBlue;


                makefileTextControl.Select(start, end);
                makefileTextControl.Refresh();
                this.Refresh();
            }
        }
    }
}
