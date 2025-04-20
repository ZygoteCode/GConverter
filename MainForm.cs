using System.Windows.Forms;
using MetroSuite;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.CSharp;
using System.Reflection;
using Microsoft.VisualBasic;
using LegitHttpClient;

public partial class MainForm : MetroForm
{
    public string[] audioFormats = new string[] { "mp3", "wav", "ogg", "m4a", "aac", "flac", "pcm", "aiff", "wma", "alac" };
    public string[] videoFormats = new string[] { "avi", "webm", "mpeg", "mp4", "swf", "mkv", "mov", "wmv", "flv", "avchd", "3gp", "ts" };
    public string[] imageFormats = new string[] { "jpg", "png", "bmp", "ico", "jpeg", "tif", "tiff", "jfif", "jpe", "rle", "dib", "svg", "svgz", "gif", "webp" };
   
    public static string ffmpegPrepend = "", threadCommand = "";

    public MainForm()
    {
        InitializeComponent();
        CheckForIllegalCrossThreadCalls = false;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

        string audioString = "", videoString = "", imageString = "";

        foreach (string format in audioFormats)
        {
            if (audioString == "")
            {
                audioString = format.ToUpper() + " audio (*." + format.ToLower() + ")|*." + format.ToLower();
            }
            else
            {
                audioString += "|" + format.ToUpper() + " audio (*." + format.ToLower() + ")|*." + format.ToLower();
            }
        }

        foreach (string format in videoFormats)
        {
            if (videoString == "")
            {
                videoString = format.ToUpper() + " video (*." + format.ToLower() + ")|*." + format.ToLower();
            }
            else
            {
                videoString += "|" + format.ToUpper() + " video (*." + format.ToLower() + ")|*." + format.ToLower();
            }
        }

        foreach (string format in imageFormats)
        {
            if (imageString == "")
            {
                imageString = format.ToUpper() + " image (*." + format.ToLower() + ")|*." + format.ToLower();
            }
            else
            {
                imageString += "|" + format.ToUpper() + " image (*." + format.ToLower() + ")|*." + format.ToLower();
            }
        }

        openFileDialog1.Filter = audioString;
        saveFileDialog1.Filter = audioString;

        openFileDialog2.Filter = videoString;
        saveFileDialog2.Filter = videoString;

        openFileDialog3.Filter = imageString;
        saveFileDialog3.Filter = imageString;

        openFileDialog6.Filter = imageString;
        openFileDialog7.Filter = audioString;
        saveFileDialog10.Filter = audioString;
        openFileDialog10.Filter = videoString;

        foreach (TabPage tabPage in firefoxMainTabControl1.TabPages)
        {
            foreach (Control control in tabPage.Controls)
            {
                if (control.Name.StartsWith("gunaLineTextBox"))
                {
                    Guna.UI.WinForms.GunaLineTextBox textBox = (Guna.UI.WinForms.GunaLineTextBox)control;

                    textBox.DragEnter += (s, e) =>
                    {
                        if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                    };

                    textBox.DragDrop += (s, e) =>
                    {
                        if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        {
                            textBox.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                        }
                    };
                }
                else if (control.Name.StartsWith("listBox"))
                {
                    ListBox listBox = (ListBox)control;

                    listBox.DragEnter += (s, e) =>
                    {
                        if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                    };

                    listBox.DragDrop += (s, e) =>
                    {
                        if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        {
                            foreach (string str in (string[])e.Data.GetData(DataFormats.FileDrop))
                            {
                                listBox.Items.Add(str);
                            }
                        }
                    };
                }
            }
        }

        ffmpegPrepend = " -hwaccel cuda ";
        threadCommand = " -threads " + System.Environment.ProcessorCount.ToString() + " ";
    }

    public void Reset()
    {
        openFileDialog1.FileName = "";
        saveFileDialog1.FileName = "";

        openFileDialog2.FileName = "";
        saveFileDialog2.FileName = "";

        openFileDialog3.FileName = "";
        saveFileDialog3.FileName = "";

        openFileDialog4.FileName = "";
        saveFileDialog4.FileName = "";

        openFileDialog5.FileName = "";
        saveFileDialog5.FileName = "";

        openFileDialog6.FileName = "";
        saveFileDialog6.FileName = "";

        saveFileDialog8.FileName = "";
        folderBrowserDialog1.SelectedPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        if (siticoneRadioButton6.Checked)
        {
            saveFileDialog6.Filter = "WEBM video (*.webm)|*.webm";
            saveFileDialog6.Title = "Choose where to save the downloaded YouTube video file...";
        }
        else
        {
            saveFileDialog6.Filter = "JPG image (*.jpg)|*.jpg";
            saveFileDialog6.Title = "Choose where to save the downloaded YouTube video thumbnail...";
        }

        saveFileDialog7.FileName = "";
        saveFileDialog9.FileName = "";

        openFileDialog7.FileName = "";
        saveFileDialog10.FileName = "";

        openFileDialog8.FileName = "";
        saveFileDialog11.FileName = "";

        openFileDialog9.FileName = "";
        saveFileDialog2.FileName = "";

        openFileDialog10.FileName = "";
    }

    private void gunaButton2_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox1.Text = openFileDialog1.FileName;
        }
    }

    private void gunaButton1_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox2.Text = saveFileDialog1.FileName;
        }
    }

    private void gunaButton3_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox1.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox1.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox2.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox2.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox2.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            ConvertAudio(gunaLineTextBox1.Text, gunaLineTextBox2.Text);
            MessageBox.Show("Succesfully converted the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to convert the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void ConvertAudio(string inputFile, string outputFile)
    {
        string inputExtension = Path.GetExtension(inputFile).ToLower().Replace(".", ""), outputExtension = Path.GetExtension(outputFile).ToLower().Replace(".", "");

        if (inputExtension == outputExtension)
        {
            System.IO.File.WriteAllBytes(outputFile, System.IO.File.ReadAllBytes(inputFile));
            return;
        }

        RunFFMPEG(inputFile, outputFile);
    }

    private void gunaButton6_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox4.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton5_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox3.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton4_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox4.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox4.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox3.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox3.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox3.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox4.Text).ToLower().Equals(System.IO.Path.GetExtension(gunaLineTextBox3.Text).ToLower()))
            {
                RunFFMPEG(gunaLineTextBox4.Text, gunaLineTextBox3.Text);           
            }
            else
            {
                System.IO.File.WriteAllBytes(gunaLineTextBox3.Text, System.IO.File.ReadAllBytes(gunaLineTextBox4.Text));
            }

            MessageBox.Show("Succesfully converted the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to convert the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void RunFFMPEG(string input, string output)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = ffmpegPrepend + $"-i \"{input}\" {threadCommand} \"{output}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        }).WaitForExit();
    }

    private void gunaButton15_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog3.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox10.Text = openFileDialog3.FileName;
        }
    }

    private void gunaButton14_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog3.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox9.Text = saveFileDialog3.FileName;
        }
    }

    private void gunaButton13_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox10.Text))
        {
            MessageBox.Show("The specified imported image does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in imageFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox10.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported image file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in imageFormats)
        {
            if (gunaLineTextBox9.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox9.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox9.Text);
            }
            catch
            {
                MessageBox.Show("The specified output image file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            if (siticoneRadioButton17.Checked)
            {
                RunFFMPEG(gunaLineTextBox10.Text, gunaLineTextBox9.Text);
            }
            else
            {
                string command = "";

                if (siticoneRadioButton16.Checked)
                {
                    command = $"-i \"{gunaLineTextBox10.Text}\" -vf hue=s=0 \"{gunaLineTextBox9.Text}\"";
                }
                else if (siticoneRadioButton15.Checked)
                {
                    command = $"-i \"{gunaLineTextBox10.Text}\" -vf convolution=\"-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2\" -c:a copy \"{gunaLineTextBox9.Text}\"";
                }
                else if (siticoneRadioButton14.Checked)
                {
                    command = $"-i \"{gunaLineTextBox10.Text}\" -vf hue=s=0,boxblur=lr=1.2,noise=c0s=7:allf=t,format=yuv420p \"{gunaLineTextBox9.Text}\"";
                }
                else if (siticoneRadioButton13.Checked)
                {
                    command = $"-i \"{gunaLineTextBox10.Text}\" -vf noise=c0s=14:c0f=t+u \"{gunaLineTextBox9.Text}\"";
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegPrepend + command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }

            MessageBox.Show("Succesfully converted the imported image!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to convert the imported image!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton21_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox14.Text = openFileDialog1.FileName;
        }
    }

    private void gunaButton20_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox13.Text = saveFileDialog1.FileName;
        }
    }

    private void gunaButton19_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox14.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox14.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox13.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox13.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox13.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            if (siticoneRadioButton24.Checked)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox14.Text}\" -af acrusher=1:1:1:0:log \"{gunaLineTextBox13.Text}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox14.Text}\" -af acrusher=.1:1:64:0:log \"{gunaLineTextBox13.Text}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }

            MessageBox.Show("Succesfully made the ear-rape of the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to make the ear-rape of the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton24_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox16.Text = openFileDialog1.FileName;
        }
    }

    private void gunaButton23_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox15.Text = saveFileDialog1.FileName;
        }
    }

    private void gunaButton22_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox16.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox16.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox15.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox15.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox15.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox16.Text}\" -af areverse \"{gunaLineTextBox15.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully reversed the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to reverse the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton30_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox20.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton29_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox19.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton28_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox20.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox20.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox19.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox19.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox19.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox20.Text}\" -vcodec copy -an \"{gunaLineTextBox19.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully removed the audio track from the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to remove the audio track from the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void metroTrackbar2_Scroll(object sender, MetroTrackbar.TrackbarEventArgs e)
    {
        metroLabel31.Text = (metroTrackbar2.Value * 0.03).ToString().Replace(",", ".");
    }

    private void gunaButton46_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox26.Text = openFileDialog1.FileName;
        }
    }

    private void gunaButton45_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox25.Text = saveFileDialog1.FileName;
        }
    }

    private void gunaButton44_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox26.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox26.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox25.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox25.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox25.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox26.Text}\" -af asetrate=44100*{((metroTrackbar2.Value * 0.03)).ToString().Replace(",", ".")},aresample=44100,atempo=1/0.9 \"{gunaLineTextBox25.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created the Nightcore from the imported audio file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the Nightcore from the imported audio file", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton48_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog6.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox27.Text = saveFileDialog6.FileName;
        }
    }

    private void gunaButton47_Click(object sender, System.EventArgs e)
    {
        string videoId = "", videoUrl = "";

        try
        {
            string[] splitted = Strings.Split(gunaLineTextBox28.Text, "/watch?v=");
            videoId = splitted[1];

            if (videoId.Length != 11)
            {
                videoId = videoId.Substring(0, 11);
            }

            videoUrl = "https://www.youtube.com/watch?v=" + videoId;
        }
        catch
        {
            videoId = gunaLineTextBox28.Text;
            videoUrl = "https://www.youtube.com/watch?v=" + videoId;
        }

        if (siticoneRadioButton6.Checked)
        {
            if (!gunaLineTextBox27.Text.ToLower().EndsWith(".webm"))
            {
                MessageBox.Show("The output file can be only a WEBM file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (System.IO.File.Exists(gunaLineTextBox27.Text))
            {
                try
                {
                    System.IO.File.Delete(gunaLineTextBox27.Text);
                }
                catch
                {
                    MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "youtube-dl.exe",
                    Arguments = $"{videoUrl} -o \"{gunaLineTextBox27.Text}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();

                MessageBox.Show("Succesfully downloaded the YouTube video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Failed to dowmload the YouTube video.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            if (!gunaLineTextBox27.Text.ToLower().EndsWith(".jpg"))
            {
                MessageBox.Show("The output file can be only a JPG file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (System.IO.File.Exists(gunaLineTextBox27.Text))
            {
                try
                {
                    System.IO.File.Delete(gunaLineTextBox27.Text);
                }
                catch
                {
                    MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                System.IO.File.WriteAllBytes(gunaLineTextBox27.Text, new WebClient().DownloadData("https://img.youtube.com/vi/" + videoId + "/0.jpg"));
                MessageBox.Show("Succesfully downloaded the YouTube video thumbnail!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Failed to dowmload the YouTube video thumbnail.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void gunaButton51_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox30.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton50_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox29.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton49_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox30.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox30.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox29.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox29.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox29.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox30.Text}\" -vn -c:a copy \"{gunaLineTextBox29.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully removed the video track from the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to remove the video track from the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton54_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox32.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton53_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox31.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton52_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox32.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox32.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox31.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox31.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox31.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            if (siticoneRadioButton25.Checked)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox32.Text}\" -af acrusher=1:1:1:0:log \"{gunaLineTextBox31.Text}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox32.Text}\" -af acrusher=.1:1:64:0:log \"{gunaLineTextBox31.Text}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }

            MessageBox.Show("Succesfully made the ear-rape of the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to make the ear-rape of the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton10_Click_1(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox8.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox8.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox7.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox7.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox7.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            RunFFMPEG(gunaLineTextBox8.Text, gunaLineTextBox7.Text);
            MessageBox.Show("Succesfully converted the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to convert the imported audio!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton12_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox8.Text = openFileDialog1.FileName;
        }
    }

    private void gunaButton11_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox7.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton9_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox6.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton8_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox5.Text = saveFileDialog1.FileName;
        }
    }

    private void gunaButton7_Click_1(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox6.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox6.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox5.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox5.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox5.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            RunFFMPEG(gunaLineTextBox6.Text, gunaLineTextBox5.Text);
            MessageBox.Show("Succesfully converted the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to convert the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton27_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox18.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton26_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox17.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton25_Click_1(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox18.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox18.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox17.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox17.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox17.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            string theArg = "";

            if (siticoneRadioButton1.Checked)
            {
                theArg = $"-i \"{gunaLineTextBox18.Text}\" -af areverse \"{gunaLineTextBox17.Text}\"";
            }
            else if (siticoneRadioButton2.Checked)
            {
                theArg = $"-i \"{gunaLineTextBox18.Text}\" -vf reverse \"{gunaLineTextBox17.Text}\"";
            }
            else
            {
                theArg = $"-i \"{gunaLineTextBox18.Text}\" -vf reverse -af areverse \"{gunaLineTextBox17.Text}\"";
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = theArg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully reversed the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to reverse the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton18_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog4.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox12.Text = openFileDialog4.FileName;
        }
    }

    private void gunaButton17_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog4.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox11.Text = saveFileDialog4.FileName;
        }
    }

    private void gunaButton16_Click_1(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox12.Text))
        {
            MessageBox.Show("The specified imported image does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!System.IO.Path.GetExtension(gunaLineTextBox12.Text).ToLower().Equals(".png"))
        {
            MessageBox.Show("The specified imported image file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!gunaLineTextBox11.Text.ToLower().EndsWith(".png"))
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox11.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox11.Text);
            }
            catch
            {
                MessageBox.Show("The specified output image file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "pngquant.exe",
                Arguments = $"\"{gunaLineTextBox12.Text}\" --force --verbose --ordered --speed=1 --quality=50-90 %1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            System.IO.File.Move(gunaLineTextBox12.Text.Replace(".png", "-or8.png"), gunaLineTextBox11.Text);
            MessageBox.Show("Succesfully optimized the imported image!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to ptimize the imported image!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }

    public string GetYouTube(string url)
    {
        url = url.Substring("https://www.youtube.com".Length) + "?cbrd=1";

        HttpClient client = new HttpClient();
        client.ConnectTo("youtube.com", true, 443);
        client.ByteRate = System.Int16.MaxValue;

        HttpRequest request = new HttpRequest();
        request.Method = HttpMethod.GET;
        request.Version = LegitHttpClient.HttpVersion.HTTP_11;
        request.URI = url;

        request.Headers.Add(new HttpHeader() { Name = "Host", Value = "www.youtube.com" });
        request.Headers.Add(new HttpHeader() { Name = "Connection", Value = "keep-alive" });
        request.Headers.Add(new HttpHeader() { Name = "Cache-Control", Value = "max-age=0" });
        request.Headers.Add(new HttpHeader() { Name = "Upgrade-Insecure-Requests", Value = "1" });
        request.Headers.Add(new HttpHeader() { Name = "User-Agent", Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36" });
        request.Headers.Add(new HttpHeader() { Name = "Accept", Value = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" });
        request.Headers.Add(new HttpHeader() { Name = "X-Client-Data", Value = "CJO2yQEIorbJAQjEtskBCKmdygEIr+zKAQiWocsBCJ75ywEI5oTMAQiljswBCJmazAEI0KLMAQiDp8wBCNupzAEYq6nKAQ==" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-Site", Value = "same-site" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-Mode", Value = "navigate" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-User", Value = "?1" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-Dest", Value = "document" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua", Value = "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"100\", \"Google Chrome\";v=\"100\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-mobile", Value = "?0" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-full-version", Value = "\"100.0.4896.127\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-arch", Value = "\"x86\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-platform", Value = "\"Windows\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-platform-version", Value = "\"10.0.0\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-model", Value = "\"\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-bitness", Value = "\"64\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-full-version-list", Value = "\" Not A;Brand\";v=\"99.0.0.0\", \"Chromium\";v=\"100.0.4896.127\", \"Google Chrome\";v=\"100.0.4896.127\"" });
        request.Headers.Add(new HttpHeader() { Name = "Accept-Language", Value = "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7" });
        request.Headers.Add(new HttpHeader() { Name = "Cookie", Value = "YSC=SdAv2dazATs; __Secure-YEC=Cgs0czNzSXlGbHZaRSi8o6qTBg%3D%3D; CONSENT=YES+cb.20220424-17-p0.it+FX+023; SOCS=CAISNQgDEitib3FfaWRlbnRpdHlmcm9udGVuZHVpc2VydmVyXzIwMjIwNDI0LjE3X3AwGgJpdCACGgYIgLKnkwY" });

        string body = System.Text.Encoding.UTF8.GetString(client.Send(request).Body);
        client.Disconnect();

        return body;
    }

    public string GetInstagram(string url)
    {
        url = url.Substring("https://www.instagram.com".Length);

        HttpClient client = new HttpClient();
        client.ConnectTo("instagram.com", true, 443, "http://127.0.0.1:8888");
        client.ByteRate = System.Int16.MaxValue;

        HttpRequest request = new HttpRequest();
        request.Method = HttpMethod.GET;
        request.Version = LegitHttpClient.HttpVersion.HTTP_11;
        request.URI = url;

        request.Headers.Add(new HttpHeader() { Name = "Host", Value = "www.instagram.com" });
        request.Headers.Add(new HttpHeader() { Name = "Connection", Value = "keep-alive" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua", Value = "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"100\", \"Google Chrome\";v=\"100\"" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-mobile", Value = "?0" });
        request.Headers.Add(new HttpHeader() { Name = "sec-ch-ua-platform", Value = "\"Windows\"" });
        request.Headers.Add(new HttpHeader() { Name = "Upgrade-Insecure-Requests", Value = "1" });
        request.Headers.Add(new HttpHeader() { Name = "User-Agent", Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36" });
        request.Headers.Add(new HttpHeader() { Name = "Accept", Value = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-Site", Value = "none" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-Mode", Value = "navigate" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-User", Value = "?1" });
        request.Headers.Add(new HttpHeader() { Name = "Sec-Fetch-Dest", Value = "document" });
        request.Headers.Add(new HttpHeader() { Name = "Accept-Language", Value = "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7" });
        request.Headers.Add(new HttpHeader() { Name = "Cookie", Value = "mid=YmqT7AALAAGlQOSsUyl4G5XuQGfq; ig_did=4405AA8E-75F9-4910-8D70-18315806A038; csrftoken=lEgMP2uLZwQIaJIq4V4eGfYCBSZflihO; ds_user_id=45490536531; sessionid=45490536531%3A6g9hkQUrhxupjS%3A27; shbid=\"17280\\05445490536531\\0541682687870:01f7b1f2676bba891b918c8c100768b41ea67c37b02829dea848d0ef8b85ea5488f19d42\"; shbts=\"1651151870\\05445490536531\\0541682687870:01f7ffb3979b7349c5dc957acfd5a13b2df332f02c1e3c0fc2bf4c75f1bc4da523b8c734\"; rur=\"ODN\\05445490536531\\0541682687880:01f73c0fb8059e6b583074932b69c5743dcf652c70af61efafbb0086a726e07d4fa418d7\"" });

        string body = FixResultString(System.Text.Encoding.UTF8.GetString(client.Send(request).Body));
        client.Disconnect();

        return body;
    }

    public static string FixResultString(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '{')
            {
                str = str.Substring(i);
                break;
            }
        }

        int steps = 0;

        for (int i = str.Length - 1; i >= 0; i--)
        {
            if (str[i] == '}')
            {
                str = str.Substring(0, str.Length - steps);
                break;
            }

            steps++;
        }

        return str;
    }

    private void gunaButton61_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog8.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox37.Text = saveFileDialog8.FileName;
        }
    }

    private void gunaButton60_Click(object sender, System.EventArgs e)
    {
        if (System.IO.File.Exists(gunaLineTextBox37.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox37.Text);
            }
            catch
            {
                MessageBox.Show("The specified output file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (!gunaLineTextBox37.Text.ToLower().EndsWith(".jpg"))
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            string url = gunaLineTextBox38.Text;

            if (!url.Contains("instagram") && !url.Contains("http"))
            {
                url = "https://www.instagram.com/" + url;
            }

            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            url += "?__a=1";

            WebClient client = new WebClient();
            string src = GetInstagram(url);
            dynamic jss = JObject.Parse(src);
            string contentUrl = jss.graphql.user.profile_pic_url_hd;
            System.IO.File.WriteAllBytes(gunaLineTextBox37.Text, client.DownloadData(contentUrl));
            MessageBox.Show("Succesfully downloaded this Instagram profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        catch
        {
            MessageBox.Show("Failed to download this Instagram profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }

    private void gunaButton59_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (folderBrowserDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox35.Text = folderBrowserDialog1.SelectedPath;
        }
    }

    private void gunaButton58_Click(object sender, System.EventArgs e)
    {
        string path = gunaLineTextBox35.Text;

        if (!System.IO.Directory.Exists(path))
        {
            MessageBox.Show("The output directory does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!path.EndsWith("\\"))
        {
            path += "\\";
        }

        path += "OutputInstagram";

        if (System.IO.Directory.Exists(path))
        {
            try
            {
                System.IO.Directory.Delete(path, true);
            }
            catch
            {
                MessageBox.Show("The output directory already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        System.IO.Directory.CreateDirectory(path);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "instaloader.exe",
                Arguments = ffmpegPrepend + $"{gunaLineTextBox36.Text}",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            if (!System.IO.Directory.Exists(Application.StartupPath + "\\" + gunaLineTextBox36.Text))
            {
                MessageBox.Show("Failed to download all posts from this Instagram profile.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            System.IO.Directory.Move(Application.StartupPath + "\\" + gunaLineTextBox36.Text, path + "\\" + gunaLineTextBox36.Text);

            MessageBox.Show("Succesfully downloaded all posts from this Instagram profile!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to download all posts from this Instagram profile.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton63_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (folderBrowserDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox39.Text = folderBrowserDialog1.SelectedPath;
        }
    }

    private void gunaButton62_Click(object sender, System.EventArgs e)
    {
        string path = gunaLineTextBox39.Text;

        if (!System.IO.Directory.Exists(path))
        {
            MessageBox.Show("The output directory does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!path.EndsWith("\\"))
        {
            path += "\\";
        }

        path += "OutputInstagram";

        if (System.IO.Directory.Exists(path))
        {
            try
            {
                System.IO.Directory.Delete(path, true);
            }
            catch
            {
                MessageBox.Show("The output directory already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        System.IO.Directory.CreateDirectory(path);

        try
        {
            string post = gunaLineTextBox40.Text;

            if (post.StartsWith("http://"))
            {
                post = post.Substring(("http://".Length), post.Length - ("http://".Length));
            }

            if (post.StartsWith("https://"))
            {
                post = post.Substring(("https://".Length), post.Length - ("https://".Length));
            }

            if (post.StartsWith("www.instagram.com"))
            {
                post = post.Substring(("www.instagram.com".Length), post.Length - ("www.instagram.com".Length));
            }

            if (post.StartsWith("instagram.com"))
            {
                post = post.Substring(("instagram.com".Length), post.Length - ("instagram.com".Length));
            }

            if (post.StartsWith("/p/"))
            {
                post = post.Substring(("/p/".Length), post.Length - ("/p/".Length));
            }

            if (post.StartsWith("p/"))
            {
                post = post.Substring(("p/".Length), post.Length - ("p/".Length));
            }

            if (post.StartsWith("/"))
            {
                post = post.Substring(("/".Length), post.Length - ("/".Length));
            }

            post = post.Replace(" ", "").Trim().Replace('\t'.ToString(), "").Replace("/", "");

            Process.Start(new ProcessStartInfo
            {
                FileName = "instaloader.exe",
                Arguments = ffmpegPrepend + $"-- -{post}",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            if (!System.IO.Directory.Exists(Application.StartupPath + "\\-" + post))
            {
                MessageBox.Show("Failed to download this Instagram post.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            System.IO.Directory.Move(Application.StartupPath + "\\-" + post, path + "\\-" + post);
            MessageBox.Show("Succesfully downloaded this Instagram post!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to download this Instagram post.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton65_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog7.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox41.Text = saveFileDialog7.FileName;
        }
    }

    private void gunaButton64_Click(object sender, System.EventArgs e)
    {
        if (System.IO.File.Exists(gunaLineTextBox41.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox41.Text);
            }
            catch
            {
                MessageBox.Show("The specified output file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (!gunaLineTextBox41.Text.ToLower().EndsWith(".jpg"))
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            string url = gunaLineTextBox42.Text;
            WebClient client = new WebClient();
            string src = GetYouTube(url);

            if (src.Contains("https://yt3.ggpht.com/ytc/"))
            {
                string[] splitted = Strings.Split(src, "\"https://yt3.ggpht.com/ytc/");
                splitted = Strings.Split(splitted[1], "\"");
                string contentUrl = splitted[0];

                if (siticoneRadioButton7.Checked)
                {
                    contentUrl = contentUrl.Replace("=s48", "=s88").Replace("=s68", "=s88");
                }
                else if (siticoneRadioButton4.Checked)
                {
                    contentUrl = contentUrl.Replace("=s48", "=s240").Replace("=s68", "=s240");
                }
                else
                {
                    contentUrl = contentUrl.Replace("=s48", "=s800").Replace("=s68", "=s800");
                }

                contentUrl = "https://yt3.ggpht.com/ytc/" + contentUrl;
                System.IO.File.WriteAllBytes(gunaLineTextBox41.Text, new WebClient().DownloadData(contentUrl));
                MessageBox.Show("Succesfully downloaded this YouTube channel profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string[] splitted = Strings.Split(src, "\"https://yt3.ggpht.com/");
                splitted = Strings.Split(splitted[1], "\"");
                string contentUrl = splitted[0];

                if (siticoneRadioButton7.Checked)
                {
                    contentUrl = contentUrl.Replace("=s48", "=s88").Replace("=s68", "=s88");
                }
                else if (siticoneRadioButton4.Checked)
                {
                    contentUrl = contentUrl.Replace("=s48", "=s240").Replace("=s68", "=s240");
                }
                else
                {
                    contentUrl = contentUrl.Replace("=s48", "=s800").Replace("=s68", "=s800");
                }

                contentUrl = "https://yt3.ggpht.com/" + contentUrl;
                System.IO.File.WriteAllBytes(gunaLineTextBox41.Text, new WebClient().DownloadData(contentUrl));
                MessageBox.Show("Succesfully downloaded this YouTube channel profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch
        {
            MessageBox.Show("Failed to download this YouTube channel profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton67_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog9.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox43.Text = saveFileDialog9.FileName;
        }
    }

    private void gunaButton66_Click(object sender, System.EventArgs e)
    {
        if (System.IO.File.Exists(gunaLineTextBox43.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox43.Text);
            }
            catch
            {
                MessageBox.Show("The specified output file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (!gunaLineTextBox43.Text.ToLower().EndsWith(".jpg"))
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            string url = gunaLineTextBox44.Text;
            WebClient client = new WebClient();
            string src = GetYouTube(url);
            string[] splitted = Strings.Split(src, "\",\"width\":1920,\"height\":1080");
            string another = splitted[0];
            splitted = Strings.Split(another, "{\"url\":\"");
            string other = splitted[splitted.Length - 1];
            System.IO.File.WriteAllBytes(gunaLineTextBox43.Text, new WebClient().DownloadData(other));
            MessageBox.Show("Succesfully downloaded this YouTube channel profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        catch
        {
            MessageBox.Show("Failed to download this YouTube channel profile picture!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }

    private void gunaButton72_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog7.ShowDialog().Equals(DialogResult.OK))
        {
            foreach (string str in openFileDialog7.FileNames)
            {
                listBox3.Items.Add(str);
            }
        }

        metroLabel63.Text = listBox3.Items.Count.ToString();
    }

    private void gunaButton71_Click(object sender, System.EventArgs e)
    {
        try
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(listBox3);
            selectedItems = listBox3.SelectedItems;

            if (listBox3.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    listBox3.Items.Remove(selectedItems[i]);
                }
            }

            metroLabel63.Text = listBox3.Items.Count.ToString();
        }
        catch
        {

        }
    }

    private void gunaButton70_Click(object sender, System.EventArgs e)
    {
        listBox3.Items.Clear();
        metroLabel63.Text = "0";
    }

    private void gunaButton69_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog10.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox45.Text = saveFileDialog10.FileName;
        }
    }

    private void gunaButton68_Click(object sender, System.EventArgs e)
    {
        List<string> audioFiles = new List<string>();

        foreach (string str in listBox3.Items)
        {
            if (System.IO.File.Exists(str))
            {
                bool valid = false;

                foreach (string format in audioFormats)
                {
                    if (System.IO.Path.GetExtension(str).ToLower().Equals("." + format))
                    {
                        valid = true;
                        break;
                    }
                }

                if (valid)
                {
                    audioFiles.Add(str);
                }
            }
        }

        if (audioFiles.Count == 0)
        {
            MessageBox.Show("Please, insert at least one valid audio file in the list.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox45.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox45.Text);
            }
            catch
            {
                MessageBox.Show("The output file already exists but cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox45.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
                break;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            string audioString = "";

            foreach (string audioFile in audioFiles)
            {
                if (audioString == "")
                {
                    audioString = "-i \"" + audioFile + "\"";
                }
                else
                {
                    audioString += " -i \"" + audioFile + "\"";
                }
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"{audioString} {threadCommand} -filter_complex amix=inputs={listBox3.Items.Count.ToString()}:duration=longest \"{gunaLineTextBox45.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully merged all input audio files into a new one audio file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to merge all input audio files into a new one audio file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton73_Click(object sender, System.EventArgs e)
    {
        List<string> audioFiles = new List<string>();

        foreach (string str in listBox3.Items)
        {
            if (System.IO.File.Exists(str))
            {
                bool valid = false;

                foreach (string format in audioFormats)
                {
                    if (System.IO.Path.GetExtension(str).ToLower().Equals("." + format))
                    {
                        valid = true;
                        break;
                    }
                }

                if (valid)
                {
                    audioFiles.Add(str);
                }
            }
        }

        if (audioFiles.Count == 0)
        {
            MessageBox.Show("Please, insert at least one valid image file in the list.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox45.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox45.Text);
            }
            catch
            {
                MessageBox.Show("The output file already exists but cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox45.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
                break;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            string audioString = "";

            foreach (string audioFile in audioFiles)
            {
                if (audioString == "")
                {
                    audioString = "\"" + audioFile + "\"";
                }
                else
                {
                    audioString += "|\"" + audioFile + "\"";
                }
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i {threadCommand} \"concat:{audioString}\" -acodec copy \"{gunaLineTextBox45.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully merged all input audio files into a new one audio file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to merge all input audio files into a new one audio file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton76_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox47.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton75_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox46.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton74_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox47.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox47.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox46.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox46.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox46.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i {threadCommand} \"{gunaLineTextBox47.Text}\" -filter:v \"setpts=PTS/2\" -af asetrate=80000,aresample=44100,atempo=1/0.9 \"{gunaLineTextBox46.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created the Nightcore from the imported video file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the Nightcore from the imported video file", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton82_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox51.Text = openFileDialog1.FileName;
        }
    }

    private void gunaButton81_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox50.Text = saveFileDialog1.FileName;
        }
    }

    private void gunaButton79_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox49.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton78_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox48.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton80_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox51.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox51.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox50.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox50.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox50.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            string command = $"-i \"{gunaLineTextBox51.Text}\" {threadCommand} -ar 44100 -b:a 1k " + (siticoneCheckBox8.Checked ? " -af \"highpass=f=800\"" : "") + $" \"{gunaLineTextBox50.Text}\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created an audio file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the audio file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton77_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox49.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox49.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox48.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox48.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox48.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            string command = "";

            if (siticoneCheckBox6.Checked && siticoneCheckBox7.Checked)
            {
                command = $"-i \"{gunaLineTextBox49.Text}\" {threadCommand} -y -f flv -ar 44100 -ab 64 -ac 1 -acodec mp3 -b:a 1k -b:v 1k " + (siticoneCheckBox9.Checked ? " -af \"highpass=f=800\"" : "") + $" \"{gunaLineTextBox48.Text}\"";
            }
            else if (siticoneCheckBox6.Checked)
            {
                command = $"-i \"{gunaLineTextBox49.Text}\" {threadCommand} -ar 44100 -ab 64 -acodec mp3 -b:a 1k " + (siticoneCheckBox9.Checked ? " -af \"highpass=f=800\"" : "") + $" \"{gunaLineTextBox48.Text}\"";
            }
            else if (siticoneCheckBox7.Checked)
            {
                command = $"-i \"{gunaLineTextBox49.Text}\" {threadCommand} -y -f flv -ac 1 -acodec mp3 -b:v 1k \"{gunaLineTextBox48.Text}\"";
            }
            else if (siticoneCheckBox9.Checked)
            {
                command = $"-i \"{gunaLineTextBox49.Text}\"{threadCommand}  -af \"highpass=f=800\" \"{gunaLineTextBox48.Text}\"";
            }
            else
            {
                command = $"-i \"{gunaLineTextBox49.Text}\" \"{gunaLineTextBox48.Text}\"";
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created a video file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the video file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton85_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox53.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton84_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox52.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton83_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox53.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox53.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox52.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox52.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox52.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            string command = $"-i \"{gunaLineTextBox53.Text}\" -i watermark.png {threadCommand}-filter_complex \"overlay=1:1\" \"{gunaLineTextBox52.Text}\"";

            if (siticoneCheckBox10.Checked)
            {
                command = $"-i \"{gunaLineTextBox53.Text}\" -i watermark1.png {threadCommand}-filter_complex \"overlay=main_w/2-overlay_w/2\" \"{gunaLineTextBox52.Text}\"";
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created the WGF video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the WGF video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton86_Click(object sender, System.EventArgs e)
    {

    }

    private void gunaButton88_Click(object sender, System.EventArgs e)
    {

    }

    private void gunaButton87_Click(object sender, System.EventArgs e)
    {

    }

    private void gunaButton89_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox57.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox57.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox56.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox56.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox56.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox57.Text}\" -i watermark2.png {threadCommand}-filter_complex \"overlay=main_w/2-overlay_w/2\" \"{gunaLineTextBox56.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created the Me Contro Te video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the Me Contro Te video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton91_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox57.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton90_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox56.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton92_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox59.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox59.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox58.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox58.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox58.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox59.Text}\" -i watermark3.png {threadCommand}-filter_complex \"overlay=main_w/2-overlay_w/2\" \"{gunaLineTextBox58.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created the Peppa Pig video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the Peppa Pig video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton94_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox59.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton93_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox58.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton88_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog8.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox55.Text = openFileDialog8.FileName;
        }
    }

    private void gunaButton87_Click_1(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog11.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox54.Text = saveFileDialog11.FileName;
        }
    }

    private void gunaButton86_Click_1(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox55.Text))
        {
            MessageBox.Show("The specified imported multi media file does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox55.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox55.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        foreach (string format in imageFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox55.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported multi media file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox54.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox54.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        foreach (string format in imageFormats)
        {
            if (gunaLineTextBox54.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox54.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox54.Text);
            }
            catch
            {
                MessageBox.Show("The specified output multi media file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (!System.IO.Path.GetExtension(gunaLineTextBox55.Text).ToLower().Equals(System.IO.Path.GetExtension(gunaLineTextBox54.Text).ToLower()))
        {
            MessageBox.Show("The extension of the input file is not identical to the extension of the output file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            RunFFMPEG(gunaLineTextBox55.Text, gunaLineTextBox54.Text);
            MessageBox.Show("Succesfully fixed the imported multi media file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to fix the imported multi media file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton97_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog9.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox61.Text = openFileDialog9.FileName;
        }
    }

    private void gunaButton96_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog12.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox60.Text = saveFileDialog12.FileName;
        }
    }

    private void gunaButton95_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox61.Text))
        {
            MessageBox.Show("The specified imported audio does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in audioFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox61.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported audio file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in audioFormats)
        {
            if (gunaLineTextBox60.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox60.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox60.Text);
            }
            catch
            {
                MessageBox.Show("The specified output audio file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        string command = "";

        if (metroLabel80.Text == "0.00" && metroLabel84.Text == "0.00" && metroLabel86.Text == "0.00" && metroLabel80.Text == "0" && metroLabel84.Text == "0" && metroLabel86.Text == "0")
        {
            command = "-f 1.0";
        }
        else
        {
            if (metroLabel80.Text != "0.00" && metroLabel80.Text != "0")
            {
                string speed = metroLabel80.Text;
                
                if (metroLabel80.Text == "3.00" || metroLabel80.Text == "3")
                {
                    speed = "0.05";
                }
                else
                {
                    decimal theSpeed = decimal.Parse(speed.Replace(".", ","));
                    theSpeed = 3.0M - theSpeed;
                    speed = theSpeed.ToString().Replace(",", ".");
                }

                if (command == "")
                {
                    command = "-t " + speed;
                }
                else
                {
                    command += " -t " + speed;
                }
            }

            if (metroLabel84.Text != "0.00" && metroLabel84.Text != "0")
            {
                if (command == "")
                {
                    command = "-p " + metroLabel84.Text;
                }
                else
                {
                    command += " -p " + metroLabel84.Text;
                }
            }

            if (metroLabel86.Text != "0.00" && metroLabel86.Text != "0")
            {
                if (command == "")
                {
                    command = "-f " + metroLabel86.Text;
                }
                else
                {
                    command += " -f " + metroLabel86.Text;
                }
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "rubberband.exe",
                Arguments = $"{command} \"{gunaLineTextBox61.Text}\" \"{gunaLineTextBox60.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully modified the audio dynamics!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to modify the audio dynamics!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void metroTrackbar8_Scroll(object sender, MetroTrackbar.TrackbarEventArgs e)
    {
        metroLabel80.Text = (metroTrackbar8.Value * 0.03).ToString().Replace(",", ".");
    }

    private void metroTrackbar9_Scroll(object sender, MetroTrackbar.TrackbarEventArgs e)
    {
        metroLabel84.Text = ((metroTrackbar9.Value - 50) * 0.5).ToString().Replace(",", ".");
    }

    private void metroTrackbar10_Scroll(object sender, MetroTrackbar.TrackbarEventArgs e)
    {
        metroLabel86.Text = (metroTrackbar10.Value * 0.06).ToString().Replace(",", ".");
    }

    private void gunaButton100_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox63.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton99_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox62.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton98_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox63.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox63.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox62.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox62.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox62.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            string command = "";

            if (siticoneRadioButton11.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf hue=s=0 \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton10.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf convolution=\"-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2\" -c:a copy \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton9.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf hue=s=0,boxblur=lr=1.2,noise=c0s=7:allf=t,format=yuv420p \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton12.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf noise=c0s=14:c0f=t+u \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton20.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf eq=contrast=-1,colorchannelmixer=.3:.4:.3:0:.3:.4:.3:0:.3:.4:.3 -pix_fmt yuv420p \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton21.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf negate -c:a copy \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton22.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-vf \"boxblur=10\" -c:a copy \"{gunaLineTextBox62.Text}\"";
            }
            else if (siticoneRadioButton23.Checked)
            {
                command = $"-i \"{gunaLineTextBox63.Text}\" {threadCommand}-lavfi \"[0:v]scale=19202:10802,boxblur=luma_radius=min(h,w)/20:luma_power=1:chroma_radius=min(cw,ch)/20:chroma_power=1[bg];[0:v]scale=-1:1080[ov];[bg][ov]overlay=(W-w)/2:(H-h)/2,crop=w=1920:h=1080\" \"{gunaLineTextBox62.Text}\"";
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created a video file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the video file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void metroTrackbar1_Scroll(object sender, MetroTrackbar.TrackbarEventArgs e)
    {
        metroLabel23.Text = (metroTrackbar1.Value * 2).ToString();
    }

    private void gunaButton33_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox22.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton32_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox21.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton31_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox22.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox22.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox21.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox21.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox21.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $"-i \"{gunaLineTextBox22.Text}\" {threadCommand}-filter:v fps={(metroTrackbar1.Value * 2).ToString()} \"{gunaLineTextBox21.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully created a video file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to create the video file with decreased quality!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton39_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog10.ShowDialog().Equals(DialogResult.OK))
        {
            foreach (string str in openFileDialog10.FileNames)
            {
                listBox1.Items.Add(str);
            }
        }

        metroLabel25.Text = listBox1.Items.Count.ToString();
    }

    private void gunaButton38_Click(object sender, System.EventArgs e)
    {
        try
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(listBox1);
            selectedItems = listBox1.SelectedItems;

            if (listBox1.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    listBox1.Items.Remove(selectedItems[i]);
                }
            }

            metroLabel63.Text = listBox1.Items.Count.ToString();
        }
        catch
        {

        }
    }

    private void gunaButton37_Click(object sender, System.EventArgs e)
    {
        listBox1.Items.Clear();
        metroLabel25.Text = "0";
    }

    private void gunaButton36_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox23.Text = saveFileDialog2.FileName;
        }
    }

    public bool CheckVideoUtilities()
    {
        List<string> videoFiles = new List<string>();

        foreach (string str in listBox1.Items)
        {
            if (System.IO.File.Exists(str))
            {
                bool valid = false;

                foreach (string format in videoFormats)
                {
                    if (System.IO.Path.GetExtension(str).ToLower().Equals("." + format))
                    {
                        valid = true;
                        break;
                    }
                }

                if (valid)
                {
                    videoFiles.Add(str);
                }
            }
        }

        if (videoFiles.Count == 0)
        {
            MessageBox.Show("Please, insert at least one valid video file in the list.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (System.IO.File.Exists(gunaLineTextBox45.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox45.Text);
            }
            catch
            {
                MessageBox.Show("The output file already exists but cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox23.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
                break;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (System.IO.File.Exists(gunaLineTextBox23.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox23.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        return true;
    }

    private void gunaButton35_Click(object sender, System.EventArgs e)
    {
        if (!CheckVideoUtilities())
        {
            return;
        }

        string inputs = "", videos = "", audios = "";

        for (int i = 0; i < listBox1.Items.Count; i++)
        {
            if (inputs == "")
            {
                inputs = "-i \"" + listBox1.Items[i].ToString() + "\"";
            }
            else
            {
                inputs += " -i \"" + listBox1.Items[i].ToString() + "\"";
            }

            videos += "[" + i.ToString() + ":v]";
            
            if (i <= 1)
            {
                audios += "[" + i.ToString() + ":a]";
            }
        }

        string command = inputs + " " + threadCommand + " -filter_complex \"" + videos + "hstack=inputs=" + listBox1.Items.Count.ToString() + "[v];" + audios + "amerge[a]\" -map \"[v]\" -map \"[a]\" -ac 2 " + gunaLineTextBox23.Text;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully concatenated all videos into a unique one new video file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to concatenate all videos into a unique one new video file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton42_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (openFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox33.Text = openFileDialog2.FileName;
        }
    }

    private void gunaButton41_Click(object sender, System.EventArgs e)
    {
        Reset();

        if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
        {
            gunaLineTextBox24.Text = saveFileDialog2.FileName;
        }
    }

    private void gunaButton40_Click(object sender, System.EventArgs e)
    {
        if (!System.IO.File.Exists(gunaLineTextBox33.Text))
        {
            MessageBox.Show("The specified imported video does not exist!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool inputValid = false;

        foreach (string format in videoFormats)
        {
            if (System.IO.Path.GetExtension(gunaLineTextBox33.Text).ToLower().Equals("." + format))
            {
                inputValid = true;
            }
        }

        if (!inputValid)
        {
            MessageBox.Show("The specified imported video file has no supported extension!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool outputValid = false;

        foreach (string format in videoFormats)
        {
            if (gunaLineTextBox24.Text.ToLower().EndsWith("." + format))
            {
                outputValid = true;
            }
        }

        if (!outputValid)
        {
            MessageBox.Show("The extension of the output file is not supported!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (System.IO.File.Exists(gunaLineTextBox24.Text))
        {
            try
            {
                System.IO.File.Delete(gunaLineTextBox24.Text);
            }
            catch
            {
                MessageBox.Show("The specified output video file already exists and cannot be deleted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + $" -i \"{gunaLineTextBox33.Text}\" {threadCommand} -c:v libx265 -vtag hvc1 \"{gunaLineTextBox24.Text}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully optimized and compressed the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to optimize and compress the imported video!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void gunaButton34_Click(object sender, System.EventArgs e)
    {
        if (!CheckVideoUtilities())
        {
            return;
        }

        string inputs = "", filters = "";

        for (int i = 0; i < listBox1.Items.Count; i++)
        {
            if (inputs == "")
            {
                inputs = "-i \"" + listBox1.Items[i].ToString() + "\"";
            }
            else
            {
                inputs += " -i \"" + listBox1.Items[i].ToString() + "\"";
            }

            if (filters == "")
            {
                filters = "[" + i.ToString() + ":v] [" + i.ToString() + ":a]";
            }
            else
            {
                filters += " [" + i.ToString() + ":v] [" + i.ToString() + ":a]";
            }
        }

        string command = inputs + " " + threadCommand + " -filter_complex \"" + filters + " concat=n=" + listBox1.Items.Count.ToString() + ":v=1:a=1 [v] [a]\" -map \"[v]\" -map \"[a]\" " + gunaLineTextBox23.Text;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = ffmpegPrepend + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();

            MessageBox.Show("Succesfully merged all videos into a unique one new video file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to merge all videos into a unique one new video file!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}