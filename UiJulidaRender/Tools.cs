using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UiJulidaRender {
    public static class Tools {
        public static int Stride(this WriteableBitmap bitmap) => (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;

        public static Color Lerp(this Color startColor, Color endColor, double t) {
            byte r = (byte) (endColor.R * t + startColor.R * (1 - t));
            byte g = (byte) (endColor.G * t + startColor.G * (1 - t));
            byte b = (byte) (endColor.B * t + startColor.B * (1 - t));
            return Color.FromRgb(r, g, b);
        }

        public static FastColor Lerp(this FastColor startColor, FastColor endColor, double t) {
            byte r = (byte) (endColor.R * t + startColor.R * (1 - t));
            byte g = (byte) (endColor.G * t + startColor.G * (1 - t));
            byte b = (byte) (endColor.B * t + startColor.B * (1 - t));
            return new FastColor(r, g, b);
        }

        public static Color ToColor(this FastColor betterColor) => Color.FromArgb(255, betterColor.R, betterColor.G, betterColor.B);
        public static FastColor ToBetterColor(this Color color) => new(color.R, color.G, color.B);

        public static void Save(this WriteableBitmap wb, string filePath) {
            if (filePath != string.Empty) {
                using FileStream stream5 = new(filePath, FileMode.Create);
                PngBitmapEncoder encoder5 = new();
                encoder5.Frames.Add(BitmapFrame.Create(wb));
                encoder5.Save(stream5);
            }
        }
    }
}
