using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Collections.ObjectModel;
using System.Linq;
using Application = System.Windows.Application;

namespace AppHiderNet
{
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;
        private Dictionary<IntPtr, string> _hiddenWindows = new Dictionary<IntPtr, string>();
        public ObservableCollection<KeyValuePair<IntPtr, string>> HiddenWindowsList { get; } = new ObservableCollection<KeyValuePair<IntPtr, string>>();
        
        private const int HOTKEY_ID_NUMPAD = 9000;
        private const int HOTKEY_ID_DIGIT = 9001;
        
 

        // Manager Window
        private MainWindow _mainWindow;

        private static System.Threading.Mutex _mutex = null;

        public Dictionary<IntPtr, string> WindowPasswords { get; set; } = new Dictionary<IntPtr, string>();

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "AppHiderNet_Unique_Mutex_Name";
            bool createdNew;
            _mutex = new System.Threading.Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                System.Windows.MessageBox.Show("App Hider is already running! Check the System Tray.");
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            // Initialize Manager Window
            _mainWindow = new MainWindow();
            _mainWindow.Show(); // Show on startup so user sees it

            // Initialize Tray Icon
            _notifyIcon = new NotifyIcon();
            
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_icon.png");
            if (System.IO.File.Exists(iconPath))
            {
                try 
                {
                    // Create Icon from PNG
                    using (var bitmap = new Bitmap(iconPath))
                    {
                        _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
                    }
                }
                catch
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application; 
            }

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "App Hider .NET";
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            UpdateContextMenu();

            // Register Hotkeys
            ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
            
            bool successNum = NativeMethods.RegisterHotKey(IntPtr.Zero, HOTKEY_ID_NUMPAD, 
                NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, 
                (int)Keys.NumPad1);

            bool successDig = NativeMethods.RegisterHotKey(IntPtr.Zero, HOTKEY_ID_DIGIT, 
                NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, 
                (int)Keys.D1);

            if (!successNum && !successDig)
            {
                System.Windows.MessageBox.Show("Could not register hotkeys (Ctrl+Shift+1 or NumPad1).\nCheck for conflicts.");
            }
            
         

            LoadState();
        }

        private void LoadState()
        {
            var savedApps = StateManager.Load();
            foreach (var app in savedApps)
            {
                IntPtr hwnd = (IntPtr)app.Hwnd;
                if (NativeMethods.IsWindow(hwnd))
                {
                    // Verify it's still running and valid
                    if (!_hiddenWindows.ContainsKey(hwnd))
                    {
                        _hiddenWindows[hwnd] = app.Title;
                        HiddenWindowsList.Add(new KeyValuePair<IntPtr, string>(hwnd, app.Title));
                        
                        if (!string.IsNullOrEmpty(app.Password))
                        {
                            WindowPasswords[hwnd] = app.Password;
                        }
                    }
                }
            }
        }

        private void SaveState()
        {
            var appsToSave = new List<HiddenApp>();
            foreach (var kvp in _hiddenWindows)
            {
                string pwd = WindowPasswords.ContainsKey(kvp.Key) ? WindowPasswords[kvp.Key] : null;
                appsToSave.Add(new HiddenApp 
                { 
                    Hwnd = (long)kvp.Key, 
                    Title = kvp.Value, 
                    Password = pwd 
                });
            }
            StateManager.Save(appsToSave);
        }

        public void ShowMainWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            // _mainWindow.RefreshList(); // Not needed with ObservableCollection
        }

        public Dictionary<IntPtr, string> GetHiddenWindows()
        {
            return new Dictionary<IntPtr, string>(_hiddenWindows);
        }

        public void ShowHiddenWindowPublic(IntPtr hwnd)
        {
            if (WindowPasswords.ContainsKey(hwnd))
            {
                var pwdDialog = new PasswordDialog();
                pwdDialog.ExpectedPassword = WindowPasswords[hwnd]; // Pass expected password
                
                if (pwdDialog.ShowDialog() != true)
                {
                    return; // Cancelled or failed (though failed won't return true now)
                }
            }
            ShowHiddenWindow(hwnd);
        }

        public void HideWindowPublic(IntPtr hwnd, string title, string password = null)
        {
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
                
                if (!_hiddenWindows.ContainsKey(hwnd))
                {
                    _hiddenWindows[hwnd] = title;
                    
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        HiddenWindowsList.Add(new KeyValuePair<IntPtr, string>(hwnd, title));
                    });

                    if (!string.IsNullOrEmpty(password))
                    {
                        WindowPasswords[hwnd] = password;
                    }

                    UpdateContextMenu();
                    SaveState();
                }
            }
        }

        private void UpdateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            
            var managerItem = new ToolStripMenuItem("Open Manager");
            managerItem.Click += (s, e) => ShowMainWindow();
            managerItem.Font = new Font(managerItem.Font, System.Drawing.FontStyle.Bold);
            contextMenu.Items.Add(managerItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            if (_hiddenWindows.Count > 0)
            {
                foreach (var kvp in _hiddenWindows)
                {
                    var item = new ToolStripMenuItem($"Show: {kvp.Value}");
                    item.Tag = kvp.Key;
                    item.Click += (s, e) => ShowHiddenWindowPublic((IntPtr)((ToolStripMenuItem)s).Tag);
                    contextMenu.Items.Add(item);
                }
                contextMenu.Items.Add(new ToolStripSeparator());
            }

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Shutdown();
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            
            // Also refresh main window if open (though ObservableCollection handles list, this updates context menu)
        }

        private void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == NativeMethods.WM_HOTKEY)
            {
                int id = msg.wParam.ToInt32();
                if (id == HOTKEY_ID_NUMPAD || id == HOTKEY_ID_DIGIT)
                {
                    HideActiveWindow();
                    handled = true;
                }
            }
        }

        public void HideWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                string title = NativeMethods.GetWindowTitle(hwnd);
                if (!string.IsNullOrEmpty(title) && NativeMethods.IsWindowVisible(hwnd))
                {
                    HideWindowPublic(hwnd, title.Length > 30 ? title.Substring(0, 30) + "..." : title);
                }
            }
        }

        private void HideActiveWindow()
        {
            IntPtr hwnd = NativeMethods.GetForegroundWindow();
            HideWindow(hwnd);
        }

        private void ShowHiddenWindow(IntPtr hwnd)
        {
            if (_hiddenWindows.ContainsKey(hwnd))
            {
                // Use SW_RESTORE to ensure it comes back from minimized state if needed
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
                NativeMethods.SetForegroundWindow(hwnd);
                _hiddenWindows.Remove(hwnd);
                
                Application.Current.Dispatcher.Invoke(() => 
                {
                    var itemToRemove = HiddenWindowsList.FirstOrDefault(x => x.Key == hwnd);
                    if (!itemToRemove.Equals(default(KeyValuePair<IntPtr, string>)))
                    {
                        HiddenWindowsList.Remove(itemToRemove);
                    }
                });

                if (WindowPasswords.ContainsKey(hwnd))
                {
                    WindowPasswords.Remove(hwnd);
                }

                UpdateContextMenu();
                SaveState();
            }
            else
            {
                System.Windows.MessageBox.Show("Window handle not found in list.");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Unregister Hotkeys
            NativeMethods.UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_NUMPAD);
            NativeMethods.UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_DIGIT);

            // Do NOT show hidden windows on exit. They remain hidden.
            // Persistence handles restoring them on next run.

            if (_notifyIcon != null) _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
