using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class CloudCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxStars = 12;
        private static readonly Geometry CloudShape = CreateCloudShape();
        private static readonly Geometry StarShape = CreateStarShape();
        private static readonly Brush CloudBrush = CreateCloudBrush();
        private static readonly Brush FaceBrush = FrozenBrush(Color.FromRgb(76, 91, 128));
        private static readonly Brush BlushBrush = FrozenBrush(Color.FromRgb(255, 181, 202));
        private static readonly Brush StarBrush = FrozenBrush(Color.FromRgb(255, 222, 125));
        private static readonly Pen Outline = CreatePen(Color.FromRgb(151, 185, 218), 0.9);
        private static readonly Pen Smile = CreatePen(Color.FromRgb(76, 91, 128), 0.9);
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<FallingStar> stars = new List<FallingStar>(MaxStars);
        private readonly Random random = new Random();
        private Point? position;
        private Vector velocity;
        private double cycleTime;
        private double untilNextStar = 0.9;

        public int ParticleCount => position.HasValue ? 1 : 0;
        public int StarCount => stars.Count;
        protected override int VisualChildrenCount => 1;

        public CloudCursorEffect(MouseEffectSetting settings = null)
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
            var left = settings.CloudPosition is "TopLeft" or "BottomLeft";
            var top = settings.CloudPosition is "TopLeft" or "TopRight";
            var target = cursor + new Vector(settings.CloudHorizontalOffset * (left ? -1 : 1), settings.CloudVerticalOffset * (top ? -1 : 1));
            if (!double.IsFinite(target.X) || !double.IsFinite(target.Y)) return;
            if (position.HasValue && !double.IsFinite((position.Value - target).Length)) return;
            for (var i = stars.Count - 1; i >= 0; i--)
            {
                var star = stars[i];
                star.Age += elapsedSeconds;
                if (star.Age >= 1.6) stars.RemoveAt(i);
                else stars[i] = star;
            }
            if (!position.HasValue || elapsedSeconds > 2)
            {
                position = target;
                velocity = new Vector();
            }
            else
            {
                var offset = position.Value - target;
                var decay = Math.Exp(-7 * elapsedSeconds);
                var cosine = Math.Cos(9 * elapsedSeconds);
                var sine = Math.Sin(9 * elapsedSeconds);
                var wave = (velocity + offset * 7) / 9;
                var nextOffset = (offset * cosine + wave * sine) * decay;
                velocity = ((wave * 9 - offset * 7) * cosine - (offset * 9 + wave * 7) * sine) * decay;
                position = target + nextOffset;
            }
            cycleTime = (cycleTime + elapsedSeconds % 4) % 4;
            var center = position.Value + new Vector(0, Math.Sin(cycleTime * Math.PI / 2) * 1.5);
            var stretch = Math.Clamp(velocity.X / 1500, -0.14, 0.14) + Math.Sin(cycleTime * Math.PI) * 0.025;
            var tilt = Math.Clamp(velocity.X / 90, -10, 10);
            if (elapsedSeconds > 0.25)
                untilNextStar = 0.9;
            else
            {
                untilNextStar -= elapsedSeconds;
                if (untilNextStar <= 0)
                {
                    if (stars.Count == MaxStars) stars.RemoveAt(0);
                    var origin = CreateCloudTransform(center, stretch, tilt, settings.CloudSizeScale).Transform(new Point(random.NextDouble() * 18 - 9, 10));
                    stars.Add(new FallingStar { Origin = origin, Drift = random.NextDouble() * 20 - 10, Size = 2.5 + random.NextDouble() * 1.5 });
                    untilNextStar = 0.9 + random.NextDouble() * 0.8;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.CloudOpacity);
            foreach (var star in stars) DrawStar(context, star);
            DrawCloud(context, center, stretch, tilt, settings.CloudSizeScale);
            context.Pop();
        }

        public void Clear()
        {
            position = null;
            velocity = new Vector();
            cycleTime = 0;
            untilNextStar = 0.9;
            stars.Clear();
            using (visual.RenderOpen()) { }
        }

        private static Matrix CreateCloudTransform(Point center, double stretch, double tilt, double sizeScale)
        {
            var transform = Matrix.Identity;
            transform.Scale((1 + stretch) * sizeScale, sizeScale / (1 + stretch));
            transform.Rotate(tilt);
            transform.Translate(center.X, center.Y);
            return transform;
        }

        private static void DrawCloud(DrawingContext context, Point center, double stretch, double tilt, double sizeScale = 1)
        {
            context.PushTransform(new MatrixTransform(CreateCloudTransform(center, stretch, tilt, sizeScale)));
            context.DrawGeometry(CloudBrush, Outline, CloudShape);
            context.DrawEllipse(Brushes.White, null, new Point(-6, -8), 3.5, 1.4);
            context.DrawEllipse(FaceBrush, null, new Point(-5, 2), 1.3, 1.8);
            context.DrawEllipse(FaceBrush, null, new Point(5, 2), 1.3, 1.8);
            context.DrawEllipse(Brushes.White, null, new Point(-5.3, 1.4), 0.4, 0.5);
            context.DrawEllipse(Brushes.White, null, new Point(4.7, 1.4), 0.4, 0.5);
            context.DrawEllipse(BlushBrush, null, new Point(-9, 5), 2.3, 1);
            context.DrawEllipse(BlushBrush, null, new Point(9, 5), 2.3, 1);
            context.DrawLine(Smile, new Point(-2, 4), new Point(0, 5.5));
            context.DrawLine(Smile, new Point(0, 5.5), new Point(2, 4));
            context.Pop();
        }

        private static void DrawStar(DrawingContext context, FallingStar star)
        {
            var progress = star.Age / 1.6;
            var center = star.Origin + new Vector(star.Drift * star.Age, 15 * star.Age + 25 * star.Age * star.Age);
            var transform = Matrix.Identity;
            transform.Scale(star.Size * (1 - progress * 0.4), star.Size * (1 - progress * 0.4));
            transform.Rotate(star.Age * 45);
            transform.Translate(center.X, center.Y);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.4));
            context.PushTransform(new MatrixTransform(transform));
            context.DrawGeometry(StarBrush, null, StarShape);
            context.Pop();
            context.Pop();
        }

        private static Geometry CreateCloudShape()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(-14, 10), true, true);
                context.BezierTo(new Point(-28, 8), new Point(-24, -9), new Point(-14, -6), true, false);
                context.BezierTo(new Point(-14, -22), new Point(6, -24), new Point(9, -10), true, false);
                context.BezierTo(new Point(18, -16), new Point(26, -5), new Point(21, 0), true, false);
                context.BezierTo(new Point(29, 10), new Point(14, 14), new Point(9, 11), true, false);
                context.BezierTo(new Point(0, 15), new Point(-7, 13), new Point(-14, 10), true, false);
            }
            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateStarShape()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0, -1), true, true);
                for (var i = 1; i < 10; i++)
                {
                    var angle = i * Math.PI / 5 - Math.PI / 2;
                    var radius = i % 2 == 0 ? 1 : 0.45;
                    context.LineTo(new Point(Math.Cos(angle) * radius, Math.Sin(angle) * radius), true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static Brush CreateCloudBrush()
        {
            var brush = new LinearGradientBrush(Colors.White, Color.FromRgb(204, 229, 255), 90);
            brush.Freeze();
            return brush;
        }

        private static Brush FrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static Pen CreatePen(Color color, double width)
        {
            var pen = new Pen(FrozenBrush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
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
                    context.DrawRoundedRectangle(FrozenBrush(Color.FromRgb(39, 43, 68)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new ScaleTransform(1.7, 1.7, 130, 70));
                    DrawCloud(context, new Point(130, 70), 0.06, -6);
                    context.Pop();
                    for (var i = 0; i < 3; i++)
                        DrawStar(context, new FallingStar { Origin = new Point(113 + i * 17, 100), Age = 0.15 + i * 0.3, Size = 4, Drift = (i - 1) * 6 });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct FallingStar
        {
            public Point Origin;
            public double Age;
            public double Drift;
            public double Size;
        }
    }
}
