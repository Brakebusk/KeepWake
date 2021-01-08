using System;
using System.Windows.Forms;
using System.Drawing;

namespace Keepwake
{
    class Program
    {
        private NotifyIcon notifyIcon;
        private ContextMenu contextMenu;
        private MenuItem menuItemExit;
        private System.ComponentModel.IContainer components;
        enum Mode
        {
            KeepWake,
            AllowSleep
        }
        private Mode mode = Mode.AllowSleep;

        static void Main(string[] args)
        {
            Program program = new Program();
            Application.Run();
            Console.ReadLine();
        }
        Program()
        {
            CreateTaskbarIcon();
        }

        private void CreateTaskbarIcon()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new ContextMenu();

            //Create menu item for closing program
            this.menuItemExit = new MenuItem();
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new EventHandler(this.Exit);

            this.contextMenu.MenuItems.AddRange(
                new MenuItem[] { this.menuItemExit });

            //Create clickable taskbar icon
            this.notifyIcon = new NotifyIcon(this.components);
            this.notifyIcon.Icon = new Icon("empty.ico");

            this.notifyIcon.ContextMenu = this.contextMenu;

            notifyIcon.Text = "KeepWake";
            notifyIcon.Visible = true;

            notifyIcon.Click += new EventHandler(this.ToggleMode);

        }

        private void ToggleMode(object Sender, EventArgs e)
        {
            //Called by clicking the task bar icon
            if (this.mode == Mode.AllowSleep)
            {
                this.mode = Mode.KeepWake;
                this.notifyIcon.Icon = new Icon("filled.ico");
            }
            else
            {
                this.mode = Mode.AllowSleep;
                this.notifyIcon.Icon = new Icon("empty.ico");
            }


        }

        private void Exit(object Sender, EventArgs e)
        {
            //Called by clicking the exit button in the menu
            Application.Exit();
        }
    }
}
