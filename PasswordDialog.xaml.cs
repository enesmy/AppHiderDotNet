using System.Windows;
using System.Windows.Media.Animation;

namespace AppHiderNet
{
    public partial class PasswordDialog : Window
    {
        public string Password { get; private set; }
        public string ExpectedPassword { get; set; }

        public PasswordDialog()
        {
            InitializeComponent();
            PasswordInput.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ExpectedPassword))
            {
                if (PasswordInput.Password != ExpectedPassword)
                {
                    // Show Error
                    ErrorText.Visibility = Visibility.Visible;
                    PasswordInput.Clear();
                    PasswordInput.Focus();

                    // Play Shake Animation
                    var sb = (Storyboard)MainBorder.Resources["ShakeAnimation"];
                    sb.Begin();
                    return;
                }
            }

            Password = PasswordInput.Password;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
