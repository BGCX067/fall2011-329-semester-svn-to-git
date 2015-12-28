namespace MakefileParser
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.makefileTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.parseButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.makefileText = new Ionic.WinForms.RichTextBoxEx();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.VariablesTree = new System.Windows.Forms.TreeView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.RulesTree = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.SymbolsTree = new System.Windows.Forms.TreeView();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Makefile:";
            // 
            // makefileTextBox
            // 
            this.makefileTextBox.Location = new System.Drawing.Point(68, 6);
            this.makefileTextBox.Name = "makefileTextBox";
            this.makefileTextBox.Size = new System.Drawing.Size(291, 20);
            this.makefileTextBox.TabIndex = 1;
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(365, 4);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // parseButton
            // 
            this.parseButton.Location = new System.Drawing.Point(446, 4);
            this.parseButton.Name = "parseButton";
            this.parseButton.Size = new System.Drawing.Size(102, 23);
            this.parseButton.TabIndex = 3;
            this.parseButton.Text = "Parse Makefile";
            this.parseButton.UseVisualStyleBackColor = true;
            this.parseButton.Click += new System.EventHandler(this.parseButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.makefileText);
            this.groupBox1.Location = new System.Drawing.Point(7, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 479);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Makefile Viewer:";
            // 
            // makefileText
            // 
            this.makefileText.AcceptsTab = true;
            this.makefileText.AutoWordSelection = true;
            this.makefileText.DetectUrls = false;
            this.makefileText.Location = new System.Drawing.Point(8, 19);
            this.makefileText.Name = "makefileText";
            this.makefileText.NumberAlignment = System.Drawing.StringAlignment.Center;
            this.makefileText.NumberBackground1 = System.Drawing.SystemColors.ControlLight;
            this.makefileText.NumberBackground2 = System.Drawing.SystemColors.Window;
            this.makefileText.NumberBorder = System.Drawing.SystemColors.ControlDark;
            this.makefileText.NumberBorderThickness = 1F;
            this.makefileText.NumberColor = System.Drawing.Color.DarkGray;
            this.makefileText.NumberFont = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.makefileText.NumberLeadingZeroes = false;
            this.makefileText.NumberLineCounting = Ionic.WinForms.RichTextBoxEx.LineCounting.CRLF;
            this.makefileText.NumberPadding = 2;
            this.makefileText.ShowLineNumbers = true;
            this.makefileText.Size = new System.Drawing.Size(438, 453);
            this.makefileText.TabIndex = 5;
            this.makefileText.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.InitialDirectory = "./";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.VariablesTree);
            this.groupBox2.Location = new System.Drawing.Point(472, 34);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(294, 219);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Makefile Variables:";
            // 
            // VariablesTree
            // 
            this.VariablesTree.Location = new System.Drawing.Point(7, 20);
            this.VariablesTree.Name = "VariablesTree";
            this.VariablesTree.Size = new System.Drawing.Size(279, 193);
            this.VariablesTree.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.RulesTree);
            this.groupBox3.Location = new System.Drawing.Point(472, 259);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(294, 249);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Makefile Rules:";
            // 
            // RulesTree
            // 
            this.RulesTree.Location = new System.Drawing.Point(7, 20);
            this.RulesTree.Name = "RulesTree";
            this.RulesTree.Size = new System.Drawing.Size(279, 222);
            this.RulesTree.TabIndex = 0;
            this.RulesTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.RulesTree_NodeMouseClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(554, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(114, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Generate SDG";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.SymbolsTree);
            this.groupBox4.Location = new System.Drawing.Point(772, 34);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(294, 219);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Makefile Symbols:";
            // 
            // SymbolsTree
            // 
            this.SymbolsTree.Location = new System.Drawing.Point(9, 19);
            this.SymbolsTree.Name = "SymbolsTree";
            this.SymbolsTree.Size = new System.Drawing.Size(279, 193);
            this.SymbolsTree.TabIndex = 0;
            this.SymbolsTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.SymbolsTree_NodeMouseClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1075, 513);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.parseButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.makefileTextBox);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Makefile Parser";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox makefileTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button parseButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.TreeView RulesTree;
        public System.Windows.Forms.TreeView VariablesTree;
        public Ionic.WinForms.RichTextBoxEx makefileText;
        private System.Windows.Forms.GroupBox groupBox4;
        public System.Windows.Forms.TreeView SymbolsTree;
    }
}

