using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class NeonRibbonCursorEffect : UIElement
    {
        public const int MaxNodes = 192;
        public const double Lifetime = 0.8;
        private static readonly double[] LayerWidths = { 3.2, 2.1, 1, 0.22 };
        private static readonly double[] LayerOpacities = { 0.06, 0.13, 0.85, 0.95 };
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<RibbonNode> nodes = new List<RibbonNode>(MaxNodes);
        private Point? lastPoint;

        public int NodeCount => nodes.Count;
        protected override int VisualChildrenCount => 1;

        public NeonRibbonCursorEffect(MouseEffectSetting settings = null)
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

            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                node.Age += elapsedSeconds;
                if (node.Age >= Lifetime) nodes.RemoveAt(i);
                else nodes[i] = node;
            }

            var distance = lastPoint.HasValue ? (cursor - lastPoint.Value).Length : 0;
            if (!lastPoint.HasValue || (double.IsFinite(distance) && distance >= 2))
            {
                if (nodes.Count == MaxNodes) nodes.RemoveAt(0);
                nodes.Add(new RibbonNode { Position = cursor });
                lastPoint = cursor;
            }

            using var context = visual.RenderOpen();
            context.PushOpacity(settings.NeonRibbonOpacity);
            DrawRibbon(context, nodes, settings);
            context.Pop();
        }

        public void Clear()
        {
            nodes.Clear();
            lastPoint = null;
            using (visual.RenderOpen()) { }
        }

        public static DrawingImage PreviewImage => CreatePreviewImage(new MouseEffectSetting());

        public static DrawingImage CreatePreviewImage(MouseEffectSetting settings)
        {
                if (settings == null) throw new ArgumentNullException(nameof(settings));
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(17, 16, 37)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushOpacity(settings.NeonRibbonOpacity);
                    DrawRibbon(context, new[]
                    {
                        new RibbonNode { Position = new Point(30, 115), Age = 0.5 },
                        new RibbonNode { Position = new Point(65, 45), Age = 0.35 },
                        new RibbonNode { Position = new Point(125, 125), Age = 0.2 },
                        new RibbonNode { Position = new Point(180, 40), Age = 0.1 },
                        new RibbonNode { Position = new Point(225, 80) }
                    }, settings);
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
        }

        private static void DrawRibbon(DrawingContext context, IReadOnlyList<RibbonNode> nodes, MouseEffectSetting settings)
        {
            if (nodes.Count < 2) return;
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
            {
                path.BeginFigure(nodes[0].Position, false, false);
                for (var i = 0; i < nodes.Count; i++)
                {
                    var end = i == nodes.Count - 1 ? nodes[i].Position : Midpoint(nodes[i].Position, nodes[i + 1].Position);
                    path.QuadraticBezierTo(nodes[i].Position, end, true, true);
                }
            }
            geometry.Freeze();
            var start = nodes[0].Position;
            var finish = nodes[nodes.Count - 1].Position;
            if ((finish - start).LengthSquared < 0.000001)
            {
                start = geometry.Bounds.TopLeft;
                finish = geometry.Bounds.BottomRight;
            }
            if ((finish - start).LengthSquared < 0.000001) return;
            var colors = new[]
            {
                (Color)ColorConverter.ConvertFromString(settings.NeonRibbonColor1),
                (Color)ColorConverter.ConvertFromString(settings.NeonRibbonColor2),
                (Color)ColorConverter.ConvertFromString(settings.NeonRibbonColor3)
            };
            var fade = Math.Clamp(1 - nodes[nodes.Count - 1].Age / Lifetime, 0, 1);
            for (var layer = 0; layer < LayerWidths.Length; layer++)
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = start,
                    EndPoint = finish,
                    MappingMode = BrushMappingMode.Absolute,
                    Opacity = fade * fade * LayerOpacities[layer]
                };
                for (var i = 0; i < colors.Length; i++)
                {
                    var color = layer == 3 ? Blend(colors[i], Colors.White, 0.8) : colors[i];
                    brush.GradientStops.Add(new GradientStop(color, i * 0.5));
                }
                brush.Freeze();
                var pen = new Pen(brush, 12 * Math.Sqrt(fade) * LayerWidths[layer])
                {
                    StartLineCap = PenLineCap.Flat,
                    EndLineCap = PenLineCap.Flat,
                    LineJoin = PenLineJoin.Round
                };
                pen.Freeze();
                context.DrawGeometry(null, pen, geometry);
            }
        }

        private static Color Blend(Color first, Color second, double amount)
        {
            return Color.FromRgb((byte)Math.Round(first.R + (second.R - first.R) * amount),
                (byte)Math.Round(first.G + (second.G - first.G) * amount),
                (byte)Math.Round(first.B + (second.B - first.B) * amount));
        }

        private static Point Midpoint(Point first, Point second) => first + (second - first) * 0.5;

        private struct RibbonNode
        {
            public Point Position;
            public double Age;
        }
    }
}
