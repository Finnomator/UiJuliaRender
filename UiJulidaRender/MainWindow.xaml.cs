using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UiJulidaRender {

    public partial class MainWindow : Window {

        public readonly int W;
        public readonly int H;
        public readonly WriteableBitmap RenderBitmap;

        private readonly MultiCoreRenderer Renderer;
        private bool BlockValueChange;

        public MainWindow() {
            InitializeComponent();

            W = (int) Width;
            H = (int) Height;

            RenderBitmap = BitmapFactory.New(W, H);
            MainCanvas.Children.Add(new Image() {
                Source = RenderBitmap,
            });

            Renderer = new(Environment.ProcessorCount, this);
            Renderer.Render();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (!BlockValueChange)
                Renderer?.Render();
        }

        private void SmoothIterationCheckBox_Clicked(object sender, RoutedEventArgs e) {
            Renderer?.Render();
        }
    }
}
