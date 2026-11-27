using System;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FlameCursorEffect : UIElement, ICursorEffect
    {
        public const int FlameCount = 6;
        public const int TailCount = 5;
        public const double OrbitRadius = MouseEffectSetting.DefaultFlameRadius;
        public const double CycleDuration = 3;
        private static readonly Geometry FlameShape = CreateFlameShape();
        private static readonly Brush OuterBrush = CreateFlameBrush(false);
        private static readonly Brush CoreBrush = CreateFlameBrush(true);
        private static readonly Brush GlowBrush = CreateGlowBrush();
        private static readonly Pen OutlinePen = CreateOutlinePen();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private bool initialized;
        private double cycleTime;
        private double orbitTime;

        public int ParticleCount => initialized ? FlameCount : 0;
        protected override int VisualChildrenCount => 1;

        public FlameCursorEffect(MouseEffectSetting settings = null)
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
            initialized = true;
            cycleTime = (cycleTime + elapsedSeconds % CycleDuration) % CycleDuration;
            var period = CycleDuration / settings.FlameSpeed;
            orbitTime = (orbitTime + elapsedSeconds % period * settings.FlameSpeed) % CycleDuration;
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.FlameOpacity);
            context.PushTransform(new TranslateTransform(cursor.X, cursor.Y));
            DrawFlames(context, orbitTime, settings.FlameSizeScale, cycleTime, settings.FlameRadius);
            context.Pop();
            context.Pop();
        }

        public void Clear()
        {
            initialized = false;
            cycleTime = 0;
            orbitTime = 0;
            using (visual.RenderOpen()) { }
        }

        private static void DrawFlames(DrawingContext context, double time, double sizeScale = 1, double? pulseTime = null, double radius = OrbitRadius)
        {
            var phase = time * Math.PI * 2 / CycleDuration;
            var pulsePhase = (pulseTime ?? time) * Math.PI * 2 / CycleDuration;
            for (var i = 0; i < FlameCount; i++)
            {
                var angle = phase + i * Math.PI * 2 / FlameCount;
                for (var tail = TailCount; tail >= 0; tail--)
                {
                    var tailAngle = angle - tail * 0.12;
                    var center = new Point(Math.Cos(tailAngle) * radius, Math.Sin(tailAngle) * radius);
                    var flicker = 1 + 0.12 * Math.Sin(pulsePhase * 7 + i * 2 - tail * 0.7);
                    var scale = (1 - tail * 0.12) * flicker * sizeScale;
                    var transform = Matrix.Identity;
                    transform.Scale(8 * scale, 12 * scale);
                    transform.Rotate(tailAngle * 180 / Math.PI);
                    transform.Translate(center.X, center.Y);
                    context.PushOpacity(tail == 0 ? 1 : 0.5 * (1 - tail / (double)(TailCount + 1)));
                    context.PushTransform(new MatrixTransform(transform));
                    context.PushOpacity(tail == 0 ? 0.85 + 0.15 * Math.Sin(pulsePhase * 4 + i) : 0.45);
                    context.DrawEllipse(GlowBrush, null, new Point(0, -0.25), 2, 2.1);
                    context.Pop();
                    context.DrawGeometry(OuterBrush, tail == 0 ? OutlinePen : null, FlameShape);
                    if (tail == 0)
                    {
                        context.PushTransform(new ScaleTransform(0.55, 0.65));
                        context.DrawGeometry(CoreBrush, null, FlameShape);
                        context.DrawEllipse(Brushes.White, null, new Point(-0.18, 0.05), 0.14, 0.3);
                        context.Pop();
                    }
                    context.Pop();
                    context.Pop();
                }
            }
        }

        private static Geometry CreateFlameShape()
        {
            var shape = new StreamGeometry();
            using (var context = shape.Open())
            {
                context.BeginFigure(new Point(0, 0.6), true, true);
                context.BezierTo(new Point(-0.85, 0.6), new Point(-1.05, -0.05), new Point(-0.85, -0.65), true, false);
                context.LineTo(new Point(-0.55, -0.35), true, false);
                context.BezierTo(new Point(-0.6, -0.95), new Point(-0.2, -1.15), new Point(-0.38, -1.75), true, false);
                context.BezierTo(new Point(0.22, -1.5), new Point(0.48, -0.95), new Point(0.25, -0.55), true, false);
                context.BezierTo(new Point(0.6, -0.7), new Point(0.72, -1.02), new Point(0.68, -1.2), true, false);
                context.BezierTo(new Point(1.3, -0.4), new Point(1, 0.6), new Point(0, 0.6), true, false);
            }
            shape.Freeze();
            return shape;
        }

        private static Brush CreateFlameBrush(bool core)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 1), EndPoint = new Point(0.5, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(core ? Color.FromRgb(255, 255, 225) : Color.FromRgb(255, 199, 54), 0),
                    new GradientStop(core ? Color.FromRgb(255, 255, 225) : Color.FromRgb(255, 199, 54), 0.42),
                    new GradientStop(core ? Color.FromRgb(255, 235, 110) : Color.FromRgb(255, 113, 28), 0.43),
                    new GradientStop(core ? Color.FromRgb(255, 235, 110) : Color.FromRgb(255, 113, 28), 0.72),
                    new GradientStop(core ? Color.FromRgb(255, 210, 66) : Color.FromRgb(246, 62, 30), 0.73),
                    new GradientStop(core ? Color.FromRgb(255, 210, 66) : Color.FromRgb(246, 62, 30), 1)
                }
            };
            brush.Freeze();
            return brush;
        }

        private static Pen CreateOutlinePen()
        {
            var pen = new Pen(new SolidColorBrush(Color.FromRgb(168, 42, 33)), 0.1) { LineJoin = PenLineJoin.Round };
            pen.Freeze();
            return pen;
        }

        private static Brush CreateGlowBrush()
        {
            var brush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(210, 255, 171, 48), 0),
                    new GradientStop(Color.FromArgb(85, 255, 95, 20), 0.35),
                    new GradientStop(Color.FromArgb(0, 255, 45, 0), 1)
                }
            };
            brush.Freeze();
            return brush;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(35, 27, 31)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new TranslateTransform(130, 85));
                    context.PushTransform(new ScaleTransform(1.2, 1.2));
                    DrawFlames(context, 0.35);
                    context.Pop();
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }
    }
}
