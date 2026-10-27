using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class InkFluidCursorEffect : UIElement
    {
        public const int MaxParticles = 192;
        public const int MaxEmissionPerUpdate = 32;

        private readonly Brush[] inkBrushes = new Brush[3];
        private readonly string[] inkColors = new string[3];

        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<InkParticle> particles = new List<InkParticle>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public InkFluidCursorEffect(MouseEffectSetting settings = null)
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

            var step = Math.Min(elapsedSeconds, 0.05);
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.Age += elapsedSeconds;
                if (particle.Age >= particle.Lifetime)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                var angle = particle.Phase + particle.Age * particle.Spin;
                var curl = new Vector(-Math.Sin(angle), Math.Cos(angle)) * 18;
                particle.Velocity *= Math.Exp(-2.2 * step);
                particle.Position += (particle.Velocity + curl) * step;
                particles[i] = particle;
            }

            elapsedSinceEmission += elapsedSeconds;
            if (!lastEmissionPoint.HasValue)
            {
                Emit(cursor, new Vector());
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }
            else
            {
                var movement = cursor - lastEmissionPoint.Value;
                var distance = movement.Length;
                if (double.IsFinite(distance) && distance >= 3)
                {
                    var speed = Math.Min(distance / Math.Max(elapsedSinceEmission, 1.0 / 240), 600) * 0.12;
                    var velocity = movement / distance * speed;
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 6));
                    for (var i = 1; i <= count; i++)
                        Emit(lastEmissionPoint.Value + movement * ((double)i / count), velocity);
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }

            Draw();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point, Vector velocity)
        {
            if (particles.Count == MaxParticles)
                particles.RemoveAt(0);

            particles.Add(new InkParticle
            {
                Position = point,
                Velocity = velocity + new Vector(random.NextDouble() * 12 - 6, random.NextDouble() * 12 - 6),
                Radius = 10 + random.NextDouble() * 6,
                Lifetime = 1.5 + random.NextDouble() * 0.9,
                Phase = random.NextDouble() * Math.PI * 2,
                Spin = (random.Next(2) == 0 ? -1 : 1) * (1.2 + random.NextDouble()),
                ColorIndex = random.Next(inkBrushes.Length)
            });
        }

        private void Draw()
        {
            UpdateInkBrush(0, settings.InkFluidColor1);
            UpdateInkBrush(1, settings.InkFluidColor2);
            UpdateInkBrush(2, settings.InkFluidColor3);
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.InkFluidOpacity);
            foreach (var particle in particles)
            {
                var progress = particle.Age / particle.Lifetime;
                var radius = particle.Radius + 24 * progress;
                var angle = particle.Phase + particle.Age * particle.Spin;
                var curlOffset = new Vector(Math.Cos(angle), Math.Sin(angle)) * (1 - Math.Exp(-particle.Age * 3)) * radius * 0.35;
                var center = particle.Position + curlOffset;
                context.PushOpacity(Math.Pow(1 - progress, 1.5));
                context.PushTransform(new RotateTransform(angle * 180 / Math.PI, center.X, center.Y));
                context.DrawEllipse(inkBrushes[particle.ColorIndex], null, center, radius * 1.2, radius * 0.85);
                context.Pop();
                context.DrawEllipse(inkBrushes[(particle.ColorIndex + 1) % inkBrushes.Length], null,
                    particle.Position - curlOffset, radius * 0.6, radius * 0.6);
                context.Pop();
            }
            context.Pop();
        }

        private void UpdateInkBrush(int index, string color)
        {
            if (inkColors[index] == color)
                return;

            inkBrushes[index] = CreateInkBrush((Color)ColorConverter.ConvertFromString(color));
            inkColors[index] = color;
        }

        private static Brush CreateInkBrush(Color color)
        {
            var brush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(200, color.R, color.G, color.B), 0),
                    new GradientStop(Color.FromArgb(120, color.R, color.G, color.B), 0.3),
                    new GradientStop(Color.FromArgb(35, color.R, color.G, color.B), 0.65),
                    new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
                }
            };
            brush.Freeze();
            return brush;
        }

        private struct InkParticle
        {
            public Point Position;
            public Vector Velocity;
            public double Radius;
            public double Lifetime;
            public double Age;
            public double Phase;
            public double Spin;
            public int ColorIndex;
        }
    }
}
