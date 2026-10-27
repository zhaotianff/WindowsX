using System;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public class FollowingDotEffect : MouseCursorEffectAbstract
    {
        private readonly MouseEffectSetting settings;

        public FollowingDotEffect(Point point) : this(point, new MouseEffectSetting())
        {
        }

        public FollowingDotEffect(Point point, MouseEffectSetting settings) : base(CreateVisual(point, settings))
        {
            this.settings = settings;
            FixedCount = 1;
            IsHitTestVisible = false;
        }

        public override DrawingVisual CreateDrawingVisualEllipsed(Point point)
        {
            return CreateVisual(point, settings);
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
