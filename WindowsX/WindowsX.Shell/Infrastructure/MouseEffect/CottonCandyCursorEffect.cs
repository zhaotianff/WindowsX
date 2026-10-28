using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class CottonCandyCursorEffect : UIElement,ICursorEffect
    {
        public const int MaxParticles = 64;
        public const int MaxEmissionPerUpdate = 16;
        private static readonly Brush[] PuffBrushes = CreateBrushes();
        private static readonly Point[] Lobes =
        {
            new Point(-0.55, 0.08), new Point(-0.3, -0.4), new Point(0.2, -0.48),
            new Point(0.58, -0.05), new Point(0.24, 0.35), new Point(-0.2, 0.3)
        };
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Puff> particles = new List<Puff>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextColor;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public CottonCandyCursorEffect(MouseEffectSetting settings = null)
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

            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var puff = particles[i];
                puff.Age += elapsedSeconds;
                if (puff.Age >= puff.Lifetime) particles.RemoveAt(i);
                else particles[i] = puff;
            }
            elapsedSinceEmission = Math.Min(1, elapsedSinceEmission + elapsedSeconds);
            if (!lastEmissionPoint.HasValue)
            {
                Emit(cursor);
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }
            else
            {
                var movement = cursor - lastEmissionPoint.Value;
                var distance = movement.Length;
                if (double.IsFinite(distance) && distance >= 8 && elapsedSinceEmission >= 0.025)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 10));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.CottonCandyOpacity);
            foreach (var puff in particles) DrawPuff(context, puff);
            context.Pop();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            nextColor = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new Puff
            {
                Origin = point,
                Radius = 9 + random.NextDouble() * 4,
                Lifetime = 0.9 + random.NextDouble() * 0.4,
                Velocity = new Vector(random.NextDouble() * 6 - 3, -6 - random.NextDouble() * 5),
                Phase = random.NextDouble() * Math.PI * 2,
                ColorIndex = nextColor
            });
            nextColor = (nextColor + 1) % PuffBrushes.Length;
        }

        private static void DrawPuff(DrawingContext context, Puff puff)
        {
            var progress = puff.Age / puff.Lifetime;
            var center = puff.Origin + puff.Velocity * puff.Age;
            var radius = puff.Radius * (1 + progress * 0.45);
            var transform = Matrix.Identity;
            transform.Rotate(Math.Sin(puff.Phase + puff.Age * 2) * 10);
            transform.Translate(center.X, center.Y);
            context.PushOpacity(0.8 * Math.Pow(Math.Max(0, 1 - progress), 1.5));
            context.PushTransform(new MatrixTransform(transform));
            for (var i = 0; i < Lobes.Length; i++)
            {
                var lobe = Lobes[i];
                var size = radius * (i % 2 == 0 ? 0.68 : 0.76);
                context.DrawEllipse(PuffBrushes[puff.ColorIndex], null, new Point(lobe.X * radius, lobe.Y * radius), size, size * 0.9);
            }
            context.DrawEllipse(PuffBrushes[puff.ColorIndex], null, new Point(0, 0), radius * 0.7, radius * 0.6);
            context.Pop();
            context.Pop();
        }

        private static Brush[] CreateBrushes()
        {
            var colors = new[] { Color.FromRgb(255, 195, 217), Color.FromRgb(244, 224, 193), Color.FromRgb(217, 200, 247) };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var brush = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.35, 0.3),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(245, 255, 251, 246), 0),
                        new GradientStop(Color.FromArgb(230, color.R, color.G, color.B), 0.62),
                        new GradientStop(Color.FromArgb(150, color.R, color.G, color.B), 0.85),
                        new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(47, 43, 62)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 13; i++)
                        DrawPuff(context, new Puff
                        {
                            Origin = new Point(38 + i * 15, 85 + Math.Sin(i * 0.3) * 17),
                            Radius = 13, Lifetime = 1.2, Age = (12 - i) * 0.055,
                            Velocity = new Vector(0, -8), Phase = i * 0.7, ColorIndex = i % PuffBrushes.Length
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Puff
        {
            public Point Origin;
            public Vector Velocity;
            public double Radius;
            public double Lifetime;
            public double Age;
            public double Phase;
            public int ColorIndex;
        }
    }
}
