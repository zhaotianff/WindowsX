using System;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class HaloRingCursorEffect : UIElement,ICursorEffect
    {
        private static readonly double[] LayerOpacities = { 0.05, 0.09, 0.16, 1 };
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private Pen[] ringPens;
        private double sizeScale;
        private string color1;
        private string color2;
        private string color3;
        private double cyclePhase;

        protected override int VisualChildrenCount => 1;

        public HaloRingCursorEffect(MouseEffectSetting settings = null)
        {
            this.settings = settings ?? new MouseEffectSetting();
            RefreshPens();
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

            var period = 1 / settings.HaloRingBreathingFrequency;
            cyclePhase = (cyclePhase + elapsedSeconds % period / period) % 1;
            RefreshPens();
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.HaloRingOpacity);
            DrawRing(context, cursor, cyclePhase, sizeScale, ringPens);
            context.Pop();
        }

        public void Clear()
        {
            cyclePhase = 0;
            using (visual.RenderOpen()) { }
        }

        private void RefreshPens()
        {
            if (ringPens != null && sizeScale == settings.HaloRingSizeScale && color1 == settings.HaloRingColor1
                && color2 == settings.HaloRingColor2 && color3 == settings.HaloRingColor3)
                return;

            ringPens = CreatePens(settings);
            sizeScale = settings.HaloRingSizeScale;
            color1 = settings.HaloRingColor1;
            color2 = settings.HaloRingColor2;
            color3 = settings.HaloRingColor3;
        }

        private static void DrawRing(DrawingContext context, Point center, double phase, double scale, Pen[] pens)
        {
            var breath = (1 - Math.Cos(phase * Math.PI * 2)) * 0.5;
            var radius = (22 + 4 * breath) * scale;
            context.PushOpacity(0.55 + 0.45 * breath);
            for (var i = 0; i < pens.Length; i++)
            {
                context.PushOpacity(LayerOpacities[i]);
                context.DrawEllipse(null, pens[i], center, radius, radius);
                context.Pop();
            }
            context.Pop();
        }

        private static Pen[] CreatePens(MouseEffectSetting settings)
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0), EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString(settings.HaloRingColor1), 0),
                    new GradientStop((Color)ColorConverter.ConvertFromString(settings.HaloRingColor2), 0.5),
                    new GradientStop((Color)ColorConverter.ConvertFromString(settings.HaloRingColor3), 1)
                }
            };
            brush.Freeze();
            var widths = new[] { 12.0, 8, 5, 2 };
            var pens = new Pen[widths.Length];
            for (var i = 0; i < widths.Length; i++)
            {
                pens[i] = new Pen(brush, widths[i] * settings.HaloRingSizeScale);
                pens[i].Freeze();
            }
            return pens;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(24, 27, 45)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new ScaleTransform(1.8, 1.8, 130, 85));
                    DrawRing(context, new Point(130, 85), 0.5, 1, CreatePens(new MouseEffectSetting()));
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }
    }
}
