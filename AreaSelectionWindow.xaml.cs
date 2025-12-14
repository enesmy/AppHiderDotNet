using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Point = System.Windows.Point;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AppHiderNet
{
    public partial class AreaSelectionWindow : Window
    {
        private Point _startPoint;
        private bool _isDragging;
        
        public Rect SelectedArea { get; private set; } = Rect.Empty;
        public bool IsSelectionConfirmed { get; private set; } = false;

        public AreaSelectionWindow()
        {
            InitializeComponent();
            
            // Ensure it covers all screens
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = e.GetPosition(SelectionCanvas);
                _isDragging = true;
                
                SelectionRect.Visibility = Visibility.Visible;
                Canvas.SetLeft(SelectionRect, _startPoint.X);
                Canvas.SetTop(SelectionRect, _startPoint.Y);
                SelectionRect.Width = 0;
                SelectionRect.Height = 0;
                
                Mouse.Capture(this);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(SelectionCanvas);
                
                double x = Math.Min(currentPoint.X, _startPoint.X);
                double y = Math.Min(currentPoint.Y, _startPoint.Y);
                double w = Math.Abs(currentPoint.X - _startPoint.X);
                double h = Math.Abs(currentPoint.Y - _startPoint.Y);
                
                Canvas.SetLeft(SelectionRect, x);
                Canvas.SetTop(SelectionRect, y);
                SelectionRect.Width = w;
                SelectionRect.Height = h;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
                
                Point endPoint = e.GetPosition(SelectionCanvas);
                
                double x = Math.Min(endPoint.X, _startPoint.X);
                double y = Math.Min(endPoint.Y, _startPoint.Y);
                double w = Math.Abs(endPoint.X - _startPoint.X);
                double h = Math.Abs(endPoint.Y - _startPoint.Y);
                
                // Only confirm if area is big enough (avoid accidental clicks)
                if (w > 10 && h > 10)
                {
                    SelectedArea = new Rect(x, y, w, h);
                    
                    // Convert to screen coordinates if needed, but since window is maximized over virtual screen,
                    // client coordinates should map closely to screen coordinates relative to VirtualScreenTop/Left.
                    // However, Window is maximized, so (0,0) is top-left of primary monitor usually?
                    // Actually, we manually set Left/Top/Width/Height to VirtualScreen.
                    // So (0,0) in Canvas is (VirtualScreenLeft, VirtualScreenTop).
                    
                    // Adjust rect to absolute screen coordinates
                    SelectedArea = new Rect(x + this.Left, y + this.Top, w, h);
                    
                    IsSelectionConfirmed = true;
                    this.Close();
                }
                else
                {
                    // Reset if too small
                    SelectionRect.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsSelectionConfirmed = false;
                this.Close();
            }
        }
    }
}
