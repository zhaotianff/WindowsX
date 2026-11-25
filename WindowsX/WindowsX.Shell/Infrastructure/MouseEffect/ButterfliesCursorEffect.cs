using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class ButterfliesCursorEffect : UIElement, ICursorEffect
    {
        public const int ButterflyCount = 3;
        public const int MaxSparkles = 96;
        private static readonly Brush[] WingBrushes = CreateWingBrushes();
        private static readonly Geometry Wing = CreateWing();
        private static readonly Geometry Star = CreateStar();
        private static readonly Brush BodyBrush = FrozenBrush(Color.FromRgb(78, 57, 98));
        private static readonly Brush FaceBrush = FrozenBrush(Color.FromRgb(255, 235, 245));
        private static readonly Brush HighlightBrush = FrozenBrush(Color.FromRgb(255, 252, 255));
        private static readonly Brush BlushBrush = FrozenBrush(Color.FromRgb(255, 147, 184));
        private static readonly Pen OutlinePen = CreateOutlinePen();
        private static readonly Brush StarBrush = FrozenBrush(Color.FromRgb(255, 242, 174));
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly Point[] anchors = new Point[ButterflyCount];
        private readonly double[] wingPhases = new double[ButterflyCount];
        private readonly List<Sparkle> sparkles = new List<Sparkle>(MaxSparkles);
        private Point? previousCursor;
        private double orbitTime;
        private double chaseBoost;

        public int ParticleCount => previousCursor.HasValue ? ButterflyCount : 0;
        public int SparkleCount => sparkles.Count;
        protected override int VisualChildrenCount => 1;

        public ButterfliesCursorEffect(MouseEffectSetting settings = null)
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
            var distance = previousCursor.HasValue ? (cursor - previousCursor.Value).Length : 0;
            if (!double.IsFinite(distance)) return;
            if (previousCursor.HasValue)
                foreach (var anchor in anchors)
                    if (!double.IsFinite((cursor - anchor).Length)) return;

            for (var i = sparkles.Count - 1; i >= 0; i--)
            {
                var sparkle = sparkles[i];
                sparkle.Age += elapsedSeconds;
                if (sparkle.Age >= 1.4) sparkles.RemoveAt(i);
                else sparkles[i] = sparkle;
            }
            var speed = distance / elapsedSeconds;
            chaseBoost = Math.Max(chaseBoost * Math.Exp(-3 * elapsedSeconds), Math.Min(1, speed / 700));
            orbitTime = (orbitTime + elapsedSeconds % 60) % 60;
            var frame = Math.Min(elapsedSeconds, 0.1);
            for (var i = 0; i < ButterflyCount; i++)
            {
                if (!previousCursor.HasValue)
                {
                    anchors[i] = cursor;
                    wingPhases[i] = (double)i / ButterflyCount;
                }
                else anchors[i] += (cursor - anchors[i]) * (1 - Math.Exp(-(2 + i * 0.3 + chaseBoost * 7) * elapsedSeconds));
                var nextPhase = wingPhases[i] + frame * (3 + i * 0.2 + chaseBoost * 2);
                wingPhases[i] = nextPhase % 1;
                if (nextPhase >= 1 && elapsedSeconds <= 0.25)
                {
                    var center = anchors[i] + Orbit(i, orbitTime);
                    if (sparkles.Count == MaxSparkles) sparkles.RemoveAt(0);
                    sparkles.Add(new Sparkle { Origin = center + new Vector(i % 2 == 0 ? -6 : 6, 4), Drift = (i - (ButterflyCount - 1) / 2.0) * 5, Size = 2 + i % 2 });
                }
            }
            previousCursor = cursor;
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.ButterfliesOpacity);
            foreach (var sparkle in sparkles) DrawSparkle(context, sparkle);
            for (var i = 0; i < ButterflyCount; i++)
                DrawButterfly(context, anchors[i] + Orbit(i, orbitTime), i, wingPhases[i], Math.Sin(orbitTime * Math.PI / 6 + i) * 18);
            context.Pop();
        }

        public void Clear()
        {
            previousCursor = null;
            orbitTime = 0;
            chaseBoost = 0;
            sparkles.Clear();
            Array.Clear(anchors, 0, anchors.Length);
            Array.Clear(wingPhases, 0, wingPhases.Length);
            using (visual.RenderOpen()) { }
        }

        private static Vector Orbit(int index, double time)
        {
            var angle = time * Math.PI * 2 / 60 * (5 + index) + index * Math.PI * 2 / ButterflyCount;
            return new Vector(Math.Cos(angle) * (30 + index * 9), Math.Sin(angle) * (22 + index * 6));
        }

        private static void DrawButterfly(DrawingContext context, Point center, int index, double phase, double tilt)
        {
            var transform = Matrix.Identity;
            transform.Scale(0.8 + index * 0.08, 0.8 + index * 0.08);
            transform.Rotate(tilt);
            transform.Translate(center.X, center.Y);
            context.PushTransform(new MatrixTransform(transform));
            var flap = 0.18 + 0.82 * (0.5 + 0.5 * Math.Cos(phase * Math.PI * 2));
            context.PushTransform(new ScaleTransform(flap, 1));
            DrawWing(context, index);
            context.PushTransform(new ScaleTransform(-1, 1));
            DrawWing(context, index);
            context.Pop();
            context.Pop();
            context.DrawEllipse(WingBrushes[index], OutlinePen, new Point(0, 1), 2.1, 6);
            context.DrawLine(OutlinePen, new Point(-1.5, -8), new Point(-4, -12));
            context.DrawLine(OutlinePen, new Point(1.5, -8), new Point(4, -12));
            context.DrawEllipse(BlushBrush, null, new Point(-4, -12), 1, 1);
            context.DrawEllipse(BlushBrush, null, new Point(4, -12), 1, 1);
            context.DrawEllipse(FaceBrush, OutlinePen, new Point(0, -6), 3.7, 3.5);
            context.DrawEllipse(BodyBrush, null, new Point(-1.4, -6.5), 0.8, 1.2);
            context.DrawEllipse(BodyBrush, null, new Point(1.4, -6.5), 0.8, 1.2);
            context.DrawEllipse(HighlightBrush, null, new Point(-1.6, -7), 0.3, 0.4);
            context.DrawEllipse(HighlightBrush, null, new Point(1.2, -7), 0.3, 0.4);
            context.DrawEllipse(BlushBrush, null, new Point(-2.5, -5), 0.7, 0.45);
            context.DrawEllipse(BlushBrush, null, new Point(2.5, -5), 0.7, 0.45);
            context.DrawLine(OutlinePen, new Point(-0.6, -4.4), new Point(0, -4));
            context.DrawLine(OutlinePen, new Point(0, -4), new Point(0.6, -4.4));
            context.Pop();
        }

        private static void DrawWing(DrawingContext context, int index)
        {
            context.DrawGeometry(WingBrushes[index], OutlinePen, Wing);
            context.DrawEllipse(HighlightBrush, null, new Point(10, -8), 2.3, 3.2);
            context.DrawEllipse(HighlightBrush, null, new Point(7, 6), 1.6, 1.9);
            context.PushTransform(new MatrixTransform(2, 0, 0, 2, 6, -3));
            context.DrawGeometry(HighlightBrush, null, Star);
            context.Pop();
        }

        private static Pen CreateOutlinePen()
        {
            var pen = new Pen(BodyBrush, 0.8) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
            pen.Freeze();
            return pen;
        }

        private static void DrawSparkle(DrawingContext context, Sparkle sparkle)
        {
            var center = sparkle.Origin + new Vector(sparkle.Drift * sparkle.Age, 12 * sparkle.Age + 24 * sparkle.Age * sparkle.Age);
            var scale = sparkle.Size * (1 - sparkle.Age / 1.4 * 0.5);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - sparkle.Age / 1.4), 1.5));
            context.PushTransform(new MatrixTransform(scale, 0, 0, scale, center.X, center.Y));
            context.DrawGeometry(StarBrush, null, Star);
            context.Pop();
            context.Pop();
        }

        private static Geometry CreateWing()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0, -3), true, true);
                context.BezierTo(new Point(17, -23), new Point(21, -2), new Point(8, 1), true, false);
                context.BezierTo(new Point(19, 10), new Point(5, 17), new Point(0, 3), true, false);
            }
            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateStar()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0, -1), true, true);
                for (var i = 1; i < 8; i++)
                {
                    var angle = i * Math.PI / 4 - Math.PI / 2;
                    var radius = i % 2 == 0 ? 1 : 0.25;
                    context.LineTo(new Point(Math.Cos(angle) * radius, Math.Sin(angle) * radius), true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static Brush FrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static Brush[] CreateWingBrushes()
        {
            var colors = new[] { Color.FromRgb(223, 142, 255), Color.FromRgb(112, 208, 255), Color.FromRgb(255, 148, 193) };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var shade = Color.FromRgb((byte)(color.R * 0.82), (byte)(color.G * 0.82), (byte)(color.B * 0.9));
                var brush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0), EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(255, 235, 255), 0),
                        new GradientStop(color, 0.35),
                        new GradientStop(color, 0.7),
                        new GradientStop(shade, 0.7),
                        new GradientStop(shade, 1)
                    }
                };
                brush.Freeze();
                brushes[i] = brush;
            }
            return brushes;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(FrozenBrush(Color.FromRgb(33, 30, 55)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < ButterflyCount; i++)
                    {
                        var center = new Point(130, 75) + Orbit(i, 2);
                        DrawButterfly(context, center, i, 0.08 + i * 0.04, (i - (ButterflyCount - 1) / 2.0) * 12);
                        for (var j = 0; j < 3; j++)
                            DrawSparkle(context, new Sparkle { Origin = center, Age = 0.2 + j * 0.3, Drift = (i - (ButterflyCount - 1) / 2.0) * 9, Size = 3 });
                    }
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Sparkle
        {
            public Point Origin;
            public double Age;
            public double Drift;
            public double Size;
        }
    }
}
