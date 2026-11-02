using System;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public class FollowingDotEffect : UIElement, ICursorEffect
    {
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private RadialGradientBrush brush;
        private string color;
        private double centerOpacity;
        private double edgeOpacity;

        protected override int VisualChildrenCount => 1;

        public FollowingDotEffect(MouseEffectSetting settings = null)
        {
            this.settings = settings ?? new MouseEffectSetting();
            IsHitTestVisible = false;
            AddVisualChild(visual);
        }

        public FollowingDotEffect(Point point) : this(point, new MouseEffectSetting())
        {
        }

        public FollowingDotEffect(Point point, MouseEffectSetting settings) : this(settings)
        {
            Update(point, 1.0 / 60);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            return visual;
        }

        public void Update(Point cursor, double elapsedSeconds)
        {
            if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y))
                return;

            if (brush == null || color != settings.FollowingColor || centerOpacity != settings.FollowingCenterOpacity || edgeOpacity != settings.FollowingEdgeOpacity)
            {
                brush = CreateGradientBrush(settings);
                color = settings.FollowingColor;
                centerOpacity = settings.FollowingCenterOpacity;
                edgeOpacity = settings.FollowingEdgeOpacity;
            }

            using var context = visual.RenderOpen();
            context.DrawEllipse(brush, null, cursor, settings.FollowingSize / 2, settings.FollowingSize / 2);
        }

        public void Clear()
        {
            using (visual.RenderOpen()) { }
        }

        public static RadialGradientBrush CreateGradientBrush(MouseEffectSetting settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var color = (Color)ColorConverter.ConvertFromString(settings.FollowingColor);
            var centerColor = Color.FromArgb((byte)Math.Round(settings.FollowingCenterOpacity * 255), color.R, color.G, color.B);
            var edgeColor = Color.FromArgb((byte)Math.Round(settings.FollowingEdgeOpacity * 255), color.R, color.G, color.B);
            var brush = new RadialGradientBrush(centerColor, edgeColor)
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(24, 27, 45)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new ScaleTransform(3, 3, 130, 85));
                    context.DrawDrawing(CreateVisual(new Point(130, 85), new MouseEffectSetting()).Drawing);
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private static DrawingVisual CreateVisual(Point point, MouseEffectSetting settings)
        {
            var drawingVisual = new DrawingVisual();
            using var context = drawingVisual.RenderOpen();
            context.DrawEllipse(CreateGradientBrush(settings), null, point, settings.FollowingSize / 2, settings.FollowingSize / 2);
            return drawingVisual;
        }
    }
}
