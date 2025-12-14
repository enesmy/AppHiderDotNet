using System;
using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace AppHiderNet
{
    public partial class CensorOverlayWindow : Window
    {
        public event EventHandler RequestClose;

        public CensorOverlayWindow(Rect area)
        {
            InitializeComponent();
            
            this.Left = area.Left;
            this.Top = area.Top;
            this.Width = area.Width;
            this.Height = area.Height;
        }

        private void RemoveCensor_Click(object sender, RoutedEventArgs e)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // Only drag if we are not resizing (WindowChrome handles the edges, but just in case)
            this.DragMove();
        }

        private void SetImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    CustomImage.Source = bitmap;
                    CustomImage.Visibility = Visibility.Visible;
                    DefaultVisuals.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UseDefault_Click(object sender, RoutedEventArgs e)
        {
            CustomImage.Visibility = Visibility.Collapsed;
            DefaultVisuals.Visibility = Visibility.Visible;
        }
    }
}
