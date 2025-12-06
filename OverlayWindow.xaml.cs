using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Interop;

namespace AppHiderNet
{
    public partial class OverlayWindow : Window
    {
        private DispatcherTimer _timer;
        private IntPtr _targetHwnd = IntPtr.Zero;
        private IntPtr _lastTargetHwnd = IntPtr.Zero;

        public OverlayWindow()
        {
            InitializeComponent();
            
            // Set up timer for polling active window
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += Timer_Tick;
            _timer.Start();
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
                if (foregroundHwnd == IntPtr.Zero)
                {
                    return;
                }

                string title = NativeMethods.GetWindowTitle(foregroundHwnd);
                // Don't track ourselves or empty titles (usually)
                if (string.IsNullOrEmpty(title) || title == "App Hider .NET" || title == "Overlay")
                {
                    this.Visibility = Visibility.Collapsed;
                    return;
                }

                // Check if window is visible and not minimized
                if (!NativeMethods.IsWindowVisible(foregroundHwnd))
                {
                    this.Visibility = Visibility.Collapsed;
                    return;
                }

                NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
                placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);
                NativeMethods.GetWindowPlacement(foregroundHwnd, ref placement);

                if (placement.showCmd == NativeMethods.SW_SHOWMINIMIZED)
                {
                    this.Visibility = Visibility.Collapsed;
                    return;
                }

                // Get Window Rect
                NativeMethods.RECT rect;
                if (NativeMethods.GetWindowRect(foregroundHwnd, out rect))
                {
                    int x = rect.Left;
                    int y = rect.Top;
                    int w = rect.Right - rect.Left;
                    int h = rect.Bottom - rect.Top;

                    // Position logic
                    // Standard Windows 10/11 title bar height is roughly 30-40px
                    // We want to be to the left of the Minimize button.
                    // Approximate width of Caption Buttons (Min, Max, Close) is ~140px on Win10
                    
                    double buttonW = this.Width;
                    double buttonH = this.Height;
                    
                    // Offset from right edge
                    int offsetRight = 150; 
                    
                    double posX = x + w - offsetRight;
                    double posY = y + 8; // Slight top padding
                    
                    // Ensure we are within screen bounds (basic check)
                    if (posX < 0) posX = 0;
                    if (posY < 0) posY = 0;

                    this.Left = posX;
                    this.Top = posY;
                    
                    if (this.Visibility != Visibility.Visible)
                    {
                        this.Visibility = Visibility.Visible;
                    }
                    
                    // Ensure we are topmost
                    this.Topmost = true;

                    _targetHwnd = foregroundHwnd;
                    _lastTargetHwnd = foregroundHwnd;
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
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


    }
}
