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

        public static MonoGradient GrayScale { get; } = new(new(), new(255, 255, 255));
    }
}
