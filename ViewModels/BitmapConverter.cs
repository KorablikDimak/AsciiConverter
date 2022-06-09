using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AsciiConverter.ViewModels;

public class BitmapConverter
{
    private byte _widthOffset;
    public byte WidthOffset
    {
        get => _widthOffset;
        set
        {
            _widthOffset = value;
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

    public Bitmap ScaledBitmap { get; private set; }
    private Bitmap _bitmap;
    public Bitmap Bitmap
    {
        get => _bitmap;
        set
        {
            _bitmap = value;
        }
    }

    // not use Reverse() for optimization
    private readonly char[] _symbols = {'@', '$', '&', '%', '#', '*', '+', '-', ';', ':', '_', ',', '.', ' '};
    private readonly char[] _invertSymbols = {' ', '.', ',', '_', ':', ';', '-', '+', '*', '#', '%', '&', '$', '@'};

    public string CreateAscii()
    {
        ConvertBitmapToMonochrome();
        return CreateAscii(_symbols);
    }

    public string CreateInvertAscii()
    {
        ConvertBitmapToMonochrome();
        return CreateAscii(_invertSymbols); 
    }
        
    private void ScaleBitmap()
    {
        int newHeight = Bitmap.Height / WidthOffset * MaxWidth / Bitmap.Width;
        if (Bitmap.Width > MaxWidth || Bitmap.Height > newHeight)
        {
            ScaledBitmap = new Bitmap(Bitmap, new Size(MaxWidth, newHeight));
        }
    }
        
    private unsafe void ConvertBitmapToMonochrome()
    {
        var rectangle = new Rectangle(0, 0, ScaledBitmap.Width, ScaledBitmap.Height);
        var bitmapData = ScaledBitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
        Span<byte> rgbValues = new Span<byte>(bitmapData.Scan0.ToPointer(), bytes);

        for (int counter = 0; counter < rgbValues.Length; counter += 4)
        {
            byte rjbSize = (byte) ((rgbValues[counter] + rgbValues[counter + 1] + rgbValues[counter + 2]) / 3);
            rgbValues[counter] = rjbSize;
        }
        
        ScaledBitmap.UnlockBits(bitmapData);
    }

    private unsafe string CreateAscii(char[] symbols)
    {
        var rectangle = new Rectangle(0, 0, ScaledBitmap.Width, ScaledBitmap.Height);
        var bitmapData = ScaledBitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
        Span<byte> rgbValues = new Span<byte>(bitmapData.Scan0.ToPointer(), bytes);
        
        Span<char> ascii = stackalloc char[ScaledBitmap.Height * (ScaledBitmap.Width + 1)];
        for (int counter = 0, i = 0, x = 1; counter < rgbValues.Length; counter += 4, i++, x++)
        {
            ascii[i] = symbols[(int)((float)rgbValues[counter] / 255 * (_symbols.Length - 1))];
            if (x == ScaledBitmap.Width)
            {
                i++;
                ascii[i] = '\n';
                x = 0;
            }
        }
        
        ScaledBitmap.UnlockBits(bitmapData);
        return new string(ascii.ToArray());
    }

    private const int CharToPixelSize = 5;
        
    public unsafe Bitmap ConvertToBimap(string ascii)
    {
        if (ascii == null) return new Bitmap(0, 0);
        var bitmap = new Bitmap(ScaledBitmap.Width * CharToPixelSize, ScaledBitmap.Height * CharToPixelSize);

        var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
        Span<byte> rgbValues = new Span<byte>(bitmapData.Scan0.ToPointer(), bytes);
        
        for (int i = 0, y = 0, x = 0; i < ascii.Length; i++, x++)
        {
            if (x == ScaledBitmap.Width)
            {
                y++;
                x = -1;
                continue;
            }
            // one char to CharToPixelSize pixels
            byte[,] pixels = FromAsciiToPixels(ascii[i]);

            for (int k = 0; k < CharToPixelSize; k++) 
            {
                for (int l = 0; l < CharToPixelSize; l++) 
                {
                    Color color = pixels[k, l] switch
                    {
                        0 => Color.White,
                        1 => Color.Gray,
                        2 => Color.Black,
                        _ => Color.White
                    };
                    int pixelPosition = x * CharToPixelSize + k + (y * CharToPixelSize + l) * bitmap.Width;
                    
                    rgbValues[pixelPosition * 4] = color.R;
                    rgbValues[pixelPosition * 4 + 1] = color.G;
                    rgbValues[pixelPosition * 4 + 2] = color.B;
                    rgbValues[pixelPosition * 4 + 3] = color.A;
                }
            }
        }
        
        bitmap.UnlockBits(bitmapData);
        return bitmap;
    }

    private byte[,] FromAsciiToPixels(char symbol)
    {
        var pixels = new byte[CharToPixelSize, CharToPixelSize];
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