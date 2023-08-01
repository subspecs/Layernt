using System.IO.Pipes;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LayerntGUI
{

    public partial class Form1 : Form
    {
        // To support flashing.
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
        public const UInt32 FLASHW_ALL = 3;

        // Flash continuously until the window comes to the foreground. 
        public const UInt32 FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        // Do the flashing - this does not involve a raincoat.
        public static bool FlashWindowEx(Form form)
        {
            IntPtr hWnd = form.Handle;
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }





        bool DoesntFit = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            textBox5.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ODialog = new OpenFileDialog();
            ODialog.Filter = "Image Files|*.png;*.tiff;*.tga;*.webp;*.bmp";
            if (ODialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Input.
                    textBox1.Text = ODialog.FileName;
                    comboBox1_SelectedIndexChanged(null, null);
                }
                catch { }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ODialog = new SaveFileDialog();
            ODialog.Filter = "Image Files|*.png;*.tiff;*.tga;*.webp;*.bmp";
            if (ODialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Output.
                    textBox3.Text = ODialog.FileName;
                    comboBox1_SelectedIndexChanged(null, null);
                }
                catch { }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ODialog = new OpenFileDialog();
            ODialog.Filter = "Any Files|*.*";
            if (ODialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Data.
                    textBox2.Text = ODialog.FileName;
                    comboBox1_SelectedIndexChanged(null, null);
                }
                catch { }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; textBox5.Text = "";

            switch (comboBox1.SelectedIndex)
            {
                case 0: //24bit.
                    {
                        if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
                        {
                            if (System.IO.File.Exists(textBox1.Text) && System.IO.File.Exists(textBox2.Text)) //Both files exist,
                            {
                                int FileSize = (int)(new System.IO.FileInfo(textBox2.Text).Length);
                                int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, 8, checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, 8, checkBox1.Checked, null);
                                textBox5.Text = "Total Available: " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";
                                return;
                            }
                        }

                        textBox5.Text = "";
                    }
                    break;
                case 1: //32bit.
                    {
                        if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
                        {
                            if (System.IO.File.Exists(textBox1.Text) && System.IO.File.Exists(textBox2.Text)) //Both files exist,
                            {
                                int FileSize = (int)(new System.IO.FileInfo(textBox2.Text).Length);
                                int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, 8, checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, 8, checkBox1.Checked, null);
                                textBox5.Text = "Total Available: " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";
                                return;
                            }
                        }

                        textBox5.Text = "";
                    }
                    break;
                case 2: //48bit.
                    {
                        if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
                        {
                            if (System.IO.File.Exists(textBox1.Text) && System.IO.File.Exists(textBox2.Text)) //Both files exist,
                            {
                                int FileSize = (int)(new System.IO.FileInfo(textBox2.Text).Length);
                                int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, 16, checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                                else if (textBox3.Text != null && textBox3.Text != "" && System.IO.Path.GetExtension(textBox3.Text) != ".png") { textBox3.Text = System.IO.Path.ChangeExtension(textBox3.Text, ".png"); textBox5.Text += "\r\nNOTE: 48bit+ images only support the '.png' format, changing output."; DoesntFit = false; textBox5.ForeColor = Color.Violet; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, 16, checkBox1.Checked, null);
                                textBox5.Text = "Total Available: " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";
                                return;
                            }
                        }

                        textBox5.Text = "";
                    }
                    break;
                case 3: //64bit.
                    {
                        if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
                        {
                            if (System.IO.File.Exists(textBox1.Text) && System.IO.File.Exists(textBox2.Text)) //Both files exist,
                            {
                                int FileSize = (int)(new System.IO.FileInfo(textBox2.Text).Length);
                                int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, 16, checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                                else if (textBox3.Text != null && textBox3.Text != "" && System.IO.Path.GetExtension(textBox3.Text) != ".png") { textBox3.Text = System.IO.Path.ChangeExtension(textBox3.Text, ".png"); textBox5.Text += "\r\nNOTE: 48bit+ images only support the '.png' format, changing output."; DoesntFit = false; textBox5.ForeColor = Color.Violet; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, 16, checkBox1.Checked, null);
                                textBox5.Text = "Total Available: " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";
                                return;
                            }
                        }

                        textBox5.Text = "";
                    }
                    break;
            }


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox4.Enabled = true;
            }
            else
            {
                textBox4.Enabled = false;
            }
            comboBox1_SelectedIndexChanged(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(null, null); //Process bs.
            if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
            {
                if (System.IO.File.Exists(textBox1.Text) && System.IO.File.Exists(textBox2.Text)) //Both files exist,
                {
                    if (textBox3.Text != null && textBox3.Text != "" && textBox3.Text.IndexOfAny(Path.GetInvalidPathChars()) == -1) //Output.
                    {
                        if (checkBox1.Checked && (textBox4.Text == null || textBox4.Text == "")) { textBox5.Text += (textBox5.Text != "" ? "\r\n" : "") + "ERROR: Write a password if you're gonna use encryption!"; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                        else if (!DoesntFit)
                        {
                            byte[] Data = System.IO.File.ReadAllBytes(textBox2.Text);

                            switch (comboBox1.SelectedIndex)
                            {
                                case 0:
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, Data.Length, System.IO.Path.GetFileName(textBox2.Text));
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage24(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, textBox2.Text);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage24(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), textBox2.Text, checkBox3.Checked);
                                        }
                                        progressBar1.Value = 100;
                                        FlashWindowEx(this);
                                    }
                                    break;
                                case 1: //32bit.
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, Data.Length, System.IO.Path.GetFileName(textBox2.Text));
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage32(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, textBox2.Text);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage32(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), textBox2.Text, checkBox3.Checked);
                                        }
                                        progressBar1.Value = 100;
                                        FlashWindowEx(this);
                                    }
                                    break;
                                case 2:
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, Data.Length, System.IO.Path.GetFileName(textBox2.Text), 16);
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage48(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, textBox2.Text);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage48(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), textBox2.Text, checkBox3.Checked);
                                        }
                                        progressBar1.Value = 100;
                                        FlashWindowEx(this);
                                    }
                                    break;
                                case 3:
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, Data.Length, System.IO.Path.GetFileName(textBox2.Text), 16);
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage64(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, textBox2.Text);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage64(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), textBox2.Text, checkBox3.Checked);

                                        }
                                        progressBar1.Value = 100;
                                        FlashWindowEx(this);
                                    }
                                    break;
                            }
                        }
                        else { textBox5.Text += (textBox5.Text != "" ? "\r\n" : "") + "ERROR: Make sure the file to save fits inside the image!"; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                    }
                    else { textBox5.Text += (textBox5.Text != "" ? "\r\n" : "") + "ERROR: The output image has to be a real path! (aka there are invalid symbols in the path.)"; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                }
                else { textBox5.Text += (textBox5.Text != "" ? "\r\n" : "") + "ERROR: Make sure the Input Image and the file you wish to save into the image exist!"; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
            }
            else { textBox5.Text += (textBox5.Text != "" ? "\r\n" : "") + "ERROR: Make sure the Input Image and the file you wish to save into the image exist!"; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            var ODialog = new OpenFileDialog();
            ODialog.Filter = "Image Files|*.png;*.tiff;*.tga;*.webp;*.bmp";
            if (ODialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Input.
                    textBox6.Text = ODialog.FileName;
                    //comboBox1_SelectedIndexChanged(null, null);

                }
                catch { }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox7.Text = fbd.SelectedPath;
                    //comboBox1_SelectedIndexChanged(null, null);

                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox8.Enabled = true;
            }
            else
            {
                textBox8.Enabled = false;
            }
            //comboBox1_SelectedIndexChanged(null, null);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //WRITE CHECKS!

            if (textBox6.Text == null || textBox6.Text == "") { textBox9.Text = "ERROR: Must specify an input file."; return; } //Input.
            if (textBox7.Text == null || textBox7.Text == "") { textBox9.Text = "ERROR: Must specify an output directory."; return; } //Folder.
            if (checkBox2.Checked && (textBox8.Text == null || textBox8.Text == "")) { textBox9.Text = "ERROR: If using encryption, password cannot be empty."; return; } //Enc Password.

            textBox9.Text = "";
            switch (Layernt.Layernt.GetFilePixelBits(textBox6.Text))
            {
                case 24:
                    {
                        progressBar2.Value = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage24(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage24(textBox6.Text, out byte[] Data, out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            textBox9.Text = "SUCCESS: File '" + DataFileName + "' successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        FlashWindowEx(this);
                    }
                    break;
                case 32:
                    {
                        progressBar2.Value = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage32(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage32(textBox6.Text, out byte[] Data, out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            textBox9.Text = "SUCCESS: File '" + DataFileName + "' successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        FlashWindowEx(this);
                    }
                    break;
                case 48:
                    {
                        progressBar2.Value = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage48(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage48(textBox6.Text, out byte[] Data, out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            textBox9.Text = "SUCCESS: File '" + DataFileName + "' successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        FlashWindowEx(this);
                    }
                    break;
                case 64:
                    {
                        progressBar2.Value = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage64(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage64(textBox6.Text, out byte[] Data, out DataFileName);
                                if (Data.Length < 32) { throw new System.Exception(); }
                                System.IO.File.WriteAllBytes(textBox7.Text + System.IO.Path.DirectorySeparatorChar + DataFileName, Data);
                            }
                            textBox9.Text = "SUCCESS: File '" + DataFileName + "' successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        FlashWindowEx(this);
                    }
                    break;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}