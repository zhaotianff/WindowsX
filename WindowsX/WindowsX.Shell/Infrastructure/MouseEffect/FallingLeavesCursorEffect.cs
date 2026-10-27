using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FallingLeavesCursorEffect : UIElement
    {
        public const int MaxParticles = 120;
        public const int MaxEmissionPerUpdate = 6;
        private static readonly Geometry LeafGeometry = CreateLeafGeometry();
        private static readonly Brush[] LeafBrushes = CreateLeafBrushes();
        private static readonly Pen VeinPen = CreateVeinPen();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Leaf> particles = new List<Leaf>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public FallingLeavesCursorEffect(MouseEffectSetting settings = null)
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
                var leaf = particles[i];
                leaf.Age += elapsedSeconds;
                if (leaf.Age >= leaf.Lifetime) particles.RemoveAt(i);
                else particles[i] = leaf;
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
                if (double.IsFinite(distance) && distance >= 14 && elapsedSinceEmission >= 0.06)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 28));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.FallingLeavesOpacity);
            foreach (var leaf in particles) DrawLeaf(context, leaf);
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
            particles.Add(new Leaf
            {
                Origin = point,
                Size = 6 + random.NextDouble() * 5,
                Lifetime = 2.8 + random.NextDouble() * 1.2,
                FallSpeed = 24 + random.NextDouble() * 14,
                Drift = random.NextDouble() * 16 - 8,
                Phase = random.NextDouble() * Math.PI * 2,
                Spin = (random.Next(2) == 0 ? -1 : 1) * (30 + random.NextDouble() * 35),
                ColorIndex = random.Next(LeafBrushes.Length)
            });
        }

        private static void DrawLeaf(DrawingContext context, Leaf leaf)
        {
            var age = leaf.Age;
            var center = leaf.Origin + new Vector(leaf.Drift * age + 16 * (Math.Sin(leaf.Phase + age * 1.8) - Math.Sin(leaf.Phase)),
                leaf.FallSpeed * age + 4 * age * age);
            var transform = Matrix.Identity;
            transform.Scale(leaf.Size * (0.7 + 0.3 * Math.Cos(leaf.Phase + age * 2.2)), leaf.Size);
            transform.Rotate(leaf.Phase * 180 / Math.PI + leaf.Spin * age);
            transform.Translate(center.X, center.Y);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - age / leaf.Lifetime), 1.2));
            context.PushTransform(new MatrixTransform(transform));
            context.DrawGeometry(LeafBrushes[leaf.ColorIndex], null, LeafGeometry);
            context.DrawLine(VeinPen, new Point(0, -0.85), new Point(0, 1.3));
            for (var i = 0; i < 3; i++)
            {
                var y = -0.35 + i * 0.4;
                context.DrawLine(VeinPen, new Point(0, y + 0.25), new Point(-0.38, y - 0.12));
                context.DrawLine(VeinPen, new Point(0, y + 0.25), new Point(0.38, y - 0.12));
            }
            context.Pop();
            context.Pop();
        }

        private static Geometry CreateLeafGeometry()
        {
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
            {
                path.BeginFigure(new Point(0, -1), true, true);
                path.BezierTo(new Point(0.95, -0.45), new Point(0.85, 0.5), new Point(0, 1), true, true);
                path.BezierTo(new Point(-0.85, 0.5), new Point(-0.95, -0.45), new Point(0, -1), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        private static Brush[] CreateLeafBrushes()
        {
            var colors = new[] { Color.FromRgb(234, 179, 48), Color.FromRgb(223, 108, 39), Color.FromRgb(177, 65, 35), Color.FromRgb(157, 113, 48) };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                brushes[i] = new LinearGradientBrush(color, Color.FromRgb((byte)(color.R * 0.7), (byte)(color.G * 0.7), (byte)(color.B * 0.7)), 45);
                brushes[i].Freeze();
            }
            return brushes;
        }

        private static Pen CreateVeinPen()
        {
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(180, 96, 57, 22)), 0.07)
            {
                StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round
            };
            pen.Freeze();
            return pen;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(35, 40, 32)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 8; i++)
                        DrawLeaf(context, new Leaf
                        {
                            Origin = new Point(35 + i * 27, 35 + i % 3 * 27),
                            Size = 7 + i % 3 * 2, Lifetime = 3.5, Age = (7 - i) * 0.1,
                            FallSpeed = 30, Phase = i * 1.2, Spin = 40, ColorIndex = i % LeafBrushes.Length
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Leaf
        {
            public Point Origin;
            public double Size;
            public double Lifetime;
            public double Age;
            public double FallSpeed;
            public double Drift;
            public double Phase;
            public double Spin;
            public int ColorIndex;
        }
    }
}
