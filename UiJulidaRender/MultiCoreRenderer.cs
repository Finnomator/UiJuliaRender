using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UiJulidaRender {
    public class MultiCoreRenderer {

        private readonly Thread[] Threads;
        private readonly WriteableBitmap RenderBitmap;
        private readonly MainWindow MainWindow;
        private long LastRenderTimeMs;
        private readonly Stopwatch sw = new();
        private int MaxIterations;
        private bool SmoothIteration;
        private Vector Constant;
        private readonly byte[] ImgData;
        private int RunningThreads;
        private ColorGradients.ColorGradient ColorGradient;
        private double Zoom;

        public MultiCoreRenderer(int threads, MainWindow mainWindow) {
            MainWindow = mainWindow;
            Threads = new Thread[threads];
            RenderBitmap = MainWindow.RenderBitmap;
            ImgData = new byte[RenderBitmap.PixelHeight * RenderBitmap.PixelWidth * 3];
        }

        public void Render() {

            sw.Restart();

            MaxIterations = (int) MainWindow.MaxIterationsSlider.Value;
            Zoom = MainWindow.ZoomSlider.Value;
            ColorGradient = MainWindow.ColorGradient;
            Constant = new(MainWindow.ConstantXSlider.Value, -MainWindow.ConstantYSlider.Value);
            SmoothIteration = (bool) MainWindow.SmoothIterCheckBox.IsChecked!;

            RunningThreads = 0;
            for (int i = 0; i < Threads.Length; ++i) {
                Threads[i] = new(RenderFractal) {
                    IsBackground = true,
                };
                Threads[i].Start();
            }

            for (int i = 0; i < Threads.Length; ++i)
                Threads[i].Join();

            RenderBitmap.WritePixels(new(0, 0, RenderBitmap.PixelWidth, RenderBitmap.PixelHeight), ImgData, RenderBitmap.Stride(), 0);

            sw.Stop();
            LastRenderTimeMs = sw.ElapsedMilliseconds;
            MainWindow.RenderTimeTextBlock.Text = LastRenderTimeMs.ToString();
        }

        private void RenderFractal() {

            int threadIndex = RunningThreads;
            RunningThreads++;

            int h = MainWindow.H;
            int w = MainWindow.W;

            double scale = 2.0 / h / Zoom;

            int yStart = (int) ((double) h / Threads.Length * threadIndex);
            int yEnd = yStart + h / Threads.Length;

            for (int y = yStart; y < yEnd; ++y) {

                double py = (y - h / 2) * scale;

                for (int x = 0; x < w / 2; ++x) {
                    double px = (x - w / 2) * scale;

                    Point p = new(px, py);

                    double iterations = SmoothIteration
                        ? ComputeIterationsSmooth(ref p, ref Constant, MaxIterations)
                        : ComputeIterations(ref p, ref Constant, MaxIterations);

                    FastColor color = ColorGradient.Lerp(iterations / MaxIterations);

                    // left
                    int i = (x + y * w) * 3;
                    ImgData[i] = color.R;
                    ImgData[i + 1] = color.G;
                    ImgData[i + 2] = color.B;

                    // right
                    i = (w - x - 1 + (h - y - 1) * w) * 3;
                    ImgData[i] = color.R;
                    ImgData[i + 1] = color.G;
                    ImgData[i + 2] = color.B;
                }
            }
        }

        private static void ComputeNext(ref Point current, ref Vector constant) {
            double zr = current.X * current.X - current.Y * current.Y;
            double zi = 2.0 * current.X * current.Y;
            current.X = zr + constant.X;
            current.Y = zi + constant.Y;
        }

        private static double ModSqr(ref Point z) => z.X * z.X + z.Y * z.Y;

        private static int ComputeIterations(ref Point z0, ref Vector constant, int maxIteration) {
            int i = 0;
            while (i < maxIteration && ModSqr(ref z0) < 4.0) {
                ComputeNext(ref z0, ref constant);
                ++i;
            }
            return i;
        }

        private static double ComputeIterationsSmooth(ref Point z0, ref Vector constant, int maxIteration) {
            int i = 0;
            while (i < maxIteration && ModSqr(ref z0) < 4.0) {
                ComputeNext(ref z0, ref constant);
                ++i;
            }
            return i - Math.Log2(Math.Max(1, Math.Log2(Math.Sqrt(ModSqr(ref z0)))));
        }
    }
}
