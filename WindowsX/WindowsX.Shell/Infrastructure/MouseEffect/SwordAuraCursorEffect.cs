using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class SwordAuraCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxParticles = 64;
        public const int MaxEmissionPerUpdate = 8;
        private static readonly Brush[] DiffusionBrushes = CreateDiffusionBrushes();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Blade> particles = new List<Blade>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextColor;
        private string cachedColor1 = MouseEffectSetting.DefaultSwordAuraColor1;
        private string cachedColor2 = MouseEffectSetting.DefaultSwordAuraColor2;
        private string cachedColor3 = MouseEffectSetting.DefaultSwordAuraColor3;
        private Brush[] currentBrushes = DiffusionBrushes;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public SwordAuraCursorEffect(MouseEffectSetting settings = null)
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
            if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y)) return;
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var blade = particles[i];
                blade.Age += elapsedSeconds;
                if (blade.Age >= blade.Lifetime) particles.RemoveAt(i);
                else particles[i] = blade;
            }
            elapsedSinceEmission = Math.Min(1, elapsedSinceEmission + elapsedSeconds);
            if (!lastEmissionPoint.HasValue)
            {
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }
            else
            {
                var movement = cursor - lastEmissionPoint.Value;
                var distance = movement.Length;
                if (double.IsFinite(distance) && distance >= 4 && elapsedSinceEmission >= 0.02)
                {
                    var direction = movement / distance;
                    var count = elapsedSinceEmission > 0.25 || distance > 600 ? 1
                        : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 14));
                    for (var i = 1; i <= count; i++)
                        Emit(lastEmissionPoint.Value + movement * ((double)i / count), direction);
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            UpdateColors();
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.SwordAuraOpacity);
            foreach (var blade in particles) DrawBlade(context, blade, settings.SwordAuraSizeScale, currentBrushes);
            context.Pop();
        }

        private void UpdateColors()
        {
            if (cachedColor1 == settings.SwordAuraColor1 && cachedColor2 == settings.SwordAuraColor2 && cachedColor3 == settings.SwordAuraColor3) return;
            cachedColor1 = settings.SwordAuraColor1;
            cachedColor2 = settings.SwordAuraColor2;
            cachedColor3 = settings.SwordAuraColor3;
            currentBrushes = cachedColor1 == MouseEffectSetting.DefaultSwordAuraColor1
                && cachedColor2 == MouseEffectSetting.DefaultSwordAuraColor2 && cachedColor3 == MouseEffectSetting.DefaultSwordAuraColor3
                ? DiffusionBrushes : CreateDiffusionBrushes(new[]
                {
                    (Color)ColorConverter.ConvertFromString(cachedColor1),
                    (Color)ColorConverter.ConvertFromString(cachedColor2),
                    (Color)ColorConverter.ConvertFromString(cachedColor3)
                });
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            nextColor = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point, Vector direction)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new Blade
            {
                Origin = point, Direction = direction,
                Length = 42 + random.NextDouble() * 28, Width = 5 + random.NextDouble() * 4,
                Lifetime = 0.45 + random.NextDouble() * 0.3,
                Phase = random.NextDouble() * Math.PI * 2, ColorIndex = nextColor
            });
            nextColor = (nextColor + 1) % DiffusionBrushes.Length;
        }

        private static void DrawBlade(DrawingContext context, Blade blade, double sizeScale = 1, Brush[] brushes = null)
        {
            var progress = blade.Age / blade.Lifetime;
            var normal = new Vector(-blade.Direction.Y, blade.Direction.X);
            var center = blade.Origin + blade.Direction * (12 * blade.Age)
                + normal * (Math.Sin(blade.Phase + blade.Age * 4) - Math.Sin(blade.Phase)) * 3;
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.5));
            DrawDiffusion(context, blade, center, progress, sizeScale, brushes ?? DiffusionBrushes);
            context.Pop();
        }

        private static void DrawDiffusion(DrawingContext context, Blade blade, Point center, double progress, double sizeScale, Brush[] brushes)
        {
            var radius = blade.Width + 22 * progress;
            var angle = blade.Phase + blade.Age * 3;
            var curl = new Vector(Math.Cos(angle), Math.Sin(angle)) * (1 - Math.Exp(-blade.Age * 4)) * radius * 0.4;
            var transform = Matrix.Identity;
            transform.Scale(sizeScale, sizeScale);
            transform.Rotate(Math.Atan2(blade.Direction.Y, blade.Direction.X) * 180 / Math.PI);
            transform.Translate(center.X, center.Y);
            context.PushTransform(new MatrixTransform(transform));
            context.DrawEllipse(brushes[blade.ColorIndex], null,
                new Point(-blade.Length * 0.45, 0) + curl, blade.Length * 0.3 + radius * 0.4, radius * 0.85);
            context.DrawEllipse(brushes[(blade.ColorIndex + 1) % brushes.Length], null,
                new Point(-blade.Length * 0.7, 0) - curl, radius * 0.9, radius * 0.65);
            context.Pop();
        }

        private static Brush[] CreateDiffusionBrushes(Color[] colors = null)
        {
            colors ??= new[] { Color.FromRgb(24, 65, 110), Color.FromRgb(41, 49, 104), Color.FromRgb(24, 112, 133) };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var brush = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(180, color.R, color.G, color.B), 0),
                        new GradientStop(Color.FromArgb(105, color.R, color.G, color.B), 0.3),
                        new GradientStop(Color.FromArgb(30, color.R, color.G, color.B), 0.65),
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(30, 34, 46)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 9; i++)
                    {
                        var angle = (-25 + i * 4) * Math.PI / 180;
                        DrawBlade(context, new Blade
                        {
                            Origin = new Point(70 + i * 19, 105 - Math.Sin(i * 0.35) * 35),
                            Direction = new Vector(Math.Cos(angle), Math.Sin(angle)),
                            Length = 64, Width = 9, Lifetime = 0.75, Age = (8 - i) * 0.045,
                            Phase = i * 0.7, ColorIndex = i % DiffusionBrushes.Length
                        });
                    }
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Blade
        {
            public Point Origin;
            public Vector Direction;
            public double Length;
            public double Width;
            public double Lifetime;
            public double Age;
            public double Phase;
            public int ColorIndex;
        }
    }
}
