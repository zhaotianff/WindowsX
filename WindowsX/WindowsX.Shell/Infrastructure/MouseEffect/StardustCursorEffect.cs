using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class StardustCursorEffect : UIElement,ICursorEffect
    {
        public const int MaxParticles = 256;
        public const int MaxEmissionPerUpdate = 24;
        private static readonly Color[] Palette = { Color.FromRgb(255, 244, 213), Color.FromRgb(255, 221, 145), Color.FromRgb(181, 224, 255) };
        private static readonly Brush[] CoreBrushes = CreateBrushes(false);
        private static readonly Brush[] HaloBrushes = CreateBrushes(true);
        private static readonly Pen[] StarPens = CreatePens();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<DustParticle> particles = new List<DustParticle>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public StardustCursorEffect(MouseEffectSetting settings = null)
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
                for (var i = 0; i < 3; i++) Emit(cursor);
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }
            else
            {
                var movement = cursor - lastEmissionPoint.Value;
                var distance = movement.Length;
                if (double.IsFinite(distance) && distance >= 4)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 8));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }

            using var context = visual.RenderOpen();
            context.PushOpacity(settings.StardustOpacity);
            foreach (var particle in particles) DrawParticle(context, particle, settings.StardustSizeScale);
            context.Pop();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new DustParticle
            {
                Origin = point + new Vector(random.NextDouble() * 12 - 6, random.NextDouble() * 12 - 6),
                Velocity = new Vector(random.NextDouble() * 8 - 4, random.NextDouble() * 6 - 1),
                Lifetime = 1.2 + random.NextDouble() * 0.8,
                Size = 0.8 + random.NextDouble() * 1.2,
                Phase = random.NextDouble() * Math.PI * 2,
                Frequency = 1.2 + random.NextDouble() * 0.8,
                ColorIndex = random.Next(Palette.Length),
                HasRays = random.Next(4) == 0
            });
        }

        private static double Twinkle(double age, double phase, double frequency)
        {
            return 0.88 + 0.12 * Math.Sin(age * Math.PI * 2 * frequency + phase);
        }

        private static void DrawParticle(DrawingContext context, DustParticle particle, double sizeScale = 1)
        {
            var progress = particle.Age / particle.Lifetime;
            var opacity = Math.Pow(Math.Max(0, 1 - progress), 1.5) * Twinkle(particle.Age, particle.Phase, particle.Frequency);
            var center = particle.Origin + particle.Velocity * (1 - Math.Exp(-particle.Age));
            var radius = particle.Size * (1 - progress * 0.25);
            context.PushOpacity(opacity);
            if (sizeScale != 1)
                context.PushTransform(new ScaleTransform(sizeScale, sizeScale, center.X, center.Y));
            context.DrawEllipse(HaloBrushes[particle.ColorIndex], null, center, radius * 3, radius * 3);
            context.DrawEllipse(CoreBrushes[particle.ColorIndex], null, center, radius, radius);
            if (particle.HasRays)
            {
                var horizontal = new Vector(radius * 2.5, 0);
                var vertical = new Vector(0, radius * 2.5);
                context.DrawLine(StarPens[particle.ColorIndex], center - horizontal, center + horizontal);
                context.DrawLine(StarPens[particle.ColorIndex], center - vertical, center + vertical);
            }
            if (sizeScale != 1)
                context.Pop();
            context.Pop();
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(20, 23, 46)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 30; i++)
                    {
                        var progress = i / 29.0;
                        DrawParticle(context, new DustParticle
                        {
                            Origin = new Point(25 + progress * 210, 90 - 30 * Math.Sin(progress * Math.PI * 2) + (i % 3 - 1) * 12),
                            Lifetime = 2,
                            Age = (1 - progress) * 1.1,
                            Size = 1.2 + i % 3 * 0.4,
                            Phase = i,
                            Frequency = 1.5,
                            ColorIndex = i % Palette.Length,
                            HasRays = i % 4 == 0
                        });
                    }
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private static Brush[] CreateBrushes(bool halo)
        {
            var brushes = new Brush[Palette.Length];
            for (var i = 0; i < brushes.Length; i++)
            {
                var color = Palette[i];
                Brush brush = halo
                    ? new RadialGradientBrush(Color.FromArgb(90, color.R, color.G, color.B), Color.FromArgb(0, color.R, color.G, color.B))
                    : new SolidColorBrush(color);
                brush.Freeze();
                brushes[i] = brush;
            }
            return brushes;
        }

        private static Pen[] CreatePens()
        {
            var pens = new Pen[Palette.Length];
            for (var i = 0; i < pens.Length; i++)
            {
                pens[i] = new Pen(CoreBrushes[i], 0.6) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                pens[i].Freeze();
            }
            return pens;
        }

        private struct DustParticle
        {
            public Point Origin;
            public Vector Velocity;
            public double Age;
            public double Lifetime;
            public double Size;
            public double Phase;
            public double Frequency;
            public int ColorIndex;
            public bool HasRays;
        }
    }
}
