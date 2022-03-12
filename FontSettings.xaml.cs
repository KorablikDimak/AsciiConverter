using System.Windows;
using AsciiConverter.Models;

namespace AsciiConverter
{
    public partial class FontSettings : Window
    {
        public FontSettingsModel FontSettingsModel { get; set; }

        public FontSettings(FontSettingsModel fontSettingsModel)
        {
            InitializeComponent();
            FontSettingsModel = fontSettingsModel;
            FontSize.Text = fontSettingsModel.FontSize.ToString();
            InvertInRedactor.IsChecked = fontSettingsModel.InvertInRedactor;
            InvertInSavedFile.IsChecked = fontSettingsModel.InvertInSavedFile;
        }

        private void AcceptSettings()
        {
            bool success  = int.TryParse(FontSize.Text, out int fontSize);
            if (!success)
            {
                fontSize = 5;
            }
            if (fontSize < 1)
            {
                fontSize = 1;
            }
            else if (fontSize > 50)
            {
                fontSize = 50;
            }
            FontSettingsModel.FontSize = fontSize;
            FontSettingsModel.InvertInRedactor = InvertInRedactor.IsChecked;
            FontSettingsModel.InvertInSavedFile = InvertInSavedFile.IsChecked;
        }

        private void Cancel(object o, RoutedEventArgs routedEventArgs)
        {
            DialogResult = false;
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            AcceptSettings();
            DialogResult = true;
        }
    }
}