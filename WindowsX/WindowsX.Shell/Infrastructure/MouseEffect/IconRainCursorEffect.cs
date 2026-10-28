using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class IconRainCursorEffect : UIElement,ICursorEffect
    {
        public const int MaxParticles = 120;
        public const int MaxEmissionPerUpdate = 6;
        private static readonly Geometry[] IconGeometries = CreateGeometries();
        private static readonly Brush[] IconBrushes = CreateBrushes();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<IconParticle> particles = new List<IconParticle>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextKind;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public IconRainCursorEffect(MouseEffectSetting settings = null)
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
                if (double.IsFinite(distance) && distance >= 12 && elapsedSinceEmission >= 0.06)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 24));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.IconRainOpacity);
            foreach (var particle in particles) DrawIcon(context, particle);
            context.Pop();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            nextKind = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new IconParticle
            {
                Origin = point,
                Velocity = new Vector(random.NextDouble() * 50 - 25, -45 - random.NextDouble() * 30),
                Size = 6 + random.NextDouble() * 4,
                Lifetime = 1.8 + random.NextDouble() * 0.6,
                Angle = random.NextDouble() * 40 - 20,
                Spin = random.NextDouble() * 70 - 35,
                Kind = nextKind
            });
            nextKind = (nextKind + 1) % IconGeometries.Length;
        }

        private static void DrawIcon(DrawingContext context, IconParticle particle)
        {
            var age = particle.Age;
            var center = particle.Origin + particle.Velocity * age + new Vector(0, 65 * age * age);
            var scale = particle.Size * (0.65 + 0.35 * Math.Min(1, age / 0.12));
            var transform = Matrix.Identity;
            transform.Scale(scale, scale);
            transform.Rotate(particle.Angle + particle.Spin * age);
            transform.Translate(center.X, center.Y);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - age / particle.Lifetime), 1.3));
            context.PushTransform(new MatrixTransform(transform));
            context.DrawGeometry(IconBrushes[particle.Kind], null, IconGeometries[particle.Kind]);
            context.Pop();
            context.Pop();
        }

        private static Geometry[] CreateGeometries()
        {
            var star = new StreamGeometry();
            using (var path = star.Open())
            {
                for (var i = 0; i < 10; i++)
                {
                    var angle = -Math.PI / 2 + i * Math.PI / 5;
                    var radius = i % 2 == 0 ? 1 : 0.45;
                    var point = new Point(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
                    if (i == 0) path.BeginFigure(point, true, true);
                    else path.LineTo(point, true, true);
                }
            }
            var heart = new StreamGeometry();
            using (var path = heart.Open())
            {
                path.BeginFigure(new Point(0, 0.95), true, true);
                path.BezierTo(new Point(-0.3, 0.65), new Point(-1, 0.1), new Point(-0.95, -0.4), true, true);
                path.BezierTo(new Point(-0.9, -1), new Point(-0.25, -1), new Point(0, -0.45), true, true);
                path.BezierTo(new Point(0.25, -1), new Point(0.9, -1), new Point(0.95, -0.4), true, true);
                path.BezierTo(new Point(1, 0.1), new Point(0.3, 0.65), new Point(0, 0.95), true, true);
            }
            var pad = new StreamGeometry();
            using (var path = pad.Open())
            {
                path.BeginFigure(new Point(0, -0.1), true, true);
                path.BezierTo(new Point(-0.25, -0.15), new Point(-0.35, 0.1), new Point(-0.6, 0.4), true, true);
                path.BezierTo(new Point(-0.9, 0.9), new Point(-0.3, 1), new Point(0, 0.72), true, true);
                path.BezierTo(new Point(0.3, 1), new Point(0.9, 0.9), new Point(0.6, 0.4), true, true);
                path.BezierTo(new Point(0.35, 0.1), new Point(0.25, -0.15), new Point(0, -0.1), true, true);
            }
            var paw = new GeometryGroup { FillRule = FillRule.Nonzero };
            paw.Children.Add(pad);
            paw.Children.Add(new EllipseGeometry(new Point(-0.7, -0.22), 0.22, 0.29));
            paw.Children.Add(new EllipseGeometry(new Point(-0.28, -0.65), 0.22, 0.3));
            paw.Children.Add(new EllipseGeometry(new Point(0.28, -0.65), 0.22, 0.3));
            paw.Children.Add(new EllipseGeometry(new Point(0.7, -0.22), 0.22, 0.29));
            var geometries = new Geometry[] { star, heart, paw };
            foreach (var geometry in geometries) geometry.Freeze();
            return geometries;
        }

        private static Brush[] CreateBrushes()
        {
            var brushes = new Brush[]
            {
                new LinearGradientBrush(Colors.LightGoldenrodYellow, Colors.Goldenrod, 60),
                new LinearGradientBrush(Colors.LightPink, Colors.DeepPink, 60),
                new LinearGradientBrush(Colors.Plum, Colors.MediumPurple, 60)
            };
            foreach (var brush in brushes) brush.Freeze();
            return brushes;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(32, 29, 50)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 9; i++)
                        DrawIcon(context, new IconParticle
                        {
                            Origin = new Point(30 + i * 25, 60 + i % 3 * 25),
                            Velocity = new Vector(0, -55), Age = 0.2 + (8 - i) * 0.08,
                            Lifetime = 2.2, Size = 9, Angle = i % 2 == 0 ? -15 : 15, Kind = i % 3
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct IconParticle
        {
            public Point Origin;
            public Vector Velocity;
            public double Size;
            public double Age;
            public double Lifetime;
            public double Angle;
            public double Spin;
            public int Kind;
        }
    }
}
