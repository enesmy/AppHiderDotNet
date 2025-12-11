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
        
        private Dictionary<IntPtr, string> _blurredWindows = new Dictionary<IntPtr, string>();
        public ObservableCollection<KeyValuePair<IntPtr, string>> BlurredWindowsList { get; } = new ObservableCollection<KeyValuePair<IntPtr, string>>();
        private Dictionary<IntPtr, Window> _blurOverlays = new Dictionary<IntPtr, Window>();
        
        private const int HOTKEY_ID_NUMPAD = 9000;
        private const int HOTKEY_ID_DIGIT = 9001;
        private const int HOTKEY_ID_BLUR_NUMPAD = 9002;
        private const int HOTKEY_ID_BLUR_DIGIT = 9003;
        
        // Settings Properties
        public bool StartMinimized { get; set; }
        public bool ShowOverlayButton { get; set; }
        public bool PasswordProtectionEnabled { get; set; }
        public string? MasterPassword { get; set; }

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

            // Load settings
            var settings = StateManager.LoadSettings();
            StartMinimized = settings.StartMinimized;
            ShowOverlayButton = settings.ShowOverlayButton;
            PasswordProtectionEnabled = settings.PasswordProtectionEnabled;
            MasterPassword = settings.MasterPassword;

            // Initialize Manager Window
            _mainWindow = new MainWindow();
            
            // Show or hide based on settings
            if (!StartMinimized)
            {
                _mainWindow.Show();
            }

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

            // Register Ctrl+Shift+2 for blur
            bool successBlurNum = NativeMethods.RegisterHotKey(IntPtr.Zero, HOTKEY_ID_BLUR_NUMPAD, 
                NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, 
                (int)Keys.NumPad2);

            bool successBlurDig = NativeMethods.RegisterHotKey(IntPtr.Zero, HOTKEY_ID_BLUR_DIGIT, 
                NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, 
                (int)Keys.D2);

            if (!successBlurNum && !successBlurDig)
            {
                System.Windows.MessageBox.Show("Could not register blur hotkeys (Ctrl+Shift+2 or NumPad2).\nCheck for conflicts.");
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
                    
                    if (app.IsBlurred && !_blurredWindows.ContainsKey(hwnd))
                    {
                        _blurredWindows[hwnd] = app.Title;
                        BlurredWindowsList.Add(new KeyValuePair<IntPtr, string>(hwnd, app.Title));
                        ApplyBlur(hwnd);
                    }
                }
            }
        }

        private void SaveState()
        {
            var appsToSave = new List<HiddenApp>();
            
            // Combine hidden and blurred windows
            var allWindows = new Dictionary<IntPtr, HiddenApp>();
            
            foreach (var kvp in _hiddenWindows)
            {
                string pwd = WindowPasswords.ContainsKey(kvp.Key) ? WindowPasswords[kvp.Key] : null;
                allWindows[kvp.Key] = new HiddenApp 
                { 
                    Hwnd = (long)kvp.Key, 
                    Title = kvp.Value, 
                    Password = pwd,
                    IsBlurred = false
                };
            }
            
            foreach (var kvp in _blurredWindows)
            {
                if (allWindows.ContainsKey(kvp.Key))
                {
                    allWindows[kvp.Key].IsBlurred = true;
                }
                else
                {
                    allWindows[kvp.Key] = new HiddenApp 
                    { 
                        Hwnd = (long)kvp.Key, 
                        Title = kvp.Value, 
                        Password = null,
                        IsBlurred = true
                    };
                }
            }
            
            StateManager.Save(allWindows.Values.ToList());
        }

        public void ShowMainWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            // _mainWindow.RefreshList(); // Not needed with ObservableCollection
        }

        public void ShowAboutWindow()
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        public void ShowSettingsWindow()
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
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

        public void HideWindowPublic(IntPtr hwnd, string title, string? password = null)
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
            
            if (_blurredWindows.Count > 0)
            {
                foreach (var kvp in _blurredWindows)
                {
                    var item = new ToolStripMenuItem($"Unblur: {kvp.Value}");
                    item.Tag = kvp.Key;
                    item.Click += (s, e) => RemoveBlurPublic((IntPtr)((ToolStripMenuItem)s).Tag);
                    contextMenu.Items.Add(item);
                }
                contextMenu.Items.Add(new ToolStripSeparator());
            }

            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (s, e) => ShowSettingsWindow();
            contextMenu.Items.Add(settingsItem);

            var aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += (s, e) => ShowAboutWindow();
            contextMenu.Items.Add(aboutItem);
            contextMenu.Items.Add(new ToolStripSeparator());

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
                else if (id == HOTKEY_ID_BLUR_NUMPAD || id == HOTKEY_ID_BLUR_DIGIT)
                {
                    ToggleBlurActiveWindow();
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

        public void ShowOverlayButtonWindow()
        {
            // TODO: Implement overlay button window if it exists
            // For now, this is a placeholder for the settings integration
        }

        public void HideOverlayButtonWindow()
        {
            // TODO: Implement overlay button window if it exists
            // For now, this is a placeholder for the settings integration
        }

        private void ToggleBlurActiveWindow()
        {
            IntPtr hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd != IntPtr.Zero)
            {
                string title = NativeMethods.GetWindowTitle(hwnd);
                if (!string.IsNullOrEmpty(title) && NativeMethods.IsWindowVisible(hwnd))
                {
                    ToggleBlurWindow(hwnd, title.Length > 30 ? title.Substring(0, 30) + "..." : title);
                }
            }
        }

        private void ToggleBlurWindow(IntPtr hwnd, string title)
        {
            if (_blurredWindows.ContainsKey(hwnd))
            {
                // Remove blur
                RemoveBlur(hwnd);
                _blurredWindows.Remove(hwnd);
                
                Application.Current.Dispatcher.Invoke(() => 
                {
                    var itemToRemove = BlurredWindowsList.FirstOrDefault(x => x.Key == hwnd);
                    if (!itemToRemove.Equals(default(KeyValuePair<IntPtr, string>)))
                    {
                        BlurredWindowsList.Remove(itemToRemove);
                    }
                });
            }
            else
            {
                // Apply blur
                if (ApplyBlur(hwnd))
                {
                    _blurredWindows[hwnd] = title;
                    
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        BlurredWindowsList.Add(new KeyValuePair<IntPtr, string>(hwnd, title));
                    });
                }
            }
            
            UpdateContextMenu();
            SaveState();
        }

        public void RemoveBlurPublic(IntPtr hwnd)
        {
            if (_blurredWindows.ContainsKey(hwnd))
            {
                RemoveBlur(hwnd);
                _blurredWindows.Remove(hwnd);
                
                Application.Current.Dispatcher.Invoke(() => 
                {
                    var itemToRemove = BlurredWindowsList.FirstOrDefault(x => x.Key == hwnd);
                    if (!itemToRemove.Equals(default(KeyValuePair<IntPtr, string>)))
                    {
                        BlurredWindowsList.Remove(itemToRemove);
                    }
                });
                
                UpdateContextMenu();
                SaveState();
            }
        }

        private bool ApplyBlur(IntPtr hwnd)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var overlay = new BlurOverlay(hwnd);
                    overlay.Show();
                    _blurOverlays[hwnd] = overlay;
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RemoveBlur(IntPtr hwnd)
        {
            try
            {
                if (_blurOverlays.ContainsKey(hwnd))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var overlay = _blurOverlays[hwnd];
                        overlay.Close();
                        _blurOverlays.Remove(hwnd);
                    });
                }
            }
            catch
            {
                // Ignore errors when removing blur
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Unregister Hotkeys
            NativeMethods.UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_NUMPAD);
            NativeMethods.UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_DIGIT);
            NativeMethods.UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_BLUR_NUMPAD);
            NativeMethods.UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_BLUR_DIGIT);
            
            // Remove blur from all blurred windows
            foreach (var kvp in _blurredWindows.ToList())
            {
                RemoveBlur(kvp.Key);
            }

            // Do NOT show hidden windows on exit. They remain hidden.
            // Persistence handles restoring them on next run.

            if (_notifyIcon != null) _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
