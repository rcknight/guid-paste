using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace WinGuid
{
    public class WinGuid : Form
    {
        public const int WIN = 8;
        public const int WM_HOTKEY_MSG_ID = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public static void PressKey(Keys key, bool up)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            if (up)
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
            else
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.Run(new WinGuid());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public WinGuid()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);
                
            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "WinGuid";
            trayIcon.Icon = new Icon(SystemIcons.WinLogo, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        private bool SetUpHotkeys()
        {
            return RegisterHotKey(this.Handle, 0, WIN, Keys.J.GetHashCode());
        }

        private bool RemoveHotKeys()
        {
            return UnregisterHotKey(this.Handle, 0);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            SetUpHotkeys();
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            RemoveHotKeys();
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY_MSG_ID)
                SendGuid();   
        }

        private void SendGuid()
        {
            var guid = Guid.NewGuid().ToString().ToUpper();

            //generate a list of keystrokes
            var keystrokes = guid.Select(c => {
                switch(c)
                {
                    case '0': case '1': case '2': case '3': case '4':
                    case '5': case '6': case '7': case '8': case '9':
                        return (Keys)Enum.Parse(typeof (Keys), "D" + c);
                    case '-':
                        return Keys.Subtract;
                    default:
                        return (Keys)Enum.Parse(typeof (Keys), c.ToString());
                }
            });

            PressKey(Keys.LWin, true);
            foreach (var key in keystrokes)
            {
                PressKey(key, false);
                PressKey(key, true);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
