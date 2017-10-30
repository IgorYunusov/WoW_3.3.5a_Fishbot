using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Project_5
{
    public partial class MainPanel : Form
    {

        public static bool enabled;
        public static Thread fishingThread;

        public MainPanel()
        {

            InitializeComponent();
        }

        // START/STOP BUTTON
        private void button1_Click(object sender, EventArgs e)
        {
            enabled = !enabled;

            if (enabled)
            {
                //fishing ability
                int virtualKey_Fishing = WindowsNative.VkKeyScan(textBox1.Text[0]);
                //interact with mouseover keybind
                int virtualKey_InteractWithMouseover = WindowsNative.VkKeyScan(textBox2.Text[0]);

                fishingThread = new Thread(() =>
                { 
                    Logic.initAndRun(virtualKey_Fishing, virtualKey_InteractWithMouseover);
                });

                fishingThread.Start();

                button1.Text = "Stop";
            }
            else
            {
                button1.Text = "Start";
            }
        }

        private void onClose(object sender, FormClosingEventArgs e)
        {
            enabled = false;
        }
    }
}
