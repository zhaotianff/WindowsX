using System;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FollowingAnimalCursorEffect : UIElement
    {
        private static readonly Brush Fur = CreateBrush(Color.FromRgb(244, 172, 82));
        private static readonly Brush Cream = CreateBrush(Color.FromRgb(255, 234, 194));
        private static readonly Brush Pink = CreateBrush(Color.FromRgb(231, 139, 145));
        private static readonly Pen Outline = CreatePen(Color.FromRgb(115, 72, 43), 1);
        private static readonly Pen TailPen = CreatePen(Color.FromRgb(244, 172, 82), 5);
        private static readonly DrawingGroup CatBody = CreateBody();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private Point? position;
        private double cycleTime;
        private int direction = 1;

        protected override int VisualChildrenCount => 1;

        public FollowingAnimalCursorEffect(MouseEffectSetting settings = null)
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

            var target = cursor + new Vector(-36, 28);
            if (!double.IsFinite(target.X) || !double.IsFinite(target.Y)) return;
            var moving = false;
            if (!position.HasValue)
                position = target;
            else
            {
                var delta = target - position.Value;
                if (!double.IsFinite(delta.X) || !double.IsFinite(delta.Y)) return;
                if (Math.Abs(delta.X) > 1) direction = delta.X > 0 ? 1 : -1;
                var amount = 1 - Math.Exp(-elapsedSeconds * 9);
                position = position.Value + delta * amount;
                moving = (target - position.Value).Length > 1;
            }
            cycleTime = (cycleTime + elapsedSeconds % 2) % 2;
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.FollowingAnimalOpacity);
            DrawCat(context, position.Value, direction, cycleTime, moving);
            context.Pop();
        }

        public void Clear()
        {
            position = null;
            cycleTime = 0;
            direction = 1;
            using (visual.RenderOpen()) { }
        }

        private static void DrawCat(DrawingContext context, Point center, int facing, double time, bool moving)
        {
            var bob = moving ? Math.Sin(time * Math.PI * 8) * 1.2 : 0;
            context.PushTransform(new MatrixTransform(facing, 0, 0, 1, center.X, center.Y + bob));
            var wag = Math.Sin(time * Math.PI) * 4;
            var tail = new StreamGeometry();
            using (var path = tail.Open())
            {
                path.BeginFigure(new Point(-11, 2), false, false);
                path.BezierTo(new Point(-24, 0), new Point(-24 + wag, -13), new Point(-19 + wag, -14), true, true);
            }
            context.DrawGeometry(null, TailPen, tail);
            var step = moving ? Math.Sin(time * Math.PI * 8) * 2 : 0;
            context.DrawEllipse(Fur, Outline, new Point(-7 + step, 11), 4, 2.5);
            context.DrawEllipse(Fur, Outline, new Point(8 - step, 11), 4, 2.5);
            context.DrawDrawing(CatBody);
            context.Pop();
        }

        private static DrawingGroup CreateBody()
        {
            var drawing = new DrawingGroup();
            using (var context = drawing.Open())
            {
                context.DrawEllipse(Fur, Outline, new Point(-2, 2), 13, 9);
                context.DrawEllipse(Cream, null, new Point(0, 5), 7, 4);
                context.DrawGeometry(Fur, Outline, Triangle(new Point(0, -10), new Point(-1, -23), new Point(9, -17)));
                context.DrawGeometry(Fur, Outline, Triangle(new Point(13, -17), new Point(23, -22), new Point(22, -9)));
                context.DrawGeometry(Pink, null, Triangle(new Point(2, -13), new Point(2, -19), new Point(7, -16)));
                context.DrawGeometry(Pink, null, Triangle(new Point(16, -16), new Point(21, -19), new Point(20, -12)));
                context.DrawEllipse(Fur, Outline, new Point(11, -8), 12, 10);
                context.DrawEllipse(Cream, null, new Point(14, -4), 7, 4);
                context.DrawEllipse(Outline.Brush, null, new Point(7, -9), 1.3, 2);
                context.DrawEllipse(Outline.Brush, null, new Point(17, -9), 1.3, 2);
                context.DrawGeometry(Pink, null, Triangle(new Point(10, -5), new Point(14, -5), new Point(12, -2)));
                context.DrawLine(Outline, new Point(12, -2), new Point(12, 0));
                context.DrawLine(Outline, new Point(5, -4), new Point(-3, -6));
                context.DrawLine(Outline, new Point(5, -2), new Point(-3, -1));
                context.DrawLine(Outline, new Point(19, -4), new Point(27, -6));
                context.DrawLine(Outline, new Point(19, -2), new Point(27, -1));
                context.DrawLine(Outline, new Point(9, -17), new Point(10, -14));
                context.DrawLine(Outline, new Point(14, -17), new Point(13, -14));
            }
            drawing.Freeze();
            return drawing;
        }

        private static Geometry Triangle(Point a, Point b, Point c)
        {
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
            {
                path.BeginFigure(a, true, true);
                path.LineTo(b, true, true);
                path.LineTo(c, true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        private static Brush CreateBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static Pen CreatePen(Color color, double width)
        {
            var pen = new Pen(CreateBrush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
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
                    context.DrawRoundedRectangle(CreateBrush(Color.FromRgb(35, 38, 52)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new ScaleTransform(2.4, 2.4, 125, 95));
                    DrawCat(context, new Point(125, 95), 1, 0.3, false);
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }
    }
}
