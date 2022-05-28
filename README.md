[![](https://img.shields.io/badge/opencvsharp4-4.5.5-red)](https://www.nuget.org/packages/OpenCvSharp4/)

# AsciiConverter
### Description
Desktop application for translating raster images in Ascii graphics.

![AsceeScreen](https://github.com/KorablikDimak/AsciiConverter/raw/master/AsciiScreen.png)
### Settings
The application allows you to fine-tune the accuracy and size of the resulting "image". making it clearer if favored by increasing the number of characters and decreasing their font size. In the application, you can choose to invert the image so that it is displayed correctly on both light and dark backgrounds.
WidthOffset is responsible for the ratio of character width to pixels. By default, the height of the character is twice the width. So, to correctly display the image as characters, you must set the value to 2. Or value 1 if you want to save the converted text back to the image while maintaining the proportions of the original.

### Use this
No launch conditions are required. Just download the app and run it. Do not forget to save the resulting Ascii art and send it to your friends.
You can also save the text converted to asci back to the image and save it with a rather unusual effect.

## Download app
You can download a checked application for PC by the link [AsciiConverter](https://disk.yandex.ru/d/ZXK9zX-djvvdsQ)

### New (27.05.2022)
Added the ability to convert video to ascii. Significantly optimized conversion of images (or video frames) to ascii. You can pause/resume the video. All tasks run asynchronously on separate threads and are controlled through the `CancellationTokenSource`.
