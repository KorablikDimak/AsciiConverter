using System.Globalization;
using System.Windows;
using AsciiConverter.Models;

namespace AsciiConverter
{
    public partial class AsciiSettings : Window
    {
        public AsciiSettingsModel AsciiSettingsModel { get; set; }
        
        public AsciiSettings(AsciiSettingsModel asciiSettingsModel)
        {
            InitializeComponent();
            AsciiSettingsModel = asciiSettingsModel;
            AsciiSize.Text = asciiSettingsModel.AsciiSize.ToString();
            WidthOffset.Text = asciiSettingsModel.WidthOffset.ToString(CultureInfo.InvariantCulture);
        }

        private void AcceptSettings()
        {
            bool success  = int.TryParse(AsciiSize.Text, out int asciiSize);
            if (!success)
            {
                asciiSize = 250;
            }
            if (asciiSize < 1)
            {
                asciiSize = 1;
            }
            else if (asciiSize > 999)
            {
                asciiSize = 999;
            }
            success  = float.TryParse(WidthOffset.Text, out float widthOffset);
            if (!success)
            {
                widthOffset = 2;
            }
            if (widthOffset < 0.1f)
            {
                widthOffset = 0.1f;
            }
            else if (widthOffset > 10)
            {
                widthOffset = 10;
            }

            AsciiSettingsModel.AsciiSize = asciiSize;
            AsciiSettingsModel.WidthOffset = widthOffset;
        }

        private void Cancel(object sender, RoutedEventArgs e)
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