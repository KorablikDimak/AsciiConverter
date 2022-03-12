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

        private const int CharToPixelSize = 5;
        
        public Bitmap ConvertToBimap(char[][] ascii)
        {
            if (ascii == null) return new Bitmap(0, 0);
            var bitmap = new Bitmap(ascii[0].Length * CharToPixelSize, ascii.Length * CharToPixelSize);

            Color color = Color.White;
            
            for (int i = 0; i < ascii.Length; i++)
            {
                for (int j = 0; j < ascii[i].Length; j++)
                {
                    int[,] pixels = FromAsciiToPixels(ascii[i][j]);

                    for (int k = 0; k < CharToPixelSize; k++)
                    {
                        for (int l = 0; l < CharToPixelSize; l++)
                        {
                            switch (pixels[k, l])
                            {
                                case 0:
                                    color = Color.White;
                                    break;
                                case 1:
                                    color = Color.Gray;
                                    break;
                                case 2:
                                    color = Color.Black;
                                    break;
                            }
                            
                            bitmap.SetPixel(j * CharToPixelSize + k, i * CharToPixelSize + l, color);
                        }
                    }
                }
            }
            
            return bitmap;
        }

        private int[,] FromAsciiToPixels(char symbol)
        {
            var pixels = new int[CharToPixelSize, CharToPixelSize];
            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    pixels[i, j] = 0;
                }
            }
            
            switch (symbol)
            {
                case '.':
                    pixels[2, 3] = 2;
                    break;
                case ',':
                    pixels[2, 3] = 2;
                    pixels[2, 4] = 1;
                    pixels[1, 4] = 2;
                    break;
                case '_':
                    pixels[1, 3] = 2;
                    pixels[2, 3] = 2;
                    pixels[3, 3] = 2;
                    break;
                case ':':
                    pixels[2, 1] = 2;
                    pixels[2, 3] = 2;
                    break;
                case ';':
                    pixels[2, 1] = 2;
                    pixels[2, 3] = 2;
                    pixels[2, 4] = 1;
                    pixels[1, 4] = 2;
                    break;
                case '-':
                    pixels[1, 2] = 2;
                    pixels[2, 2] = 2;
                    pixels[3, 2] = 2;
                    break;
                case '+':
                    pixels[2, 0] = 1;
                    pixels[2, 1] = 2;
                    pixels[2, 3] = 2;
                    pixels[2, 4] = 1;
                    pixels[0, 2] = 1;
                    pixels[1, 2] = 2;
                    pixels[2, 2] = 2;
                    pixels[3, 2] = 2;
                    pixels[4, 2] = 1;
                    break;
                case '*':
                    pixels[0, 0] = 2;
                    pixels[1, 1] = 2;
                    pixels[2, 2] = 2;
                    pixels[3, 3] = 2;
                    pixels[4, 4] = 2;
                    pixels[2, 0] = 2;
                    pixels[2, 1] = 2;
                    pixels[0, 4] = 2;
                    pixels[1, 3] = 2;
                    pixels[3, 1] = 2;
                    pixels[4, 0] = 2;
                    break;
                case '#':
                    pixels[1, 0] = 2;
                    pixels[1, 1] = 2;
                    pixels[1, 2] = 2;
                    pixels[1, 3] = 2;
                    pixels[1, 4] = 2;
                    pixels[3, 0] = 2;
                    pixels[3, 1] = 2;
                    pixels[3, 2] = 2;
                    pixels[3, 4] = 2;
                    pixels[0, 1] = 2;
                    pixels[2, 1] = 2;
                    pixels[4, 1] = 2;
                    pixels[0, 3] = 2;
                    pixels[2, 3] = 2;
                    pixels[4, 3] = 2;
                    break;
                case '%':
                    pixels[0, 4] = 2;
                    pixels[1, 3] = 2;
                    pixels[2, 2] = 2;
                    pixels[3, 1] = 2;
                    pixels[4, 0] = 2;
                    pixels[1, 0] = 2;
                    pixels[2, 1] = 2;
                    pixels[1, 2] = 2;
                    pixels[0, 1] = 2;
                    pixels[0, 0] = 1;
                    pixels[2, 0] = 1;
                    pixels[0, 2] = 1;
                    pixels[3, 2] = 2;
                    pixels[4, 3] = 2;
                    pixels[3, 4] = 2;
                    pixels[2, 3] = 2;
                    pixels[4, 2] = 1;
                    pixels[4, 4] = 1;
                    pixels[2, 4] = 1;
                    break;
                case '&':
                    pixels[4, 1] = 2;
                    pixels[3, 0] = 2;
                    pixels[2, 1] = 2;
                    pixels[2, 0] = 1;
                    pixels[1, 1] = 1;
                    pixels[1, 2] = 1;
                    pixels[2, 2] = 2;
                    pixels[3, 3] = 2;
                    pixels[4, 4] = 2;
                    pixels[2, 4] = 2;
                    pixels[1, 4] = 2;
                    pixels[1, 3] = 2;
                    break;
                case '$':
                    pixels[3, 1] = 2;
                    pixels[3, 0] = 1;
                    pixels[2, 0] = 2;
                    pixels[1, 0] = 1;
                    pixels[1, 1] = 2;
                    pixels[2, 1] = 2;
                    pixels[2, 2] = 2;
                    pixels[2, 3] = 2;
                    pixels[2, 4] = 2;
                    pixels[1, 4] = 2;
                    pixels[3, 4] = 2;
                    pixels[3, 3] = 1;
                    break;
                case '@':
                    pixels[1, 1] = 2;
                    pixels[1, 2] = 2;
                    pixels[1, 3] = 1;
                    pixels[2, 3] = 2;
                    pixels[3, 3] = 1;
                    pixels[4, 2] = 2;
                    pixels[4, 1] = 1;
                    pixels[3, 0] = 1;
                    pixels[2, 0] = 2;
                    pixels[1, 0] = 2;
                    pixels[0, 1] = 2;
                    pixels[0, 2] = 2;
                    pixels[0, 3] = 2;
                    pixels[0, 4] = 1;
                    pixels[1, 4] = 2;
                    pixels[2, 4] = 1;
                    break;
            }

            return pixels;
        }
    }
}