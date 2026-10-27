using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public class CursorTrailEffect : MouseCursorEffectAbstract
    {
        public const int MaxNodes = 256;
        public const double Lifetime = 0.5;

        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual;
        private readonly List<TrailNode> nodes = new List<TrailNode>(MaxNodes);
        private Point? lastPoint;

        public int NodeCount => nodes.Count;

        public CursorTrailEffect(Point point) : this(point, new MouseEffectSetting())
        {
        }

        public CursorTrailEffect(Point point, MouseEffectSetting settings) : base(new DrawingVisual())
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            visual = (DrawingVisual)GetVisualChild(0);
            FixedCount = 1;
            IsHitTestVisible = false;
            Update(point, 1.0 / 60);
        }

        public void Update(Point cursor, double elapsedSeconds)
        {
            if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y))
                return;

            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                node.Age += elapsedSeconds;
                if (node.Age >= Lifetime)
                    nodes.RemoveAt(i);
                else
                    nodes[i] = node;
            }

            if (!lastPoint.HasValue || cursor != lastPoint.Value)
            {
                if (nodes.Count == MaxNodes)
                    nodes.RemoveAt(0);
                nodes.Add(new TrailNode { Position = cursor });
                lastPoint = cursor;
            }

            using var context = visual.RenderOpen();
            DrawTrail(context, nodes, settings);
        }

        public void Clear()
        {
            nodes.Clear();
            lastPoint = null;
            using (visual.RenderOpen()) { }
        }

        public override DrawingVisual CreateDrawingVisualEllipsed(Point point)
        {
            var drawingVisual = new DrawingVisual();
            using var context = drawingVisual.RenderOpen();
            DrawTrail(context, new[] { new TrailNode { Position = point } }, settings);
            return drawingVisual;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(24, 27, 45)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new MatrixTransform(0.85, 0, 0, 0.85, -6, 23));
                    context.DrawDrawing(CreatePreviewImage(new MouseEffectSetting()).Drawing);
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        public static DrawingImage CreatePreviewImage(MouseEffectSetting settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var drawing = new DrawingGroup();
            using (var context = drawing.Open())
            {
                DrawTrail(context, new[]
                {
                    new TrailNode { Position = new Point(40, 100) },
                    new TrailNode { Position = new Point(95, 45) },
                    new TrailNode { Position = new Point(155, 100) },
                    new TrailNode { Position = new Point(215, 45) },
                    new TrailNode { Position = new Point(280, 70) }
                }, settings);
            }
            var image = new DrawingImage(drawing);
            image.Freeze();
            return image;
        }

        private static void DrawTrail(DrawingContext context, IReadOnlyList<TrailNode> nodes, MouseEffectSetting settings)
        {
            if (nodes.Count == 0)
                return;

            var color = (Color)ColorConverter.ConvertFromString(settings.TrailColor);
            var fade = Math.Clamp(1 - nodes[nodes.Count - 1].Age / Lifetime, 0, 1);
            var distances = new double[nodes.Count];
            for (var i = 1; i < nodes.Count; i++)
                distances[i] = distances[i - 1] + (nodes[i].Position - nodes[i - 1].Position).Length;
            var totalLength = distances[nodes.Count - 1];
            if (totalLength == 0)
            {
                var brush = new SolidColorBrush(WithOpacity(color, settings.TrailHeadOpacity * fade));
                context.DrawEllipse(brush, null, nodes[0].Position, settings.TrailSize / 2, settings.TrailSize / 2);
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var point = nodes[i].Position;
                var start = i == 0 ? point : Midpoint(nodes[i - 1].Position, point);
                var end = i == nodes.Count - 1 ? point : Midpoint(point, nodes[i + 1].Position);
                var startProgress = i == 0 ? 0 : (distances[i - 1] + distances[i]) / (2 * totalLength);
                var endProgress = i == nodes.Count - 1 ? 1 : (distances[i] + distances[i + 1]) / (2 * totalLength);
                var startOpacity = settings.TrailTailOpacity + (settings.TrailHeadOpacity - settings.TrailTailOpacity) * startProgress;
                var endOpacity = settings.TrailTailOpacity + (settings.TrailHeadOpacity - settings.TrailTailOpacity) * endProgress;
                Brush brush;
                if ((end - start).LengthSquared < 0.000001)
                {
                    brush = new SolidColorBrush(WithOpacity(color, (startOpacity + endOpacity) * 0.5 * fade));
                }
                else
                {
                    brush = new LinearGradientBrush(WithOpacity(color, startOpacity * fade), WithOpacity(color, endOpacity * fade), start, end)
                    {
                        MappingMode = BrushMappingMode.Absolute
                    };
                }
                brush.Freeze();
                var pen = new Pen(brush, settings.TrailSize)
                {
                    StartLineCap = i == 0 ? PenLineCap.Round : PenLineCap.Flat,
                    EndLineCap = i == nodes.Count - 1 ? PenLineCap.Round : PenLineCap.Flat,
                    LineJoin = PenLineJoin.Round
                };
                pen.Freeze();
                var geometry = new StreamGeometry();
                using (var path = geometry.Open())
                {
                    path.BeginFigure(start, false, false);
                    path.QuadraticBezierTo(point, end, true, true);
                }
                geometry.Freeze();
                context.DrawGeometry(null, pen, geometry);
            }
        }

        private static Point Midpoint(Point first, Point second)
        {
            return first + (second - first) * 0.5;
        }

        private static Color WithOpacity(Color color, double opacity)
        {
            return Color.FromArgb((byte)Math.Round(Math.Clamp(opacity, 0, 1) * 255), color.R, color.G, color.B);
        }

        private struct TrailNode
        {
            public Point Position;
            public double Age;
        }
    }
}
