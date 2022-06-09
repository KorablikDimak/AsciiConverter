[![](https://img.shields.io/badge/opencvsharp4-4.5.5-red)](https://www.nuget.org/packages/OpenCvSharp4/)

# AsciiConverter
### Description
Desktop application for translating raster images in Ascii graphics. The application uses unsafe code, calculations are carried out in separate threads with the ability to pause and resume video playback. The application is built on an architectural pattern MVVM.

![AsceeScreen](https://github.com/KorablikDimak/AsciiConverter/raw/master/AsciiScreen.png)
### Settings
The application allows you to fine-tune the accuracy and size of the resulting "image". making it clearer if favored by increasing the number of characters and decreasing their font size. In the application, you can choose to invert the image so that it is displayed correctly on both light and dark backgrounds.

### Use this
No launch conditions are required. Just download the app and run it. Do not forget to save the resulting Ascii art and send it to your friends.
You can also save the text converted to asci back to the image and save it with a rather unusual effect.

## Download app
You can download a checked application for PC by the link [AsciiConverter](https://disk.yandex.ru/d/w-BS3Hyixi-2Cg)

### New (27.05.2022)
Added the ability to convert video to ascii. Significantly optimized conversion of images (or video frames) to ascii. You can pause/resume the video. All tasks run asynchronously on separate threads and are controlled through the `CancellationTokenSource`.
