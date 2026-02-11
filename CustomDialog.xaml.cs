using System.Windows;
using System.Windows.Media;

namespace SystemCleaner
{
    /// <summary>
    /// A custom dialog window that matches the application's design language.
    /// </summary>
    public partial class CustomDialog : Window
    {
        public CustomDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();
            Title = title;
            MessageTextBlock.Text = message;
            IconPath.Visibility = Visibility.Collapsed;

            // Set icon based on MessageBoxImage
            switch (icon)
            {
                case MessageBoxImage.Information:
                    IconPath.Visibility = Visibility.Visible;
                    IconPath.Data = Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z");
                    IconPath.Fill = System.Windows.Media.Brushes.Green;
                    break;
                case MessageBoxImage.Warning:
                    IconPath.Visibility = Visibility.Visible;
                    IconPath.Data = Geometry.Parse("M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16");
                    IconPath.Fill = System.Windows.Media.Brushes.Orange;
                    break;
                case MessageBoxImage.Error:
                    IconPath.Visibility = Visibility.Visible;
                    IconPath.Data = Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z");
                    IconPath.Fill = System.Windows.Media.Brushes.Red;
                    break;
                case MessageBoxImage.Question:
                    IconPath.Visibility = Visibility.Visible;
                    IconPath.Data = Geometry.Parse("M10,19H13V22H10V19M12,2C17.35,2.22 19.68,7.62 16.5,11.67C15.67,12.67 14.33,13.33 13.67,14.17C13,15 13,16 13,17H10C10,15.33 10,13.92 10.67,12.92C11.33,11.92 12.67,11.33 13.5,10.67C15.92,8.43 15.32,5.26 12,5A3,3 0 0,0 9,8H6A6,6 0 0,1 12,2Z");
                    IconPath.Fill = System.Windows.Media.Brushes.DodgerBlue;
                    break;
            }

            // Configure buttons based on MessageBoxButton
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            Close();
        }

        /// <summary>
        /// Shows a custom dialog with the application's design language.
        /// </summary>
        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            var dialog = new CustomDialog(title, message, buttons, icon);
            dialog.Owner = Application.Current.MainWindow;
            var result = dialog.ShowDialog();

            if (buttons == MessageBoxButton.OK || buttons == MessageBoxButton.OKCancel)
            {
                return result == true ? MessageBoxResult.OK : MessageBoxResult.Cancel;
            }
            else // YesNo or YesNoCancel
            {
                if (result == true) return MessageBoxResult.Yes;
                if (result == false) return MessageBoxResult.No;
                return MessageBoxResult.Cancel;
            }
        }
    }
}
