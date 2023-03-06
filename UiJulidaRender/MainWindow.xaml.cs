using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static UiJulidaRender.ColorGradients;

namespace UiJulidaRender {

    public partial class MainWindow : Window {

        public const int H = 1080;
        public const int W = (int) (H * (16.0 / 9));
        public readonly WriteableBitmap RenderBitmap;
        private readonly MultiCoreRenderer Renderer;
        public ColorGradient ColorGradient { get; private set; }

        public MainWindow() {
            InitializeComponent();

            WindowState = WindowState.Maximized;

            RenderBitmap = new(W, H, 96, 96, PixelFormats.Rgb24, null);
            MainCanvas.Children.Add(new Image() {
                Source = RenderBitmap,
            });

            foreach (Image image in ColorGradientComboBox.Items) {

                WriteableBitmap colorBmp = BitmapFactory.New((int) image.Width, (int) image.Height);
                image.Source = colorBmp;

                for (int x = 0; x < colorBmp.PixelWidth; x++) {

                    ColorGradient gradient;

                    if (image.Name == "RainbowGradientImg")
                        gradient = RainbowGradient;
                    else if (image.Name == "GrayScaleImg")
                        gradient = GrayScale;
                    else
                        throw new Exception();

                    FastColor c = gradient.Lerp(x / image.Width);

                    for (int y = 0; y < colorBmp.PixelHeight; y++)
                        colorBmp.SetPixel(x, y, c.ToColor());
                }
            }

            Renderer = new(Environment.ProcessorCount, this);
            Renderer.Render();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Renderer?.Render();
        }

        private void SmoothIterationCheckBox_Clicked(object sender, RoutedEventArgs e) {
            Renderer?.Render();
        }

        private void ColorGradientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (ColorGradientComboBox.SelectedIndex == 0)
                ColorGradient = RainbowGradient;
            else if (ColorGradientComboBox.SelectedIndex == 1)
                ColorGradient = GrayScale;
            else
                throw new Exception();

            Renderer?.Render();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new() {
                DefaultExt = "png",
                Filter = "png files (*.png)|*.png|All files (*.*)|*.*"
            };
            dialog.ShowDialog();
            if (dialog.FileName != "")
                RenderBitmap.Save(dialog.FileName);
        }
    }
}
