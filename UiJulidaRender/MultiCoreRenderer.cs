using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Imaging;

namespace UiJuliaRender;

public class MultiCoreRenderer {

    private Thread[]? Threads;
    private readonly WriteableBitmap RenderBitmap;
    private readonly MainWindow MainWindow;
    private long LastRenderTimeMs;
    private readonly Stopwatch Sw = new();
    private int MaxIterations;
    private bool SmoothIteration;
    private FastVectorD Constant;
    private readonly byte[] ImgData;
    private int RunningThreads;
    private ColorGradients.ColorGradient? ColorGradient;
    private double Zoom;

    public MultiCoreRenderer(MainWindow mainWindow) {
        MainWindow = mainWindow;
        RenderBitmap = MainWindow.RenderBitmap;
        ImgData = new byte[RenderBitmap.PixelHeight * RenderBitmap.PixelWidth * 3];
    }

    public void Render() {

        Sw.Restart();

        int threads = (int) MainWindow.ThreadsSlider.Value;

        Threads = new Thread[threads];

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

        foreach (Thread t in Threads)
            t.Join();

        RenderBitmap.WritePixels(new(0, 0, RenderBitmap.PixelWidth, RenderBitmap.PixelHeight), ImgData, RenderBitmap.Stride(), 0);

        Sw.Stop();
        LastRenderTimeMs = Sw.ElapsedMilliseconds;
        MainWindow.RenderTimeTextBlock.Text = LastRenderTimeMs.ToString();
    }

    private void RenderFractal() {

        int threadIndex = RunningThreads;
        RunningThreads++;

        const int h = MainWindow.H;
        const int w = MainWindow.W;

        double scale = 2.0 / h / Zoom;

        int yStart = (int) ((double) h / Threads.Length * threadIndex);
        int yEnd = yStart + h / Threads.Length;

        for (int y = yStart; y < yEnd; ++y) {

            double py = (y - h / 2.0) * scale;

            for (int x = 0; x < w / 2; ++x) {
                double px = (x - w / 2.0) * scale;

                FastPointD p = new(px, py);

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

    private static void ComputeNext(ref FastPointD current, ref FastVectorD constant) {
        double zr = current.X * current.X - current.Y * current.Y;
        double zi = 2.0 * current.X * current.Y;
        current.X = zr + constant.X;
        current.Y = zi + constant.Y;
    }

    private static double ModSqr(ref FastPointD z) => z.X * z.X + z.Y * z.Y;

    private static int ComputeIterations(ref FastPointD z0, ref FastVectorD constant, int maxIteration) {
        int i = 0;
        while (i < maxIteration && ModSqr(ref z0) < 4.0) {
            ComputeNext(ref z0, ref constant);
            ++i;
        }
        return i;
    }

    private static double ComputeIterationsSmooth(ref FastPointD z0, ref FastVectorD constant, int maxIteration) {
        int i = 0;
        while (i < maxIteration && ModSqr(ref z0) < 4.0) {
            ComputeNext(ref z0, ref constant);
            ++i;
        }
        return i - Math.Log2(Math.Max(1, Math.Log2(Math.Sqrt(ModSqr(ref z0)))));
    }
}

public struct FastPointD {
    public double X { get; set; }
    public double Y { get; set; }

    public FastPointD(double x, double y) {
        X = x;
        Y = y;
    }
}

public struct FastVectorD {
    public double X { get; }
    public double Y { get; }

    public FastVectorD(double x, double y) {
        X = x;
        Y = y;
    }
}