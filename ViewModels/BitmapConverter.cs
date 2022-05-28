using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AsciiConverter.ViewModels;

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

            ScaleBitmap();
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

    // not use Reverse() for optimization
    private readonly char[] _symbols = {'@', '$', '&', '%', '#', '*', '+', '-', ';', ':', '_', ',', '.', ' '};
    private readonly char[] _invertSymbols = {' ', '.', ',', '_', ':', ';', '-', '+', '*', '#', '%', '&', '$', '@'};
    private char[][] _ascii;
    private int[][] _asciiIndexes;

    public char[][] CreateAscii()
    {
        if (_asciiIndexes != null) 
        {
            ConvertToAscii(_symbols);
            return _ascii;
        }
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
        var rectangle = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
        var bitmapData = Bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        IntPtr ptr = bitmapData.Scan0;
            
        int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
        byte[] rgbValues = new byte[bytes];
        Marshal.Copy(ptr, rgbValues, 0, bytes);

        for (int counter = 0; counter < rgbValues.Length; counter += 4)
        {
            byte rjbSize = (byte) ((rgbValues[counter] + rgbValues[counter + 1] + rgbValues[counter + 2]) / 3);
            rgbValues[counter] = rjbSize;
            rgbValues[counter + 1] = rjbSize;
            rgbValues[counter + 2] = rjbSize;
        }
            
        Marshal.Copy(rgbValues, 0, ptr, bytes);
        Bitmap.UnlockBits(bitmapData);
    }

    private void CreateAsciiIndexes()
    {
        var rectangle = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
        var bitmapData = Bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        IntPtr ptr = bitmapData.Scan0;
            
        int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
        byte[] rgbValues = new byte[bytes];
        Marshal.Copy(ptr, rgbValues, 0, bytes);

        _asciiIndexes = new int[Bitmap.Height][];
        _asciiIndexes[0] = new int[Bitmap.Width];
        for (int counter = 0, x = 0, y = 0; counter < rgbValues.Length; counter += 4, x++)
        {
            if (x == Bitmap.Width)
            {
                x = 0;
                y++;
                _asciiIndexes[y] = new int[Bitmap.Width];
            }

            _asciiIndexes[y][x] = (int)((float)rgbValues[counter] / 255 * (_symbols.Length - 1));
        }
            
        Marshal.Copy(rgbValues, 0, ptr, bytes);
        Bitmap.UnlockBits(bitmapData);
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

        var color = Color.White;
            
        for (int i = 0; i < ascii.Length; i++)
        {
            for (int j = 0; j < ascii[i].Length; j++)
            {
                // one char to CharToPixelSize pixels
                int[,] pixels = FromAsciiToPixels(ascii[i][j]);

                for (int k = 0; k < CharToPixelSize; k++)
                {
                    for (int l = 0; l < CharToPixelSize; l++)
                    {
                        color = pixels[k, l] switch
                        {
                            0 => Color.White,
                            1 => Color.Gray,
                            2 => Color.Black,
                            _ => color
                        };
                        // can be optimized
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