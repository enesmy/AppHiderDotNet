using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Forms; // For ContextMenuStrip
using Application = System.Windows.Application; // Resolve ambiguity
using Point = System.Windows.Point; // Resolve ambiguity

namespace AppHiderNet
{
    public partial class OverlayWindow : Window
    {
        private DispatcherTimer _timer;
        private IntPtr _targetHwnd = IntPtr.Zero;
        private IntPtr _lastTargetHwnd = IntPtr.Zero;

        private bool _isDragging = false;
        private Point _startPoint;

        public OverlayWindow()
        {
            InitializeComponent();
            
            // Set up timer for polling active window
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Initial position (Bottom Right)
            this.Left = SystemParameters.PrimaryScreenWidth - 100;
            this.Top = SystemParameters.PrimaryScreenHeight - 150;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                IntPtr foregroundHwnd = NativeMethods.GetForegroundWindow();

                // If we (the overlay) have focus, don't change anything
                if (foregroundHwnd == new WindowInteropHelper(this).Handle)
                {
                    return;
                }

                // Filter out invalid windows
                if (foregroundHwnd == IntPtr.Zero) return;

                string title = NativeMethods.GetWindowTitle(foregroundHwnd);
                // Don't track ourselves or empty titles
                if (string.IsNullOrEmpty(title) || title == "App Hider .NET" || title == "Overlay")
                {
                    return;
                }

                // Check if window is visible and not minimized
                if (!NativeMethods.IsWindowVisible(foregroundHwnd)) return;

                NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
                placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);
                NativeMethods.GetWindowPlacement(foregroundHwnd, ref placement);

                if (placement.showCmd == NativeMethods.SW_SHOWMINIMIZED) return;

                _targetHwnd = foregroundHwnd;
                _lastTargetHwnd = foregroundHwnd;
            }
            catch
            {
                // Ignore errors
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double Click -> Open Main Window
                ((App)Application.Current).ShowMainWindow();
                return;
            }

            _isDragging = false;
            _startPoint = e.GetPosition(this);
            MainContainer.CaptureMouse();
        }

        private void MainContainer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MainContainer.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(this);
                if (Math.Abs(currentPoint.X - _startPoint.X) > 5 || Math.Abs(currentPoint.Y - _startPoint.Y) > 5)
                {
                    _isDragging = true;
                    this.DragMove();
                    MainContainer.ReleaseMouseCapture();
                }
            }
        }

        private void MainContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainContainer.ReleaseMouseCapture();
            if (!_isDragging)
            {
                // Click -> Hide Active Window
                HideTargetWindow();
            }
        }

        private void HideTargetWindow()
        {
            IntPtr hwndToHide = _targetHwnd != IntPtr.Zero ? _targetHwnd : _lastTargetHwnd;
            
            if (hwndToHide != IntPtr.Zero)
            {
                string title = NativeMethods.GetWindowTitle(hwndToHide);
                if (!string.IsNullOrEmpty(title))
                {
                    ((App)System.Windows.Application.Current).HideWindow(hwndToHide);
                }
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Show Context Menu with Hidden Apps
            var contextMenu = new ContextMenuStrip();
            var hiddenApps = ((App)Application.Current).GetHiddenWindows();

            if (hiddenApps.Count > 0)
            {
                foreach (var kvp in hiddenApps)
                {
                    var item = new ToolStripMenuItem($"Restore: {kvp.Value}");
                    item.Tag = kvp.Key;
                    item.Click += (s, ev) => ((App)Application.Current).ShowHiddenWindowPublic((IntPtr)((ToolStripMenuItem)s).Tag);
                    contextMenu.Items.Add(item);
                }
                contextMenu.Items.Add(new ToolStripSeparator());
            }
            else
            {
                contextMenu.Items.Add(new ToolStripMenuItem("No hidden apps") { Enabled = false });
                contextMenu.Items.Add(new ToolStripSeparator());
            }

            var openMainItem = new ToolStripMenuItem("Open Manager");
            openMainItem.Click += (s, ev) => ((App)Application.Current).ShowMainWindow();
            contextMenu.Items.Add(openMainItem);

            // Show at mouse position
            contextMenu.Show(System.Windows.Forms.Cursor.Position);
        }
    }
}
