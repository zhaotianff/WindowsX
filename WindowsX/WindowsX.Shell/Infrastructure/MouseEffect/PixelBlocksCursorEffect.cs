using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class PixelBlocksCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxParticles = 160;
        public const int MaxEmissionPerUpdate = 16;
        private static readonly Brush[] BlockBrushes = CreateBrushes();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Block> particles = new List<Block>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextColor;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public PixelBlocksCursorEffect(MouseEffectSetting settings = null)
        {
            this.settings = settings ?? new MouseEffectSetting();
            IsHitTestVisible = false;
            RenderOptions.SetEdgeMode(visual, EdgeMode.Aliased);
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
                var block = particles[i];
                block.Age += elapsedSeconds;
                if (block.Age >= block.Lifetime) particles.RemoveAt(i);
                else particles[i] = block;
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
                if (double.IsFinite(distance) && distance >= 6 && elapsedSinceEmission >= 0.025)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 8));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.PixelBlocksOpacity);
            foreach (var block in particles) DrawBlock(context, block, settings.PixelBlocksSizeScale);
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
            particles.Add(new Block
            {
                Origin = point,
                Size = 4 + random.Next(4) * 2,
                Lifetime = 0.7 + random.NextDouble() * 0.5,
                Velocity = new Vector(random.NextDouble() * 36 - 18, -12 - random.NextDouble() * 18),
                ColorIndex = nextColor
            });
            nextColor = (nextColor + 1) % BlockBrushes.Length;
        }

        private static void DrawBlock(DrawingContext context, Block block, double sizeScale = 1)
        {
            var progress = block.Age / block.Lifetime;
            var center = block.Origin + block.Velocity * block.Age + new Vector(0, 40 * block.Age * block.Age);
            var size = Math.Max(2, block.Size - 2 * Math.Floor(progress * 3));
            var x = Math.Round((center.X - size / 2) / 2) * 2;
            var y = Math.Round((center.Y - size / 2) / 2) * 2;
            var scaledSize = size * sizeScale;
            var offset = (size - scaledSize) / 2;
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.3));
            context.DrawRectangle(BlockBrushes[block.ColorIndex], null, new Rect(x + offset, y + offset, scaledSize, scaledSize));
            context.Pop();
        }

        private static Brush[] CreateBrushes()
        {
            var colors = new[]
            {
                Color.FromRgb(89, 222, 255), Color.FromRgb(161, 130, 255),
                Color.FromRgb(255, 119, 181), Color.FromRgb(255, 214, 102), Color.FromRgb(125, 235, 159)
            };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var brush = new SolidColorBrush(colors[i]);
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
                RenderOptions.SetEdgeMode(drawing, EdgeMode.Aliased);
                using (var context = drawing.Open())
                {
                    context.DrawRectangle(new SolidColorBrush(Color.FromRgb(30, 30, 46)), null, new Rect(0, 0, 260, 170));
                    for (var i = 0; i < 20; i++)
                        DrawBlock(context, new Block
                        {
                            Origin = new Point(32 + i * 10, 78 + Math.Sin(i * 0.4) * 22),
                            Size = 6 + i % 3 * 2, Lifetime = 1.2, Age = (19 - i) * 0.04,
                            Velocity = new Vector((i % 3 - 1) * 12, -16), ColorIndex = i % BlockBrushes.Length
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Block
        {
            public Point Origin;
            public Vector Velocity;
            public double Size;
            public double Lifetime;
            public double Age;
            public int ColorIndex;
        }
    }
}
