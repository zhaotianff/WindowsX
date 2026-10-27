using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FallingPetalsCursorEffect : UIElement
    {
        public const int MaxParticles = 160;
        public const int MaxEmissionPerUpdate = 8;
        private static readonly Geometry[] PetalGeometries =
        {
            CreatePetalGeometry(0), CreatePetalGeometry(1), CreatePetalGeometry(2)
        };
        private static readonly Brush PetalBrush = CreatePetalBrush();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Petal> particles = new List<Petal>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public FallingPetalsCursorEffect(MouseEffectSetting settings = null)
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
                var petal = particles[i];
                petal.Age += elapsedSeconds;
                if (petal.Age >= petal.Lifetime) particles.RemoveAt(i);
                else particles[i] = petal;
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
                if (double.IsFinite(distance) && distance >= 12 && elapsedSinceEmission >= 0.04)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 24));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.FallingPetalsOpacity);
            foreach (var petal in particles) DrawPetal(context, petal);
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
            particles.Add(new Petal
            {
                Origin = point + new Vector(random.NextDouble() * 12 - 6, random.NextDouble() * 8 - 4),
                Size = 3 + random.NextDouble() * 6,
                Kind = random.Next(PetalGeometries.Length),
                Lifetime = 2.4 + random.NextDouble() * 1.2,
                FallSpeed = 22 + random.NextDouble() * 14,
                Drift = random.NextDouble() * 12 - 6,
                Phase = random.NextDouble() * Math.PI * 2,
                Spin = (random.Next(2) == 0 ? -1 : 1) * (25 + random.NextDouble() * 30)
            });
        }

        private static void DrawPetal(DrawingContext context, Petal petal)
        {
            var age = petal.Age;
            var center = petal.Origin + new Vector(petal.Drift * age + 12 * (Math.Sin(petal.Phase + age * 1.8) - Math.Sin(petal.Phase)),
                petal.FallSpeed * age + 3 * age * age);
            var transform = Matrix.Identity;
            var flutter = 0.65 + 0.35 * Math.Cos(petal.Phase + age * 2.4);
            transform.Scale(petal.Size * flutter, petal.Size);
            transform.Rotate(petal.Phase * 180 / Math.PI + petal.Spin * age);
            transform.Translate(center.X, center.Y);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - age / petal.Lifetime), 1.3));
            context.PushTransform(new MatrixTransform(transform));
            context.DrawGeometry(PetalBrush, null, PetalGeometries[petal.Kind]);
            context.Pop();
            context.Pop();
        }

        private static Geometry CreatePetalGeometry(int kind)
        {
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
            {
                path.BeginFigure(new Point(0, 1), true, true);
                if (kind == 1)
                {
                    path.BezierTo(new Point(-0.7, 0.45), new Point(-0.7, -0.9), new Point(0, -0.9), true, true);
                    path.BezierTo(new Point(0.7, -0.9), new Point(0.7, 0.45), new Point(0, 1), true, true);
                }
                else if (kind == 2)
                {
                    path.BezierTo(new Point(-0.55, 0.25), new Point(-0.35, -0.55), new Point(0.25, -0.9), true, true);
                    path.BezierTo(new Point(0.6, -0.3), new Point(0.4, 0.6), new Point(0, 1), true, true);
                }
                else
                {
                    path.BezierTo(new Point(-1.1, 0.3), new Point(-1, -1), new Point(-0.25, -0.9), true, true);
                    path.QuadraticBezierTo(new Point(-0.06, -0.86), new Point(0, -0.62), true, true);
                    path.QuadraticBezierTo(new Point(0.22, -1.05), new Point(0.65, -0.65), true, true);
                    path.BezierTo(new Point(1.15, -0.12), new Point(0.65, 0.65), new Point(0, 1), true, true);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static Brush CreatePetalBrush()
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0), EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(255, 222, 235), 0),
                    new GradientStop(Color.FromRgb(255, 182, 193), 0.45),
                    new GradientStop(Color.FromRgb(238, 119, 164), 1)
                }
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(55, 47, 68)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 9; i++)
                        DrawPetal(context, new Petal
                        {
                            Origin = new Point(35 + i * 23, 35 + i % 3 * 28),
                            Kind = i % 3, Size = 3 + i % 4 * 2, Lifetime = 3.5, Age = (8 - i) * 0.1,
                            FallSpeed = 25, Phase = i * 1.3, Spin = 35
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Petal
        {
            public Point Origin;
            public int Kind;
            public double Size;
            public double Lifetime;
            public double Age;
            public double FallSpeed;
            public double Drift;
            public double Phase;
            public double Spin;
        }
    }
}
