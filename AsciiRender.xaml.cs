using AsciiConverter.ViewModels;

namespace AsciiConverter
{
    public partial class AsciiRender
    {
        public AsciiRender()
        {
            InitializeComponent();
            DataContext = new AsciiRenderModel();
        }
    }
}