using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace UiJulidaRender {
    public class MultiCoreRenderer {

        private readonly Thread[] Threads;
        private readonly WriteableBitmap[] RenderBitmaps;
        private readonly MainWindow MainWindow;
        private long LastRenderTimeMs;
        private readonly Stopwatch sw = new();
        private byte MaxIterations;
        private bool SmoothIteration;
        private Vector Constant;
        private readonly byte[][] ImgDatas;
        private int RunningThreads;

        public MultiCoreRenderer(int threads, MainWindow mainWindow) {
            Threads = new Thread[threads];
            RenderBitmaps = new WriteableBitmap[threads];
            ImgDatas = new byte[threads][];
            MainWindow = mainWindow;

            WriteableBitmap bitmap = MainWindow.RenderBitmap;
            int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;

            for (int i = 0; i < threads; ++i)
                ImgDatas[i] = new byte[stride * (bitmap.PixelHeight / Threads.Length)];
        }

        public void Render() {

            sw.Restart();

            MaxIterations = (byte) MainWindow.MaxIterationsSlider.Value;

            Constant = new(MainWindow.ConstantXSlider.Value, MainWindow.ConstantYSlider.Value);
            SmoothIteration = (bool) MainWindow.SmoothIterCheckBox.IsChecked!;

            RunningThreads = 0;
            for (int i = 0; i < Threads.Length; ++i) {
                Threads[i] = new(RenderFractal) {
                    IsBackground = true,
                };
                Threads[i].Start();
            }

            for (int i = 0; i < Threads.Length; ++i) {
                Threads[i].Join();

                WriteableBitmap bitmap = MainWindow.RenderBitmap;
                int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;

                bitmap.WritePixels(new(0, i * (bitmap.PixelHeight / Threads.Length), bitmap.PixelWidth, bitmap.PixelHeight / Threads.Length), ImgDatas[i], stride, 0);

                RenderBitmaps[i] = bitmap;
            }

            sw.Stop();
            LastRenderTimeMs = sw.ElapsedMilliseconds;
            MainWindow.RenderTimeTextBlock.Text = LastRenderTimeMs.ToString();
        }

        private void RenderFractal() {

            int threadIndex = RunningThreads;
            RunningThreads++;
            byte[] imgData = ImgDatas[threadIndex];

            int h = MainWindow.H;
            int w = MainWindow.W;

            double scale = 1.0 / ((double) h / 2);

            int yStart = (int) ((double) h / Threads.Length * threadIndex);

            for (int y = 0; y < h / Threads.Length; ++y) {
                for (int x = 0; x < w; ++x) {
                    double px = (x - w / 2) * scale;
                    double py = (y + yStart - h / 2) * scale;
                    byte iterations = SmoothIteration
                        ? (byte) ComputeIterationsSmooth(new(px, py), Constant, MaxIterations)
                        : ComputeIterations(new(px, py), Constant, MaxIterations);

                    imgData[(x + y * w) * 4] = iterations;
                    imgData[(x + y * w) * 4 + 1] = iterations;
                    imgData[(x + y * w) * 4 + 2] = iterations;
                    imgData[(x + y * w) * 4 + 3] = 255;
                }
            }
        }

        private static Point ComputeNext(Point current, Vector constant) {
            double zr = current.X * current.X - current.Y * current.Y;
            double zi = 2.0 * current.X * current.Y;
            return new Point(zr, zi) + constant;
        }

        private static double ModSqr(Point z) => z.X * z.X + z.Y * z.Y;

        private static byte ComputeIterations(Point z0, Vector constant, byte maxIteration) {
            Point zn = z0;
            byte i = 0;
            while (ModSqr(zn) < 4.0 && i < maxIteration) {
                zn = ComputeNext(zn, constant);
                ++i;
            }
            return i;
        }

        private static double ComputeIterationsSmooth(Point z0, Vector constant, byte maxIteration) {
            Point zn = z0;
            byte i = 0;
            while (ModSqr(zn) < 4.0 && i < maxIteration) {
                zn = ComputeNext(zn, constant);
                ++i;
            }
            return i - Math.Log2(Math.Max(1, Math.Log2(Math.Sqrt(ModSqr(zn)))));
        }
    }
}
