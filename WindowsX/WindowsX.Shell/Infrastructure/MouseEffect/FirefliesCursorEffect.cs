using System;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FirefliesCursorEffect : UIElement, ICursorEffect
    {
        public const int FireflyCount = 6;
        private static readonly Brush[] GlowBrushes = CreateGlowBrushes();
        private static readonly Brush CoreBrush = CreateCoreBrush();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly Point[] anchors = new Point[FireflyCount];
        private bool initialized;
        private double cycleTime;

        public int ParticleCount => initialized ? FireflyCount : 0;
        protected override int VisualChildrenCount => 1;

        public FirefliesCursorEffect(MouseEffectSetting settings = null)
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
            if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y))
                return;

            if (initialized)
            {
                foreach (var anchor in anchors)
                {
                    var delta = cursor - anchor;
                    if (!double.IsFinite(delta.X) || !double.IsFinite(delta.Y)) return;
                }
            }
            for (var i = 0; i < FireflyCount; i++)
            {
                if (!initialized) anchors[i] = cursor;
                else anchors[i] += (cursor - anchors[i]) * (1 - Math.Exp(-elapsedSeconds * (1.2 + i * 0.22) * settings.FirefliesFollowSpeed));
            }
            initialized = true;
            cycleTime = (cycleTime + elapsedSeconds % 60) % 60;
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.FirefliesOpacity);
            for (var i = 0; i < FireflyCount; i++)
                DrawFirefly(context, anchors[i] + Offset(i, cycleTime), i, cycleTime);
            context.Pop();
        }

        public void Clear()
        {
            initialized = false;
            cycleTime = 0;
            Array.Clear(anchors, 0, anchors.Length);
            using (visual.RenderOpen()) { }
        }

        private static Vector Offset(int index, double time)
        {
            var phase = index * Math.PI * 2 / FireflyCount;
            var orbit = time * Math.PI * 2 / 60;
            var radius = 20 + index * 4;
            return new Vector(Math.Cos(phase) * radius + Math.Sin(orbit * (7 + index) + phase) * 9,
                Math.Sin(phase) * radius * 0.65 + Math.Cos(orbit * (9 + index) + phase) * 7);
        }

        private static void DrawFirefly(DrawingContext context, Point center, int index, double time)
        {
            var pulse = (1 + Math.Sin(time * Math.PI * 2 / 60 * (18 + index * 2) + index * 1.7)) * 0.5;
            var brightness = 0.15 + 0.85 * pulse * pulse;
            var radius = 1.5 + index % 3 * 0.3;
            context.PushOpacity(brightness);
            context.DrawEllipse(GlowBrushes[index % GlowBrushes.Length], null, center, radius * 7, radius * 7);
            context.DrawEllipse(CoreBrush, null, center, radius, radius);
            context.Pop();
        }

        private static Brush[] CreateGlowBrushes()
        {
            var colors = new[] { Color.FromRgb(208, 255, 100), Color.FromRgb(255, 231, 112), Color.FromRgb(163, 244, 125) };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var brush = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(200, color.R, color.G, color.B), 0),
                        new GradientStop(Color.FromArgb(75, color.R, color.G, color.B), 0.3),
                        new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
                    }
                };
                brush.Freeze();
                brushes[i] = brush;
            }
            return brushes;
        }

        private static Brush CreateCoreBrush()
        {
            var brush = new SolidColorBrush(Color.FromRgb(255, 255, 205));
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(20, 32, 29)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < FireflyCount; i++)
                        DrawFirefly(context, new Point(40 + i * 35, 85 + Math.Sin(i * 1.5) * 27), i, 0.8);
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }
    }
}
