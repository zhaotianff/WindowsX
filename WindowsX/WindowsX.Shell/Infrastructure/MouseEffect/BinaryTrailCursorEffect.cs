using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class BinaryTrailCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxParticles = 24;
        public const int MaxEmissionPerUpdate = 2;
        private const double CharacterSpacing = 24;
        private const double EmissionInterval = 0.04;
        private static readonly Color[] DigitColors = { Color.FromRgb(92, 255, 143), Color.FromRgb(81, 244, 191), Color.FromRgb(100, 230, 255) };
        private static readonly Geometry[] DigitShapes = { CreateDigit(false), CreateDigit(true) };
        private static readonly Pen[][] DigitPens = CreatePens();
        private static readonly Brush[] HaloBrushes = CreateHaloBrushes();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Digit> particles = new List<Digit>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextDigit;
        private int nextColor;
        private string cachedColor;
        private string cachedFont;
        private Geometry[] currentShapes = DigitShapes;
        private Pen[][] currentPens = DigitPens;
        private Brush[] currentHalos = HaloBrushes;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public BinaryTrailCursorEffect(MouseEffectSetting settings = null)
        {
            this.settings = settings ?? new MouseEffectSetting();
            IsHitTestVisible = false;
            AddVisualChild(visual);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return visual;
        }

        public void Update(Point cursor, double elapsedSeconds)
        {
            if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y)) return;
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var digit = particles[i];
                digit.Age += elapsedSeconds;
                if (digit.Age >= digit.Lifetime) particles.RemoveAt(i);
                else particles[i] = digit;
            }
            elapsedSinceEmission = Math.Min(1, elapsedSinceEmission + elapsedSeconds);
            if (!lastEmissionPoint.HasValue)
            {
                Emit(cursor);
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }
            else
            {
                var movement = cursor - lastEmissionPoint.Value;
                var distance = movement.Length;
                if (double.IsFinite(distance) && distance >= CharacterSpacing && elapsedSinceEmission >= EmissionInterval)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Floor(distance / CharacterSpacing));
                    var direction = movement / distance;
                    for (var i = count - 1; i >= 0; i--) Emit(cursor - direction * (i * CharacterSpacing));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            UpdateAppearance();
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.BinaryTrailOpacity);
            foreach (var digit in particles) DrawDigit(context, digit, currentShapes, currentPens, currentHalos, settings.BinaryTrailSizeScale, cachedFont != MouseEffectSetting.DefaultBinaryTrailFontFamily);
            context.Pop();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            nextDigit = 0;
            nextColor = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new Digit
            {
                Origin = point, Size = 0.8 + random.NextDouble() * 0.3,
                Lifetime = 0.6 + random.NextDouble() * 0.25,
                Velocity = new Vector(random.NextDouble() * 6 - 3, 5 + random.NextDouble() * 8),
                Value = nextDigit, ColorIndex = nextColor
            });
            nextDigit = 1 - nextDigit;
            nextColor = (nextColor + 1) % DigitPens.Length;
        }

        private void UpdateAppearance()
        {
            var colorKey = settings.BinaryTrailUseDefaultColors ? string.Empty : settings.BinaryTrailColor;
            if (cachedColor != colorKey)
            {
                if (colorKey.Length == 0)
                {
                    currentPens = DigitPens;
                    currentHalos = HaloBrushes;
                }
                else
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorKey);
                    var colors = new[] { color, color, color };
                    currentPens = CreatePens(colors);
                    currentHalos = CreateHaloBrushes(colors);
                }
                cachedColor = colorKey;
            }
            if (cachedFont != settings.BinaryTrailFontFamily)
            {
                currentShapes = settings.BinaryTrailFontFamily == MouseEffectSetting.DefaultBinaryTrailFontFamily
                    ? DigitShapes : CreateFontDigits(settings.BinaryTrailFontFamily);
                cachedFont = settings.BinaryTrailFontFamily;
            }
        }

        private static Geometry[] CreateFontDigits(string fontFamily)
        {
            var typeface = new Typeface(new FontFamily(fontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var shapes = new Geometry[2];
            for (var i = 0; i < shapes.Length; i++)
            {
                var text = new FormattedText(i.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, typeface, 16, Brushes.White, 1);
                var geometry = text.BuildGeometry(new Point());
                var bounds = geometry.Bounds;
                if (bounds.IsEmpty || bounds.Height <= 0)
                    return DigitShapes;
                var scale = Math.Min(12 / bounds.Height, 10 / bounds.Width);
                var transform = Matrix.Identity;
                transform.Translate(-bounds.X - bounds.Width / 2, -bounds.Y - bounds.Height / 2);
                transform.Scale(scale, scale);
                geometry.Transform = new MatrixTransform(transform);
                geometry.Freeze();
                shapes[i] = geometry;
            }
            return shapes;
        }

        private static void DrawDigit(DrawingContext context, Digit digit, Geometry[] shapes = null, Pen[][] pens = null, Brush[] halos = null, double sizeScale = 1, bool filled = false)
        {
            shapes ??= DigitShapes;
            pens ??= DigitPens;
            halos ??= HaloBrushes;
            var center = digit.Origin + digit.Velocity * digit.Age;
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - digit.Age / digit.Lifetime), 1.4));
            var size = digit.Size * sizeScale;
            context.PushTransform(new MatrixTransform(size, 0, 0, size, center.X, center.Y));
            var pulse = 0.85 + 0.15 * Math.Sin(digit.Age * 7 + digit.ColorIndex * 1.7 + digit.Value);
            context.PushOpacity(pulse);
            context.DrawEllipse(halos[digit.ColorIndex], null, new Point(0, 0), 13, 17);
            context.Pop();
            foreach (var pen in pens[digit.ColorIndex])
                context.DrawGeometry(filled ? pen.Brush : null, pen, shapes[digit.Value]);
            context.Pop();
            context.Pop();
        }

        private static Brush[] CreateHaloBrushes(Color[] colors = null)
        {
            colors ??= DigitColors;
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var brush = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(95, color.R, color.G, color.B), 0),
                        new GradientStop(Color.FromArgb(38, color.R, color.G, color.B), 0.45),
                        new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
                    }
                };
                brush.Freeze();
                brushes[i] = brush;
            }
            return brushes;
        }

        private static Geometry CreateDigit(bool one)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                if (one)
                {
                    context.BeginFigure(new Point(-3, -4), false, false);
                    context.LineTo(new Point(0, -6), true, false);
                    context.LineTo(new Point(0, 6), true, false);
                    context.BeginFigure(new Point(-3, 6), false, false);
                    context.LineTo(new Point(3, 6), true, false);
                }
                else
                {
                    context.BeginFigure(new Point(0, -6), false, true);
                    context.BezierTo(new Point(5, -6), new Point(5, 6), new Point(0, 6), true, false);
                    context.BezierTo(new Point(-5, 6), new Point(-5, -6), new Point(0, -6), true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static Pen[][] CreatePens(Color[] colors = null)
        {
            var useDefaultColors = colors == null;
            colors ??= DigitColors;
            var pens = new Pen[colors.Length][];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var highlight = useDefaultColors ? Color.FromRgb(225, 255, 242)
                    : Color.FromRgb((byte)(color.R + (255 - color.R) * 0.8), (byte)(color.G + (255 - color.G) * 0.8), (byte)(color.B + (255 - color.B) * 0.8));
                pens[i] = new[]
                {
                    CreatePen(Color.FromArgb(38, color.R, color.G, color.B), 7),
                    CreatePen(Color.FromArgb(100, color.R, color.G, color.B), 3.5),
                    CreatePen(color, 1.8),
                    CreatePen(highlight, 0.6)
                };
            }
            return pens;
        }

        private static Pen CreatePen(Color color, double width)
        {
            var pen = new Pen(new SolidColorBrush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
            pen.Freeze();
            return pen;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(16, 29, 34)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 9; i++)
                        DrawDigit(context, new Digit
                        {
                            Origin = new Point(34 + i * 24, 85 + Math.Sin(i * 0.5) * 22),
                            Size = 1.2, Lifetime = 0.85, Age = (8 - i) * 0.07,
                            Velocity = new Vector(0, 8), Value = i % 2, ColorIndex = i % DigitPens.Length
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Digit
        {
            public Point Origin;
            public Vector Velocity;
            public double Size;
            public double Lifetime;
            public double Age;
            public int Value;
            public int ColorIndex;
        }
    }
}
