using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class WaterRippleCursorEffect : UIElement
    {
        public const int MaxRipples = 24;
        public const double RingLifetime = 1.2;
        public const double RingDelay = 0.24;
        public const double Lifetime = RingLifetime + RingDelay * 2;
        public const double EmissionInterval = 0.14;
        private static readonly Pen HaloPen = CreatePen(Color.FromArgb(55, 75, 190, 255), 6);
        private static readonly Pen RingPen = CreatePen(Color.FromRgb(105, 210, 255), 1.8);
        private static readonly Pen HighlightPen = CreatePen(Color.FromArgb(160, 225, 250, 255), 0.7);
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Ripple> ripples = new List<Ripple>(MaxRipples);
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;

        public int RippleCount => ripples.Count;
        protected override int VisualChildrenCount => 1;

        public WaterRippleCursorEffect(MouseEffectSetting settings = null)
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

            for (var i = ripples.Count - 1; i >= 0; i--)
            {
                var ripple = ripples[i];
                ripple.Age += elapsedSeconds;
                if (ripple.Age >= Lifetime) ripples.RemoveAt(i);
                else ripples[i] = ripple;
            }

            elapsedSinceEmission = Math.Min(EmissionInterval, elapsedSinceEmission + elapsedSeconds);
            var distance = lastEmissionPoint.HasValue ? (cursor - lastEmissionPoint.Value).Length : 0;
            if (!lastEmissionPoint.HasValue || (elapsedSinceEmission >= EmissionInterval && double.IsFinite(distance) && distance >= 12))
            {
                if (ripples.Count == MaxRipples) ripples.RemoveAt(0);
                ripples.Add(new Ripple { Center = cursor });
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }

            using var context = visual.RenderOpen();
            context.PushOpacity(settings.WaterRippleOpacity);
            foreach (var ripple in ripples) DrawRipple(context, ripple);
            context.Pop();
        }

        public void Clear()
        {
            ripples.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            using (visual.RenderOpen()) { }
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(19, 47, 66)), null, new Rect(0, 0, 260, 170), 10, 10);
                    DrawRipple(context, new Ripple { Center = new Point(95, 90), Age = 0.8 });
                    DrawRipple(context, new Ripple { Center = new Point(192, 65), Age = 0.5 });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private static void DrawRipple(DrawingContext context, Ripple ripple)
        {
            for (var ring = 0; ring < 3; ring++)
            {
                var age = ripple.Age - ring * RingDelay;
                if (age <= 0 || age >= RingLifetime) continue;
                var progress = age / RingLifetime;
                var radius = 6 + 54 * (1 - Math.Pow(1 - progress, 2));
                var opacity = Math.Min(1, age / 0.08) * Math.Pow(1 - progress, 2);
                context.PushOpacity(opacity);
                context.DrawEllipse(null, HaloPen, ripple.Center, radius, radius);
                context.DrawEllipse(null, RingPen, ripple.Center, radius, radius);
                context.DrawEllipse(null, HighlightPen, ripple.Center, radius - 1.2, radius - 1.2);
                context.Pop();
            }
        }

        private static Pen CreatePen(Color color, double width)
        {
            var pen = new Pen(new SolidColorBrush(color), width);
            pen.Freeze();
            return pen;
        }

        private struct Ripple
        {
            public Point Center;
            public double Age;
        }
    }
}
