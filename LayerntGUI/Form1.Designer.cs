namespace LayerntGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            checkBox4 = new CheckBox();
            label12 = new Label();
            comboBox2 = new ComboBox();
            label11 = new Label();
            label10 = new Label();
            checkBox3 = new CheckBox();
            label7 = new Label();
            progressBar1 = new ProgressBar();
            button4 = new Button();
            textBox5 = new TextBox();
            label5 = new Label();
            checkBox1 = new CheckBox();
            textBox4 = new TextBox();
            button3 = new Button();
            textBox3 = new TextBox();
            label4 = new Label();
            comboBox1 = new ComboBox();
            label3 = new Label();
            button2 = new Button();
            textBox2 = new TextBox();
            label2 = new Label();
            button1 = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            tabPage2 = new TabPage();
            textBox9 = new TextBox();
            progressBar2 = new ProgressBar();
            button7 = new Button();
            label9 = new Label();
            checkBox2 = new CheckBox();
            textBox8 = new TextBox();
            button6 = new Button();
            textBox7 = new TextBox();
            label8 = new Label();
            button5 = new Button();
            textBox6 = new TextBox();
            label6 = new Label();
            toolTip1 = new ToolTip(components);
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(601, 310);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(checkBox4);
            tabPage1.Controls.Add(label12);
            tabPage1.Controls.Add(comboBox2);
            tabPage1.Controls.Add(label11);
            tabPage1.Controls.Add(label10);
            tabPage1.Controls.Add(checkBox3);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(progressBar1);
            tabPage1.Controls.Add(button4);
            tabPage1.Controls.Add(textBox5);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(checkBox1);
            tabPage1.Controls.Add(textBox4);
            tabPage1.Controls.Add(button3);
            tabPage1.Controls.Add(textBox3);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(comboBox1);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(button2);
            tabPage1.Controls.Add(textBox2);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(593, 282);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Save";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Checked = true;
            checkBox4.CheckState = CheckState.Checked;
            checkBox4.Location = new Point(226, 130);
            checkBox4.Name = "checkBox4";
            checkBox4.RightToLeft = RightToLeft.Yes;
            checkBox4.Size = new Size(15, 14);
            checkBox4.TabIndex = 25;
            checkBox4.UseVisualStyleBackColor = true;
            checkBox4.CheckedChanged += checkBox4_CheckedChanged;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(184, 129);
            label12.Name = "label12";
            label12.Size = new Size(36, 15);
            label12.TabIndex = 24;
            label12.Text = "Auto:";
            toolTip1.SetToolTip(label12, resources.GetString("label12.ToolTip"));
            // 
            // comboBox2
            // 
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.Enabled = false;
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "1bit", "2bit", "3bit", "4bit", "5bit", "6bit", "7bit", "8bit", "9bit", "10bit", "11bit", "12bit", "13bit", "14bit", "15bit", "16bit" });
            comboBox2.Location = new Point(107, 126);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(71, 23);
            comboBox2.TabIndex = 23;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(5, 130);
            label11.Name = "label11";
            label11.Size = new Size(96, 15);
            label11.TabIndex = 22;
            label11.Text = "Bits Per Channel:";
            toolTip1.SetToolTip(label11, resources.GetString("label11.ToolTip"));
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(366, 130);
            label10.Name = "label10";
            label10.Size = new Size(163, 15);
            label10.TabIndex = 21;
            label10.Text = "Use Safe Platform Encryption:";
            toolTip1.SetToolTip(label10, resources.GetString("label10.ToolTip"));
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Location = new Point(535, 131);
            checkBox3.Name = "checkBox3";
            checkBox3.RightToLeft = RightToLeft.Yes;
            checkBox3.Size = new Size(15, 14);
            checkBox3.TabIndex = 20;
            checkBox3.UseVisualStyleBackColor = true;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(16, 228);
            label7.Name = "label7";
            label7.Size = new Size(151, 15);
            label7.TabIndex = 19;
            label7.Text = "* Hover on titles to see tips.";
            label7.Click += label7_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(16, 185);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(569, 40);
            progressBar1.TabIndex = 17;
            // 
            // button4
            // 
            button4.Location = new Point(240, 231);
            button4.Name = "button4";
            button4.Size = new Size(97, 43);
            button4.TabIndex = 16;
            button4.Text = "Save";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // textBox5
            // 
            textBox5.BorderStyle = BorderStyle.None;
            textBox5.CausesValidation = false;
            textBox5.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            textBox5.ForeColor = SystemColors.ActiveCaptionText;
            textBox5.Location = new Point(184, 90);
            textBox5.Margin = new Padding(3, 0, 3, 0);
            textBox5.Multiline = true;
            textBox5.Name = "textBox5";
            textBox5.ReadOnly = true;
            textBox5.Size = new Size(401, 33);
            textBox5.TabIndex = 15;
            textBox5.TabStop = false;
            textBox5.Text = "efewfwef";
            textBox5.TextAlign = HorizontalAlignment.Center;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 159);
            label5.Name = "label5";
            label5.Size = new Size(67, 15);
            label5.TabIndex = 14;
            label5.Text = "Encryption:";
            toolTip1.SetToolTip(label5, resources.GetString("label5.ToolTip"));
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(83, 160);
            checkBox1.Name = "checkBox1";
            checkBox1.RightToLeft = RightToLeft.Yes;
            checkBox1.Size = new Size(15, 14);
            checkBox1.TabIndex = 13;
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // textBox4
            // 
            textBox4.Enabled = false;
            textBox4.Location = new Point(104, 156);
            textBox4.Name = "textBox4";
            textBox4.PasswordChar = '*';
            textBox4.Size = new Size(478, 23);
            textBox4.TabIndex = 12;
            // 
            // button3
            // 
            button3.Location = new Point(509, 64);
            button3.Name = "button3";
            button3.Size = new Size(76, 23);
            button3.TabIndex = 10;
            button3.Text = "Save";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(107, 64);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(396, 23);
            textBox3.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 67);
            label4.Name = "label4";
            label4.Size = new Size(84, 15);
            label4.TabIndex = 8;
            label4.Text = "Output Image:";
            toolTip1.SetToolTip(label4, "The resulting outgoing image file location.");
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "24 Bits", "32 Bits", "48 Bits", "64 Bits" });
            comboBox1.Location = new Point(107, 97);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(71, 23);
            comboBox1.TabIndex = 7;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(24, 100);
            label3.Name = "label3";
            label3.Size = new Size(77, 15);
            label3.TabIndex = 6;
            label3.Text = "Bits Per Pixel:";
            toolTip1.SetToolTip(label3, resources.GetString("label3.ToolTip"));
            // 
            // button2
            // 
            button2.Location = new Point(509, 35);
            button2.Name = "button2";
            button2.Size = new Size(76, 23);
            button2.TabIndex = 5;
            button2.Text = "Browse";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(107, 35);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(396, 23);
            textBox2.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(32, 38);
            label2.Name = "label2";
            label2.Size = new Size(69, 15);
            label2.TabIndex = 3;
            label2.Text = "File to Save:";
            toolTip1.SetToolTip(label2, "The file to save.");
            // 
            // button1
            // 
            button1.Location = new Point(509, 6);
            button1.Name = "button1";
            button1.Size = new Size(76, 23);
            button1.TabIndex = 2;
            button1.Text = "Browse";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(107, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(396, 23);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 10);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 0;
            label1.Text = "Image to Use:";
            toolTip1.SetToolTip(label1, "The image to use for saving.");
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(textBox9);
            tabPage2.Controls.Add(progressBar2);
            tabPage2.Controls.Add(button7);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(checkBox2);
            tabPage2.Controls.Add(textBox8);
            tabPage2.Controls.Add(button6);
            tabPage2.Controls.Add(textBox7);
            tabPage2.Controls.Add(label8);
            tabPage2.Controls.Add(button5);
            tabPage2.Controls.Add(textBox6);
            tabPage2.Controls.Add(label6);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(593, 285);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Read";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox9
            // 
            textBox9.BorderStyle = BorderStyle.None;
            textBox9.CausesValidation = false;
            textBox9.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            textBox9.ForeColor = SystemColors.ActiveCaptionText;
            textBox9.Location = new Point(9, 110);
            textBox9.Margin = new Padding(3, 0, 3, 0);
            textBox9.Multiline = true;
            textBox9.Name = "textBox9";
            textBox9.ReadOnly = true;
            textBox9.Size = new Size(569, 33);
            textBox9.TabIndex = 20;
            textBox9.TabStop = false;
            textBox9.TextAlign = HorizontalAlignment.Center;
            // 
            // progressBar2
            // 
            progressBar2.Location = new Point(9, 160);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new Size(569, 40);
            progressBar2.TabIndex = 19;
            // 
            // button7
            // 
            button7.Location = new Point(238, 225);
            button7.Name = "button7";
            button7.Size = new Size(97, 43);
            button7.TabIndex = 18;
            button7.Text = "Read";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 67);
            label9.Name = "label9";
            label9.Size = new Size(67, 15);
            label9.TabIndex = 17;
            label9.Text = "Encryption:";
            toolTip1.SetToolTip(label9, resources.GetString("label9.ToolTip"));
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(79, 68);
            checkBox2.Name = "checkBox2";
            checkBox2.RightToLeft = RightToLeft.Yes;
            checkBox2.Size = new Size(15, 14);
            checkBox2.TabIndex = 16;
            checkBox2.UseVisualStyleBackColor = true;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            // 
            // textBox8
            // 
            textBox8.Enabled = false;
            textBox8.Location = new Point(100, 64);
            textBox8.Name = "textBox8";
            textBox8.PasswordChar = '*';
            textBox8.Size = new Size(478, 23);
            textBox8.TabIndex = 15;
            // 
            // button6
            // 
            button6.Location = new Point(505, 35);
            button6.Name = "button6";
            button6.Size = new Size(80, 23);
            button6.TabIndex = 5;
            button6.Text = "Browse";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // textBox7
            // 
            textBox7.Location = new Point(100, 35);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(399, 23);
            textBox7.TabIndex = 4;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(8, 38);
            label8.Name = "label8";
            label8.Size = new Size(84, 15);
            label8.TabIndex = 3;
            label8.Text = "Output Folder:";
            // 
            // button5
            // 
            button5.Location = new Point(505, 6);
            button5.Name = "button5";
            button5.Size = new Size(80, 23);
            button5.TabIndex = 2;
            button5.Text = "Browse";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(100, 6);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(399, 23);
            textBox6.TabIndex = 1;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(8, 9);
            label6.Name = "label6";
            label6.Size = new Size(86, 15);
            label6.TabIndex = 0;
            label6.Text = "Image to Read:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(601, 310);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Layernt";
            Load += Form1_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button button1;
        private TextBox textBox1;
        private Label label1;
        private Button button2;
        private TextBox textBox2;
        private Label label2;
        private ComboBox comboBox1;
        private Label label3;
        private ToolTip toolTip1;
        private Button button3;
        private TextBox textBox3;
        private Label label4;
        private CheckBox checkBox1;
        private TextBox textBox4;
        private TextBox textBox5;
        private Label label5;
        private ProgressBar progressBar1;
        private Button button4;
        private Label label7;
        private Button button5;
        private TextBox textBox6;
        private Label label6;
        private Label label9;
        private CheckBox checkBox2;
        private TextBox textBox8;
        private Button button6;
        private TextBox textBox7;
        private Label label8;
        private TextBox textBox9;
        private ProgressBar progressBar2;
        private Button button7;
        private Label label10;
        private CheckBox checkBox3;
        private CheckBox checkBox4;
        private Label label12;
        private ComboBox comboBox2;
        private Label label11;
    }
}