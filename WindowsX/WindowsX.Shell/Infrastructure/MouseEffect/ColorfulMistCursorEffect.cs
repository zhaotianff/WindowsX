using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class ColorfulMistCursorEffect : UIElement,ICursorEffect
    {
        public const int MaxParticles = 160;
        public const int MaxEmissionPerUpdate = 32;
        private static readonly Brush[] MistBrushes = CreateMistBrushes();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<MistParticle> particles = new List<MistParticle>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextColor;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public ColorfulMistCursorEffect(MouseEffectSetting settings = null)
        {
            this.settings = settings ?? new MouseEffectSetting();
            IsHitTestVisible = false;
            AddVisualChild(visual);
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

            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.Age += elapsedSeconds;
                if (particle.Age >= particle.Lifetime) particles.RemoveAt(i);
                else particles[i] = particle;
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
                if (double.IsFinite(distance) && distance >= 6)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 12));
                    for (var i = 1; i <= count; i++)
                        Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.ColorfulMistOpacity);
            foreach (var particle in particles) DrawMist(context, particle);
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
            particles.Add(new MistParticle
            {
                Origin = point,
                Radius = 18 + random.NextDouble() * 8,
                Lifetime = 1.8 + random.NextDouble() * 0.8,
                Velocity = new Vector(random.NextDouble() * 12 - 6, -8 - random.NextDouble() * 8),
                Phase = random.NextDouble() * Math.PI * 2,
                ColorIndex = nextColor
            });
            nextColor = (nextColor + 1) % MistBrushes.Length;
        }

        private static void DrawMist(DrawingContext context, MistParticle particle)
        {
            var age = particle.Age;
            var progress = age / particle.Lifetime;
            var center = particle.Origin + particle.Velocity * age + new Vector(
                6 * (Math.Sin(particle.Phase + age * 1.5) - Math.Sin(particle.Phase)), 0);
            var radius = particle.Radius * (1 + progress * 1.4);
            var offset = new Vector(Math.Cos(particle.Phase + age) * radius * 0.3, Math.Sin(particle.Phase + age) * radius * 0.2);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.6));
            context.DrawEllipse(MistBrushes[particle.ColorIndex], null, center, radius, radius * 0.8);
            context.DrawEllipse(MistBrushes[(particle.ColorIndex + 1) % MistBrushes.Length], null,
                center + offset, radius * 0.85, radius * 0.95);
            context.Pop();
        }

        private static Brush[] CreateMistBrushes()
        {
            var colors = new[]
            {
                Color.FromRgb(74, 213, 255), Color.FromRgb(151, 112, 255),
                Color.FromRgb(255, 119, 201), Color.FromRgb(255, 186, 115),
                Color.FromRgb(117, 238, 196)
            };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var brush = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(85, color.R, color.G, color.B), 0),
                        new GradientStop(Color.FromArgb(55, color.R, color.G, color.B), 0.3),
                        new GradientStop(Color.FromArgb(16, color.R, color.G, color.B), 0.65),
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(25, 28, 45)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 18; i++)
                        DrawMist(context, new MistParticle
                        {
                            Origin = new Point(30 + i * 12, 90 + Math.Sin(i * 0.3) * 25),
                            Radius = 22, Lifetime = 2.4, Age = (17 - i) * 0.035,
                            Velocity = new Vector(0, -10), Phase = i * 0.7, ColorIndex = i % MistBrushes.Length
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct MistParticle
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
