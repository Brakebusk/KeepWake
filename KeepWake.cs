using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Microsoft.Win32;

namespace Keepwake
{
    class KeepWake
    {
        private NotifyIcon notifyIcon;
        private ContextMenu contextMenu;
        private MenuItem menuItemStart;
        private MenuItem menuItemExit;
        private System.ComponentModel.IContainer components;

        //Create helper thread for keeping windows awake
        private static ManualResetEventSlim mre = new ManualResetEventSlim(false);
        private Thread wakerThread = new Thread(new ThreadStart(Waker));
        
        //Application can be in two states: Prevent Windows from sleeping or allow it
        enum State : int
        {
            KeepWake = 0,
            AllowSleep = 1
        }
        String[] stateCaptions =
        {
            "Prevent sleep",
            "Allow sleep"
        };
        private State state = State.AllowSleep;

        RegistryKey registryStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        static void Main(string[] args)
        {
            //Initialize program as running in the background with the task bar icon as only interface
            _ = new KeepWake();
            Application.Run();
            Console.ReadLine();
        }
        KeepWake()
        {
            wakerThread.Start();
            mre.Reset();
            CreateTaskbarIcon();
        }

        private void CreateTaskbarIcon()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new ContextMenu();

            //Create menu item for toggling "start with Windows"
            this.menuItemStart = new MenuItem();
            this.menuItemStart.Text = "Start with Windows";
            //Check registry for startup entry
            if (registryStartup.GetValue("Keepwake") != null)
                this.menuItemStart.Checked = true;
            this.menuItemStart.Click += new EventHandler(this.ToggleStartWithWindows);


            //Create menu item for closing program
            this.menuItemExit = new MenuItem();
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new EventHandler(this.Exit);

            //Add all menu items to context menu
            this.contextMenu.MenuItems.AddRange(
                new MenuItem[] { 
                    this.menuItemStart,
                    this.menuItemExit 
                });

            //Create clickable taskbar icon
            this.notifyIcon = new NotifyIcon(this.components);
            this.notifyIcon.Icon = new Icon("empty.ico");

            this.notifyIcon.ContextMenu = this.contextMenu;

            notifyIcon.Text = "KeepWake: Allow Sleep";
            notifyIcon.Text = String.Format("KeepWake: {0}", stateCaptions[(int) this.state]);
            notifyIcon.Visible = true;

            notifyIcon.Click += new EventHandler(this.ToggleMode);
        }

        private void ToggleMode(object Sender, EventArgs e)
        {
            //Called by clicking the task bar icon
            
            //Only handle left clicks of task bar icon
            if (((MouseEventArgs) e).Button != MouseButtons.Left) return;

            if (this.state == State.AllowSleep)
            {
                this.state = State.KeepWake;
                this.notifyIcon.Icon = new Icon("filled.ico");
                Console.WriteLine("Staring waker");
                mre.Set();
            }
            else
            {
                this.state = State.AllowSleep;
                this.notifyIcon.Icon = new Icon("empty.ico");
                Console.WriteLine("Suspending waker");
                mre.Reset();
            }

            notifyIcon.Text = String.Format("KeepWake: {0}", stateCaptions[(int)this.state]);
        }

        private void ToggleStartWithWindows(object Sender, EventArgs e)
        {
            menuItemStart.Checked = !menuItemStart.Checked;

            if (menuItemStart.Checked)
            {
                registryStartup.SetValue("Keepwake", Application.ExecutablePath);
            } else
            {
                registryStartup.DeleteValue("Keepwake", false);
            }
        }

        private void Exit(object Sender, EventArgs e)
        {
            //Called by clicking the exit button in the menu
            wakerThread.Abort();
            Application.Exit();
        }

        private static void Waker()
        {
            //The waker thread will run in the background if application is in state KeepWake.
            //It will periodically simulate a keypress to prevent sleep
            //When in the AllowSleep state it will wait until it is once again told to keep Windows awake
            while (true)
            {
                mre.Wait();
                System.Threading.Thread.Sleep(60 * 1000);
                Console.WriteLine("Sending wake signal to Windows");
                SendKeys.SendWait("{F15}");
            }
        }
    }
}
