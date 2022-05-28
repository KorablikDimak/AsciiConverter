using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AsciiConverter.Models;
using Microsoft.Win32;
using OpenCvSharp;

namespace AsciiConverter.ViewModels;

public class AsciiRenderModel : INotifyPropertyChanged
{
    private string _fileName;
    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged();
        }
    }
        
    private string _filePath;
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
        }
    }

    private string _asciiText;
    public string AsciiText
    {
        get => _asciiText;
        set
        {
            _asciiText = value;
            OnPropertyChanged();
        }
    }

    private string _imageSize;
    public string ImageSize
    {
        get => _imageSize;
        set
        {
            _imageSize = value;
            OnPropertyChanged();
        }
    }

    private string _fontSize;
    public string FontSize
    {
        get => _fontSize ?? "Font size: 5";
        set
        {
            _fontSize = $"Font size: {value}";
            OnPropertyChanged();
        } 
    }

    private FontSettingsModel _fontSettingsModel = new()
    {
        FontSize = 5,
        InvertInRedactor = true,
        InvertInSavedFile = false
    };
    public FontSettingsModel FontSettingsModel
    {
        get => _fontSettingsModel;
        set
        {
            FontSize = value.FontSize.ToString();
            _fontSettingsModel = value;
            OnPropertyChanged();
        }
    }

    private string _widthOffset;
    public string WidthOffset
    {
        get => _widthOffset ?? "Width offset: 2";
        set
        {
            _widthOffset = $"Width offset: {value}";
            OnPropertyChanged();
        } 
    }
        
    private string _asciiSize;
    public string AsciiSize
    {
        get => _asciiSize;
        set
        {
            _asciiSize = value;
            OnPropertyChanged();
        } 
    }

    private AsciiSettingsModel _asciiSettingsModel = new()
    {
        AsciiSize = 250,
        WidthOffset = 2
    };
    public AsciiSettingsModel AsciiSettingsModel
    {
        get => _asciiSettingsModel;
        set
        {
            _asciiSettingsModel = value;
            WidthOffset = value.WidthOffset.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged();
        }
    }

    private Command _choseImage;
    public Command ChoseImage => _choseImage ??= new Command(OpenFileDialog);

    private Command _changeFontSettings;
    public Command ChangeFontSettings => _changeFontSettings ??= new Command(OpenFontSettingsWindow);

    private Command _save;
    public Command Save => _save ??= new Command(SaveAsciiAsync);

    private Command _saveAs;
    public Command SaveAs => _saveAs ??= new Command(SaveAsAsciiAsync);

    private Command _exit;
    public Command Exit => _exit ??= new Command(ExitApp);

    private Command _asciiSettings;
    public Command AsciiSettings => _asciiSettings ??= new Command(OpenAsciiSettingsWindow);

    private Command _stopPlay;
    public Command StopPlay => _stopPlay ??= new Command(StopVideo);
    
    private Command _continuePlay;
    public Command ContinuePlay => _continuePlay ??= new Command(ContinueVideo);

    private readonly BitmapConverter _bitmapConverter = new();
    private delegate char[][] CreateAscii();

    private CancellationTokenSource _cancellationTokenSource = new();

    private async void OpenFileDialog(object o)
    {
        var fileDialog = new OpenFileDialog
        {
            Filter = "Images | *.bmp; *.png; *.jpg; *.jpeg; | Video | *.mp4; *.avi",
            Multiselect = false
        };
        bool? isChosen = fileDialog.ShowDialog();
        if (isChosen != true) return;
        
        _cancellationTokenSource.Cancel();
        await Task.Delay(1000);
        _isVideoPlaying = false;
        _cancellationTokenSource.Dispose();
        
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;
        
        FilePath = fileDialog.FileName;
        FileName = fileDialog.SafeFileName;
        string extension = Path.GetExtension(FileName);
        
        try
        {
            switch (extension)
            {
                case ".mp4":
                    Task.Run(() => OpenVideo(token), token);
                    break;
                case ".avi":
                    Task.Run(() => OpenVideo(token), token);
                    break;
                case ".bmp":
                    Task.Run(OpenBitmap);
                    break;
                case ".png":
                    Task.Run(OpenBitmap);
                    break;
                case ".jpg":
                    Task.Run(OpenBitmap);
                    break;
                case ".jpeg":
                    Task.Run(OpenBitmap);
                    break;
            }
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
    private bool _isVideoPlaying;

    private async void OpenVideo(CancellationToken token)
    {
        var capture = new VideoCapture(FilePath);
        _isVideoPlaying = true;
        var image = new Mat();
        int frameTime = 1000 / (int ) capture.Fps; // in milliseconds (for example 33ms when original fps is 30)
        ImageSize = $"{capture.FrameHeight}px x {capture.FrameWidth}px";

        while (capture.IsOpened())
        {
            if (token.IsCancellationRequested) return;
            if (_stopTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(-1, _continueTokenSource.Token);
                }
                catch (TaskCanceledException e)
                {
                    // ignored
                }
            }

            int timeStartFrame = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond; // this line does not affect performance
            if (!capture.Read(image)) return;

            _bitmapConverter.Bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
            if (AsciiSettingsModel.AsciiSize > 360) AsciiSettingsModel.AsciiSize = 360;
            _bitmapConverter.MaxWidth = AsciiSettingsModel.AsciiSize;
            _bitmapConverter.WidthOffset = AsciiSettingsModel.WidthOffset;

            if (FontSettingsModel.InvertInRedactor == true)
                SetAsciiText(_bitmapConverter.CreateInvertAscii);
            else SetAsciiText(_bitmapConverter.CreateAscii);

            int timeEndFrame = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond; // this line does not affect performance
            
            if ((frameTime - (timeEndFrame - timeStartFrame) > 1) & (frameTime - (timeEndFrame - timeStartFrame) < 100)) // needed to limit ascii fps
                Task.Delay(frameTime - (timeEndFrame - timeStartFrame)).Wait();
        }
    }

    private CancellationTokenSource _stopTokenSource = new();
    private CancellationTokenSource _continueTokenSource = new();

    private async void StopVideo(object o)
    {
        _stopTokenSource.Cancel();
        await Task.Delay(1000);
        _stopTokenSource.Dispose();
        _stopTokenSource = new CancellationTokenSource();
    }

    private async void ContinueVideo(object o)
    {
        _continueTokenSource.Cancel();
        await Task.Delay(1000);
        _continueTokenSource.Dispose();
        _continueTokenSource = new CancellationTokenSource();
    }

    private void OpenBitmap()
    { 
        _bitmapConverter.Bitmap = new Bitmap(FilePath);
        ImageSize = $"{_bitmapConverter.Bitmap.Height}px x {_bitmapConverter.Bitmap.Width}px";
        _bitmapConverter.MaxWidth = AsciiSettingsModel.AsciiSize;
        _bitmapConverter.WidthOffset = AsciiSettingsModel.WidthOffset;

        if (FontSettingsModel.InvertInRedactor == true) SetAsciiText(_bitmapConverter.CreateInvertAscii);
        else SetAsciiText(_bitmapConverter.CreateAscii);
    }
    
    private void SetAsciiText(CreateAscii createAscii)
    {
        char[][] ascii = createAscii.Invoke();
        AsciiSize = $"{ascii.Length}symbols x {ascii[0].Length}symbols";
        string[] asciiText = new string[ascii.Length];
        for (int y = 0; y < ascii.Length; y++)
        {
            asciiText[y] = new string(ascii[y]);
        }
        AsciiText = string.Join("\n", asciiText);
    }
    
    private string GetAsciiText(CreateAscii createAscii)
    {
        char[][] ascii = createAscii.Invoke();
        string[] asciiText = new string[ascii.Length];
        for (int y = 0; y < ascii.Length; y++)
        {
            asciiText[y] = new string(ascii[y]);
        }
        return string.Join("\n", asciiText);
    }

    private void OpenFontSettingsWindow(object o)
    {
        var fontSettings = new FontSettings(FontSettingsModel);
        if (fontSettings.ShowDialog() == true)
        {
            FontSettingsModel = fontSettings.FontSettingsModel;
            if (!_isVideoPlaying) Task.Run(OpenBitmap);
        }
    }

    private void OpenAsciiSettingsWindow(object o)
    {
        var asciiSettings = new AsciiSettings(AsciiSettingsModel);
        if (asciiSettings.ShowDialog() == true)
        {
            AsciiSettingsModel = asciiSettings.AsciiSettingsModel;
            if (!_isVideoPlaying) Task.Run(OpenBitmap);
        }
    }

    private async void SaveAsciiAsync(object o)
    {
        if (FileName == null) return;
        await SaveTextAsync($"{FileName}.txt");
    }

    private async void SaveAsAsciiAsync(object o)
    {
        StopVideo(null);
        if (AsciiText == null) return;
        var fileDialog = new SaveFileDialog {Filter = "txt file (*.txt)|*.txt|image file (*.png)|*.png"};
        bool? isChosen = fileDialog.ShowDialog();
        if (isChosen != true) return;
        string filePath = fileDialog.FileName;

        if (fileDialog.FilterIndex == 1)
        {
            await SaveTextAsync(filePath);
        }
        else if (fileDialog.FilterIndex == 2)
        {
            SaveImage(filePath);
        }
    }

    private void SaveImage(string path)
    {
        CreateAscii createAscii;
        if (FontSettingsModel.InvertInSavedFile == true) createAscii = _bitmapConverter.CreateInvertAscii;
        else createAscii = _bitmapConverter.CreateAscii;
        
        _bitmapConverter.WidthOffset = 1;
        Bitmap bitmap = _bitmapConverter.ConvertToBimap(createAscii.Invoke());
        _bitmapConverter.WidthOffset = 2;
        bitmap.Save(path);
    }

    private async Task SaveTextAsync(string path)
    {
        string text;
        if (FontSettingsModel.InvertInSavedFile == true) text = GetAsciiText(_bitmapConverter.CreateInvertAscii);
        else text = GetAsciiText(_bitmapConverter.CreateAscii);
            
        if (text == null) return;
        try
        {
            using var streamWriter = new StreamWriter(path, false, System.Text.Encoding.UTF8);
            await streamWriter.WriteAsync(text);
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    private void ExitApp(object o)
    {
        Application.Current.Shutdown();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}