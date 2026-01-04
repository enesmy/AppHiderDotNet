using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace AppHiderNet
{
    public partial class WindowSelectionOverlay : Window
    {
        public event Action<IntPtr> WindowSelected;
        private DispatcherTimer _timer;
        private IntPtr _lastHwnd = IntPtr.Zero;

        public WindowSelectionOverlay()
        {
            InitializeComponent();
            
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            this.Loaded += WindowSelectionOverlay_Loaded;
        }

        private void WindowSelectionOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.Focus();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var point = System.Windows.Forms.Cursor.Position;
            IntPtr hwnd = FindWindowUnderMouse(point);
            
            if (hwnd != IntPtr.Zero)
            {
                if (hwnd != _lastHwnd)
                {
                    _lastHwnd = hwnd;
                    HighlightWindow(hwnd);
                }
            }
        }

        private IntPtr FindWindowUnderMouse(System.Drawing.Point point)
        {
            IntPtr currentHwnd = NativeMethods.GetTopWindow(IntPtr.Zero);
            IntPtr overlayHwnd = new WindowInteropHelper(this).Handle;

            while (currentHwnd != IntPtr.Zero)
            {
                if (currentHwnd != overlayHwnd && NativeMethods.IsWindowVisible(currentHwnd))
                {
                    NativeMethods.RECT rect;
                    if (NativeMethods.GetWindowRect(currentHwnd, out rect))
                    {
                        if (point.X >= rect.Left && point.X <= rect.Right &&
                            point.Y >= rect.Top && point.Y <= rect.Bottom)
                        {
                            // Check if it's a root window or has a parent
                            IntPtr root = NativeMethods.GetAncestor(currentHwnd, NativeMethods.GA_ROOT);
                            return root != IntPtr.Zero ? root : currentHwnd;
                        }
                    }
                }
                currentHwnd = NativeMethods.GetWindow(currentHwnd, NativeMethods.GW_HWNDNEXT);
            }
            return IntPtr.Zero;
        }

        private void HighlightWindow(IntPtr hwnd)
        {
            if (hwnd == new WindowInteropHelper(this).Handle) return;

            NativeMethods.RECT rect;
            if (NativeMethods.GetWindowRect(hwnd, out rect))
            {
                double x = rect.Left - this.Left;
                double y = rect.Top - this.Top;
                double w = rect.Right - rect.Left;
                double h = rect.Bottom - rect.Top;

                HighlightBorder.Visibility = Visibility.Visible;
                Canvas.SetLeft(HighlightBorder, x);
                Canvas.SetTop(HighlightBorder, y);
                HighlightBorder.Width = w;
                HighlightBorder.Height = h;
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_lastHwnd != IntPtr.Zero && _lastHwnd != new WindowInteropHelper(this).Handle)
                {
                    WindowSelected?.Invoke(_lastHwnd);
                    CloseOverlay();
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                CloseOverlay();
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Timer handles hit testing to avoid lag, but we could do it here too.
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseOverlay();
            }
        }

        private void CloseOverlay()
        {
            _timer.Stop();
            this.Close();
        }
    }
}
