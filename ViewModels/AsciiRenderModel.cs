using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AsciiConverter.Models;
using Microsoft.Win32;
using OpenCvSharp;

namespace AsciiConverter.ViewModels
{
    public class AsciiRenderModel : INotifyPropertyChanged
    {
        private string _fileName;
        public string FileName
        {
            get => _fileName ?? "";
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }
        
        private string _filePath;
        public string FilePath
        {
            get => _filePath ?? "";
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
            get => _imageSize ?? "";
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
            get => _asciiSize ?? "";
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
                if (_bitmapConverter == null) return;
                OpenBitmap();
                RenderAscii(null);
            }
        }

        private Command _choseImage;
        public Command ChoseImage => _choseImage ??= new Command(OpenFileDialog);

        private Command _createImage;
        public Command CreateImage => _createImage ??= new Command(RenderAscii);

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

        private BitmapConverter _bitmapConverter;
        private delegate char[][] CreateAscii();

        private void OpenFileDialog(object o)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Images | *.bmp; *.png; *.jpg; *.jpeg; | Video | *.mp4; *.avi"
            };
            bool? isChosen = fileDialog.ShowDialog();
            if (isChosen != true) return;
            FilePath = fileDialog.FileName;
            FileName = fileDialog.SafeFileName;
            OpenVideo();
        }

        private void OpenVideo()
        {
            var capture = new VideoCapture(FilePath);
            var image = new Mat();

            while (capture.IsOpened())
            {
                capture.Read(image);
                if (image.Empty())
                {
                    break;
                }

                var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                var rectangle = new Rectangle(0, 0, image.Width, image.Height);
                var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bitmapData.Scan0 = image.Data;
                bitmap.UnlockBits(bitmapData);
                
                if (_bitmapConverter == null) return;
                CreateAscii createAscii;
                if (FontSettingsModel.InvertInRedactor == true) createAscii = _bitmapConverter.CreateInvertAscii;
                else createAscii = _bitmapConverter.CreateAscii;
                AsciiText = CreateAsciiString(createAscii);
            }
        }

        private void OpenBitmap()
        {
            var bitmap = new Bitmap(FilePath);
            _bitmapConverter = new BitmapConverter(bitmap)
            {
                MaxWidth = AsciiSettingsModel.AsciiSize, 
                WidthOffset = AsciiSettingsModel.WidthOffset
            };
            ImageSize = $"{bitmap.Height}px x {bitmap.Width}px";
        }

        private void RenderAscii(object o)
        {
            if (_bitmapConverter == null) return;
            CreateAscii createAscii;
            if (FontSettingsModel.InvertInRedactor == true) createAscii = _bitmapConverter.CreateInvertAscii;
            else createAscii = _bitmapConverter.CreateAscii;
            AsciiText = CreateAsciiString(createAscii);
        }

        private string CreateAsciiString(CreateAscii createAscii)
        {
            char[][] ascii = createAscii.Invoke();
            AsciiSize = $"{ascii.Length}symbols x {ascii[0].Length}symbols";
            string[] asciiText = new string[ascii.Length];
            for (int y = 0; y < ascii.Length; y++)
            {
                asciiText[y] = new string(ascii[y]);
            }
            string text = string.Join("\n", asciiText);
            return text;
        }

        private void OpenFontSettingsWindow(object o)
        {
            var fontSettings = new FontSettings(FontSettingsModel);
            if (fontSettings.ShowDialog() == true) FontSettingsModel = fontSettings.FontSettingsModel;
        }

        private void OpenAsciiSettingsWindow(object o)
        {
            var asciiSettings = new AsciiSettings(AsciiSettingsModel);
            if (asciiSettings.ShowDialog() == true) AsciiSettingsModel = asciiSettings.AsciiSettingsModel;
        }

        private async void SaveAsciiAsync(object o)
        {
            if (FileName == null) return;
            await SaveTextAsync($"{FileName}.txt");
        }

        private async void SaveAsAsciiAsync(object o)
        {
            if (AsciiText == null) return;
            var fileDialog = new SaveFileDialog {Filter = "txt file (*.txt)|*.txt|image file (*.png)|*.png"};
            bool? isChosen = fileDialog.ShowDialog();
            if (isChosen != true) return;
            string filePath = fileDialog.FileName;

            if (fileDialog.FilterIndex < 2)
            {
                await SaveTextAsync(filePath);
            }

            SaveImageAsync(filePath);
        }

        private void SaveImageAsync(string path)
        {
            CreateAscii createAscii;
            if (FontSettingsModel.InvertInSavedFile == true) createAscii = _bitmapConverter.CreateInvertAscii;
            else createAscii = _bitmapConverter.CreateAscii;

            Bitmap bitmap = _bitmapConverter.ConvertToBimap(createAscii.Invoke());
            bitmap.Save(path);
        }

        private async Task SaveTextAsync(string path)
        {
            CreateAscii createAscii;
            if (FontSettingsModel.InvertInSavedFile == true) createAscii = _bitmapConverter.CreateInvertAscii;
            else createAscii = _bitmapConverter.CreateAscii;
            string text = CreateAsciiString(createAscii);
            
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
}