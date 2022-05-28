using System.Windows;
using AsciiConverter.Models;

namespace AsciiConverter;

public partial class FontSettings : Window
{
    public FontSettingsModel FontSettingsModel { get; set; }

    public FontSettings(FontSettingsModel fontSettingsModel)
    {
        InitializeComponent();
        FontSettingsModel = fontSettingsModel;
        AsciiSize.Text = fontSettingsModel.AsciiSize.ToString();
        FontSize.Text = fontSettingsModel.FontSize.ToString();
        InvertInRedactor.IsChecked = fontSettingsModel.InvertInRedactor;
        InvertInSavedFile.IsChecked = fontSettingsModel.InvertInSavedFile;
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
        else if (asciiSize > 500)
        {
            asciiSize = 500;
        }
        success  = int.TryParse(FontSize.Text, out int fontSize);
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
        FontSettingsModel.AsciiSize = asciiSize;
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