using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AppHiderNet
{
    public partial class BlurOverlay : Window
    {
        private IntPtr _targetWindow;
        private DispatcherTimer _updateTimer;

        public BlurOverlay(IntPtr targetWindow)
        {
            InitializeComponent();
            _targetWindow = targetWindow;
            
            // Position the overlay over the target window
            UpdatePosition();
            
            // Set up timer to keep overlay positioned correctly
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var hwndSource = (System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this);
            if (hwndSource != null)
            {
                IntPtr overlayHwnd = hwndSource.Handle;
                
                // Set WS_EX_NOACTIVATE style so overlay doesn't steal focus
                int exStyle = NativeMethods.GetWindowLong(overlayHwnd, NativeMethods.GWL_EXSTYLE);
                exStyle |= NativeMethods.WS_EX_NOACTIVATE;
                NativeMethods.SetWindowLong(overlayHwnd, NativeMethods.GWL_EXSTYLE, exStyle);
                
                // Use TOPMOST then NOTOPMOST trick to force overlay in front
                // First make it topmost
                NativeMethods.SetWindowPos(overlayHwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                
                // Then remove topmost so other windows can go on top
                NativeMethods.SetWindowPos(overlayHwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Prevent Alt+F4 from closing the overlay
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
            // Prevent all other key inputs to the overlay
            e.Handled = true;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Check if target window still exists
            if (!NativeMethods.IsWindow(_targetWindow))
            {
                this.Close();
                return;
            }

            // Check window placement to detect minimize
            NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
            placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);
            
            if (NativeMethods.GetWindowPlacement(_targetWindow, ref placement))
            {
                // If window is minimized (SW_SHOWMINIMIZED = 2), hide overlay
                if (placement.showCmd == NativeMethods.SW_SHOWMINIMIZED)
                {
                    this.Visibility = Visibility.Hidden;
                    return;
                }
            }

            // Check if target window is visible
            if (!NativeMethods.IsWindowVisible(_targetWindow))
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
                UpdatePosition();
                
                // Only enforce z-order when target window is the foreground window
                IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
                if (foregroundWindow == _targetWindow)
                {
                    var hwndSource = (System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this);
                    if (hwndSource != null)
                    {
                        IntPtr overlayHwnd = hwndSource.Handle;
                        
                        // Use TOPMOST then NOTOPMOST trick to force overlay in front
                        NativeMethods.SetWindowPos(overlayHwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                        NativeMethods.SetWindowPos(overlayHwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                    }
                }
            }
        }

        private void UpdatePosition()
        {
            NativeMethods.RECT rect;
            if (NativeMethods.GetWindowRect(_targetWindow, out rect))
            {
                this.Left = rect.Left;
                this.Top = rect.Top;
                this.Width = rect.Right - rect.Left;
                this.Height = rect.Bottom - rect.Top;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
