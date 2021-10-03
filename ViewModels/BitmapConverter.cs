using System.Drawing;

namespace AsciiConverter.ViewModels
{
    public class BitmapConverter
    {
        private float _widthOffset;
        public float WidthOffset
        {
            get => _widthOffset;
            set
            {
                if (value < 0.1f) _widthOffset = 0.1f;
                else if (value > 10) _widthOffset = 10;
                else _widthOffset = value;
            }
        }

        private int _maxWidth;
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (value < 1) _maxWidth = 1;
                else if (value > Bitmap.Width) _maxWidth = Bitmap.Width;
                else _maxWidth = value;
            }
        }

        private Bitmap _bitmap;
        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _asciiIndexes = null;
                _bitmap = value;
            }
        }

        private readonly char[] _symbols = {'@', '$', '&', '%', '#', '*', '+', '-', ';', ':', '_', ',', '.'};
        private readonly char[] _invertSymbols = {'.', ',', '_', ':', ';', '-', '+', '*', '#', '%', '&', '$', '@'};
        private char[][] _ascii;
        private int[][] _asciiIndexes;

        public BitmapConverter(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public char[][] CreateAscii()
        {
            if (_asciiIndexes != null) 
            {
                ConvertToAscii(_symbols);
                return _ascii;
            }
            ScaleBitmap();
            ConvertBitmapToMonochrome();
            CreateAsciiIndexes();
            ConvertToAscii(_symbols);
            return _ascii;
        }

        public char[][] CreateInvertAscii()
        {
            if (_asciiIndexes != null)
            {
                ConvertToAscii(_invertSymbols);
                return _ascii;
            }
            ScaleBitmap();
            ConvertBitmapToMonochrome();
            CreateAsciiIndexes();
            ConvertToAscii(_invertSymbols);
            return _ascii;
        }
        
        private void ScaleBitmap()
        {
            int newHeight = (int) (Bitmap.Height / WidthOffset * MaxWidth / Bitmap.Width);
            if (Bitmap.Width > MaxWidth || Bitmap.Height > newHeight)
            {
                Bitmap = new Bitmap(Bitmap, new Size(MaxWidth, newHeight));
            }
        }
        
        private void ConvertBitmapToMonochrome()
        {
            for (int y = 0; y < Bitmap.Height; y++)
            {
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    Color pixel = Bitmap.GetPixel(x, y);
                    int rjbSize = (pixel.R + pixel.G + pixel.B) / 3;
                    Bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, rjbSize, rjbSize, rjbSize));
                }
            }
        }

        private void CreateAsciiIndexes()
        {
            _asciiIndexes = new int[Bitmap.Height][];
            for (int y = 0; y < Bitmap.Height; y++)
            {
                _asciiIndexes[y] = new int[Bitmap.Width];
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    _asciiIndexes[y][x] = 
                        MapArrays(Bitmap.GetPixel(x, y).R, 0, 255, 0, _symbols.Length - 1);
                }
            }
        }

        private static int MapArrays(int valueToMap, int minValue1, int maxValue1, int minValue2, int maxValue2)
        {
            return (int) ((float) (valueToMap - minValue1) / (maxValue1 - minValue1) * (maxValue2 - minValue2) + minValue2);
        }

        private void ConvertToAscii(char[] symbols)
        {
            _ascii = new char[Bitmap.Height][];
            for (int y = 0; y < Bitmap.Height; y++)
            {
                _ascii[y] = new char[Bitmap.Width];
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    _ascii[y][x] = symbols[_asciiIndexes[y][x]];
                }
            }
        }
    }
}