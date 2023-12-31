using System.Diagnostics;
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
        private static int FileByteAmountNeeded = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            textBox5.Text = "";
        }

        private void button1_Click(object sender, EventArgs e) //
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
            ODialog.Multiselect = true;
            if (ODialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    textBox2.Text = "";
                    var FNames = ODialog.FileNames;
                    if (FNames != null && FNames.Length > 0)
                    {
                        int n = 0; while (n < FNames.Length)
                        {
                            textBox2.Text += FNames[n] + "|";
                            n++;
                        }
                        textBox2.Text.TrimEnd('|');
                        comboBox1_SelectedIndexChanged(null, null);
                    }
                    else
                    {
                        textBox2.Text = ODialog.FileName;
                        comboBox1_SelectedIndexChanged(null, null);
                    }
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
                        if (comboBox2.Enabled && (comboBox2.SelectedIndex + 1) > 8) { comboBox1.SelectedIndex = 2; comboBox1_SelectedIndexChanged(null, null); return; }

                        if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
                        {
                            if (System.IO.File.Exists(textBox1.Text)) //Both files exist,
                            {
                                int FileSize = 0; string[] Files = textBox2.Text.Split('|', StringSplitOptions.RemoveEmptyEntries); FileByteAmountNeeded = 0;
                                int n = 0; while(n < Files.Length)
                                {
                                    int Size = (int)(new System.IO.FileInfo(Files[n]).Length);
                                    FileByteAmountNeeded += Size;
                                    FileSize += 4 + Size + (System.IO.Path.GetFileName(Files[n]).Length * 2) + 1;
                                    n++;
                                }
                                //int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, !comboBox2.Enabled ? 8 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel/bits per channel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                                else if (textBox3.Text != null && textBox3.Text != "" && System.IO.Path.GetExtension(textBox3.Text) == ".webp") { textBox3.Text = System.IO.Path.ChangeExtension(textBox3.Text, ".png"); textBox5.Text += "\r\nNOTE: WebP doesn't save 24bit(ImageSharp bug) images at this time."; DoesntFit = false; textBox5.ForeColor = Color.Violet; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, !comboBox2.Enabled ? 8 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, null);
                                textBox5.Text = "Total Available: " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";
                                return;
                            }
                        }

                        textBox5.Text = "";
                    }
                    break;
                case 1: //32bit.
                    {
                        if (comboBox2.Enabled && (comboBox2.SelectedIndex + 1) > 8) { comboBox1.SelectedIndex = 2; comboBox1_SelectedIndexChanged(null, null); return; }

                        if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "") //If Input and data is available.
                        {
                            if (System.IO.File.Exists(textBox1.Text)) //Both files exist,
                            {
                                int FileSize = 0; string[] Files = textBox2.Text.Split('|', StringSplitOptions.RemoveEmptyEntries); FileByteAmountNeeded = 0;
                                int n = 0; while (n < Files.Length)
                                {
                                    int Size = (int)(new System.IO.FileInfo(Files[n]).Length);
                                    FileByteAmountNeeded += Size;
                                    FileSize += 4 + Size + (System.IO.Path.GetFileName(Files[n]).Length * 2) + 1;
                                    n++;
                                }
                                //int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, !comboBox2.Enabled ? 8 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel/bits per channel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, !comboBox2.Enabled ? 8 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, null);
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
                            if (System.IO.File.Exists(textBox1.Text)) //Both files exist,
                            {
                                int FileSize = 0; string[] Files = textBox2.Text.Split('|', StringSplitOptions.RemoveEmptyEntries); FileByteAmountNeeded = 0;
                                int n = 0; while (n < Files.Length)
                                {
                                    int Size = (int)(new System.IO.FileInfo(Files[n]).Length);
                                    FileByteAmountNeeded += Size;
                                    FileSize += 4 + Size + (System.IO.Path.GetFileName(Files[n]).Length * 2) + 1;
                                    n++;
                                }
                                //int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, !comboBox2.Enabled ? 16 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel/bits per channel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                                else if (textBox3.Text != null && textBox3.Text != "" && System.IO.Path.GetExtension(textBox3.Text) != ".png") { textBox3.Text = System.IO.Path.ChangeExtension(textBox3.Text, ".png"); textBox5.Text += "\r\nNOTE: 48bit+ images only support the '.png' format, changing output."; DoesntFit = false; textBox5.ForeColor = Color.Violet; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, true, !comboBox2.Enabled ? 16 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, null);
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
                            if (System.IO.File.Exists(textBox1.Text)) //Both files exist,
                            {
                                int FileSize = 0; string[] Files = textBox2.Text.Split('|', StringSplitOptions.RemoveEmptyEntries); FileByteAmountNeeded = 0;
                                int n = 0; while (n < Files.Length)
                                {
                                    int Size = (int)(new System.IO.FileInfo(Files[n]).Length);
                                    FileByteAmountNeeded += Size;
                                    FileSize += 4 + Size + (System.IO.Path.GetFileName(Files[n]).Length * 2) + 1;
                                    n++;
                                }
                                // int SaveBits = Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, FileSize, System.IO.Path.GetFileName(textBox2.Text));
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, !comboBox2.Enabled ? 16 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, System.IO.Path.GetFileName(textBox2.Text));

                                textBox5.Text = "Total Available: " + (FileSize / 1024d).ToString("#.##") + " KB / " + (MaxBufferSpace / 1024d).ToString("#.##") + " KB";

                                if (FileSize > MaxBufferSpace) { textBox5.Text += "\r\nWARNING: Need a bigger image or more bits per pixel/bits per channel to store!"; DoesntFit = true; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                                else if (textBox3.Text != null && textBox3.Text != "" && System.IO.Path.GetExtension(textBox3.Text) != ".png") { textBox3.Text = System.IO.Path.ChangeExtension(textBox3.Text, ".png"); textBox5.Text += "\r\nNOTE: 48bit+ images only support the '.png' format, changing output."; DoesntFit = false; textBox5.ForeColor = Color.Violet; textBox5.BackColor = textBox5.BackColor; } else { DoesntFit = false; textBox5.ForeColor = Color.Black; textBox5.BackColor = textBox5.BackColor; }
                                return;
                            }
                        }

                        if (textBox1.Text != null && textBox1.Text != "")
                        {
                            if (System.IO.File.Exists(textBox1.Text))
                            {
                                long MaxBufferSpace = Layernt.Layernt.GetAvailableSpace(textBox1.Text, false, !comboBox2.Enabled ? 16 : (comboBox2.SelectedIndex + 1), checkBox1.Checked, null);
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
                if (System.IO.File.Exists(textBox1.Text)) //Both files exist,
                {
                    if (textBox3.Text != null && textBox3.Text != "" && textBox3.Text.IndexOfAny(Path.GetInvalidPathChars()) == -1) //Output.
                    {
                        if (checkBox1.Checked && (textBox4.Text == null || textBox4.Text == "")) { textBox5.Text += (textBox5.Text != "" ? "\r\n" : "") + "ERROR: Write a password if you're gonna use encryption!"; textBox5.ForeColor = Color.Red; textBox5.BackColor = textBox5.BackColor; }
                        else if (!DoesntFit)
                        {
                            string[] Files = textBox2.Text.Split('|', StringSplitOptions.RemoveEmptyEntries); int FileSizes = FileByteAmountNeeded + (4 * Files.Length);
                            byte[] Data = new byte[FileSizes]; string ProperFilePath = ""; int Offset = 0;
                            int n = 0; while(n < Files.Length)
                            {
                                var tmp = System.IO.File.ReadAllBytes(Files[n]);
                                ProperFilePath += System.IO.Path.GetFileName(Files[n]) + '|';
                                System.Buffer.BlockCopy(System.BitConverter.GetBytes(tmp.Length), 0, Data, Offset, 4);
                                System.Buffer.BlockCopy(tmp, 0, Data, Offset + 4, tmp.Length);
                                Offset += tmp.Length + 4;
                                n++;
                            }
                            ProperFilePath.TrimEnd('|');

                            switch (comboBox1.SelectedIndex)
                            {
                                case 0:
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = !comboBox2.Enabled ? Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, Data.Length, ProperFilePath) : (comboBox2.SelectedIndex + 1);
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage24(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, ProperFilePath);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage24(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), ProperFilePath, checkBox3.Checked);
                                        }
                                        progressBar1.Value = 100;
                                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                                    }
                                    break;
                                case 1: //32bit.
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = !comboBox2.Enabled ? Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, Data.Length, ProperFilePath) : (comboBox2.SelectedIndex + 1);
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage32(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, ProperFilePath);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage32(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), ProperFilePath, checkBox3.Checked);
                                        }
                                        progressBar1.Value = 100;
                                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                                    }
                                    break;
                                case 2:
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = !comboBox2.Enabled ? Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, true, checkBox1.Checked, Data.Length, ProperFilePath, 16) : (comboBox2.SelectedIndex + 1);
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage48(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, ProperFilePath);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage48(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), ProperFilePath, checkBox3.Checked);
                                        }
                                        progressBar1.Value = 100;
                                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                                    }
                                    break;
                                case 3:
                                    {
                                        progressBar1.Value = 0;
                                        int SaveBits = !comboBox2.Enabled ? Layernt.Layernt.GetPerfectSaveBits(textBox1.Text, false, checkBox1.Checked, Data.Length, ProperFilePath, 16) : (comboBox2.SelectedIndex + 1);
                                        if (!checkBox1.Checked)
                                        {
                                            Layernt.Layernt.SaveImage64(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, ProperFilePath);
                                        }
                                        else
                                        {
                                            Layernt.Layernt.EncryptImage64(textBox1.Text, textBox3.Text, Data, 0, Data.Length, SaveBits, System.Text.Encoding.ASCII.GetBytes(textBox4.Text), ProperFilePath, checkBox3.Checked);

                                        }
                                        progressBar1.Value = 100;
                                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
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
            ODialog.Filter = "Image Files|*.png;*.tiff;*.tga;*.webp;*.bmp;*.jpg";
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
                            string DataFileName; int FileCount = 0;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage24(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while(n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage24(textBox6.Text, out byte[] Data, out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            textBox9.Text = "SUCCESS: "+ FileCount + " Files successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                    }
                    break;
                case 32:
                    {
                        progressBar2.Value = 0; int FileCount = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage32(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage32(textBox6.Text, out byte[] Data, out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            textBox9.Text = "SUCCESS: " + FileCount + " Files successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                    }
                    break;
                case 48:
                    {
                        progressBar2.Value = 0; int FileCount = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage48(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage48(textBox6.Text, out byte[] Data, out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            textBox9.Text = "SUCCESS: " + FileCount + " Files successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                    }
                    break;
                case 64:
                    {
                        progressBar2.Value = 0; int FileCount = 0;
                        try
                        {
                            string DataFileName;
                            if (checkBox2.Checked)
                            {
                                Layernt.Layernt.DecryptImage64(textBox6.Text, out byte[] Data, System.Text.Encoding.ASCII.GetBytes(textBox8.Text), out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            else
                            {
                                Layernt.Layernt.ReadImage64(textBox6.Text, out byte[] Data, out DataFileName);
                                var Files = DataFileName.Split('|', StringSplitOptions.RemoveEmptyEntries); int n = 0, Offset = 0; FileCount = Files.Length; while (n < Files.Length)
                                {
                                    var Dir = textBox7.Text + System.IO.Path.DirectorySeparatorChar;
                                    var FileIO = System.IO.File.Open(Dir + Files[n], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                                    var DataLen = System.BitConverter.ToInt32(Data, Offset);
                                    FileIO.Write(Data, Offset + 4, DataLen); Offset += 4 + DataLen;
                                    FileIO.Flush(); FileIO.Close(); FileIO.Dispose();
                                    n++;
                                }
                            }
                            textBox9.Text = "SUCCESS: " + FileCount + " Files successfully read and stored in '" + textBox7.Text + "'.";
                        }
                        catch { textBox9.Text = "ERROR: Either the file is encrypted/corrupted or its just a normal file not containing anything."; }
                        progressBar2.Value = 100;
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) { FlashWindowEx(this); }
                    }
                    break;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = !checkBox4.Checked;
            comboBox1_SelectedIndexChanged(null, null);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(null, null);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}