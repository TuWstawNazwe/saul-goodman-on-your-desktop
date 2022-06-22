using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Saul_Goodman_3D
{
    public partial class MainForm : Form
    {
        int iTimeElapsed = 0;
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Saul Goodman");

        public MainForm()
        {
            InitializeComponent();

            IntPtr progman = W32.FindWindow("Progman", null);

            IntPtr result = IntPtr.Zero;

            W32.SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, W32.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);

            IntPtr workerw = IntPtr.Zero;

            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    workerw = W32.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
                }
                return true; 

            }), IntPtr.Zero);

            W32.SetParent(Handle, workerw);

            Setup();
        }

        static void PrintVisibleWindowHandles(IntPtr hwnd, int maxLevel = -1, int level = 0)
        {
            bool isVisible = W32.IsWindowVisible(hwnd);

            if (isVisible && (maxLevel == -1 || level <= maxLevel))
            {
                StringBuilder className = new StringBuilder(256);
                W32.GetClassName(hwnd, className, className.Capacity);

                StringBuilder windowTitle = new StringBuilder(256);
                W32.GetWindowText(hwnd, windowTitle, className.Capacity);

                Console.WriteLine("".PadLeft(level * 2) + "0x{0:X8} \"{1}\" {2}", hwnd.ToInt64(), windowTitle, className);

                level++;

                W32.EnumChildWindows(hwnd, new W32.EnumWindowsProc((childhandle, childparamhandle) =>
                {
                    PrintVisibleWindowHandles(childhandle, maxLevel, level);
                    return true;
                }), IntPtr.Zero);
            }
        }

        void Setup()
        {
            
            Directory.CreateDirectory(path);

            if (File.Exists(Path.Combine(path, "saul-goodman.mp4")) == true)
            {
                //do nothing
            } else
            {
                File.WriteAllBytes(Path.Combine(path, "saul-goodman.mp4"), Properties.Resources.saul_goodman);
            }

            if (File.Exists(Path.Combine(path, "time-elapsed")) == true)
            {
                try
                {
                    iTimeElapsed = Int32.Parse(File.ReadAllText(Path.Combine(path, "time-elapsed")));
                } catch(Exception e)
                {
                    MessageBox.Show("Caught " + e.GetType() + " when loading \"time-elapsed\" file (Did you modify it?)\n Fix: Go to %appdata% > Saul Goodman > Remove \"time-elapsed\" file.");
                    videoPlayer.close();
                    videoPlayer.Dispose();
                    Close();
                }
            }
            else
            {
                File.WriteAllText(Path.Combine(path, "time-elapsed"), "0");
            }

            videoPlayer.stretchToFit = true;
            videoPlayer.uiMode = "none";
            videoPlayer.settings.setMode("loop", true);
            string filename = Path.Combine(path, "saul-goodman.mp4");
            videoPlayer.URL = filename;

            notifyIcon.Text = "Saul Goodman";
            notifyIcon.Icon = this.Icon;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Visible = true;


        }
        private void closeSaulGoodman_Click(object sender, EventArgs e)
        {
            videoPlayer.close();
            videoPlayer.Dispose();
            //Process.Start("ms-settings:personalization-background");
            Close();
        }

        private void muteSaulGoodman_Click(object sender, EventArgs e)
        {
            if (videoPlayer.settings.mute == true)
            {
                muteSaulGoodman.Text = "Mute Saul Goodman";
                videoPlayer.settings.mute = false;
            } else
            {
                muteSaulGoodman.Text = "Unmute Saul Goodman";
                videoPlayer.settings.mute = true;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            iTimeElapsed++;
            File.WriteAllText(Path.Combine(path, "time-elapsed"), iTimeElapsed.ToString());
            timeElapsed.Text = "Saul goodman has been on your desktop for " + iTimeElapsed + " seconds.";
        }
    }
}