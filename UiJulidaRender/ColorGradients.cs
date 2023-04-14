using System.Linq;

namespace UiJulidaRender {

    public readonly struct FastColor {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public FastColor(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }
    }

    public static class ColorGradients {

        public abstract class ColorGradient {
            public abstract FastColor Lerp(double t);
        }

        public class MonoGradient : ColorGradient {

            public FastColor StartColor { get; private set; }
            public FastColor EndColor { get; private set; }

            public MonoGradient(FastColor startColor, FastColor endColor) {
                StartColor = startColor;
                EndColor = endColor;
            }

            public override FastColor Lerp(double t) => StartColor.Lerp(EndColor, t);
        }

        public class MultiColorGradient : ColorGradient {

            private readonly FastColor[] Colors;
            private readonly int MaxIndex;

            public MultiColorGradient(FastColor[] colors) {
                Colors = colors;
                MaxIndex = Colors.Length - 1;
            }

            public override FastColor Lerp(double t) {
                FastColor color1 = Colors[(int) (t * MaxIndex)];
                // TODO: why -0.03???
                FastColor color2 = Colors[(int) ((t - 0.03) * MaxIndex) + 1];
                return color1.Lerp(color2, t * MaxIndex);
            }
        }

        public static MultiColorGradient RainbowGradient { get; } = new(new FastColor[] {
            new(255, 0, 255),
            new(255, 0, 0),
            new(255, 255, 0),
            new(0, 255, 0),
            new(0, 255, 255),
            new(0, 0, 255),
        }.Reverse().ToArray());

        // "#FBB9C5", "#FDD0B1", "#F9EFC7", "#C3EDBF", "#B8DFE6", "#C5BBDE"
        public static MultiColorGradient PastelRainbowGradient { get; } = new(new FastColor[] {
            new(251, 185, 197),
            new(253, 208, 177),
            new(249, 239, 199),
            new(195, 237, 191),
            new(184, 223, 230),
            new(197, 187, 222),
        }.Reverse().ToArray());

        public static MonoGradient GrayScale { get; } = new(new(), new(255, 255, 255));
    }
}
