namespace AsciiConverter.Models;

public class FontSettingsModel
{
    public int FontSize { get; set; }
    public int AsciiSize { get; set; }
    public bool? InvertInRedactor { get; set; }
    public bool? InvertInSavedFile { get; set; }
}