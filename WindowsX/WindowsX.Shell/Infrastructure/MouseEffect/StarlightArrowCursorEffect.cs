using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class StarlightArrowCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxParticles = 32;
        public const double EmissionInterval = 0.08;
        private static readonly Geometry StarShape = CreateStar();
        private static readonly Brush StarBrush = CreateStarBrush();
        private static readonly Brush GlowBrush = CreateGlowBrush();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Star> particles = new List<Star>(MaxParticles);
        private readonly Random random = new Random();
        private bool initialized;
        private double elapsedSinceEmission;
        private string cachedColor = MouseEffectSetting.DefaultStarlightArrowColor;
        private Brush currentStarBrush = StarBrush;
        private Brush currentGlowBrush = GlowBrush;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public StarlightArrowCursorEffect(MouseEffectSetting settings = null)
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
                var star = particles[i];
                star.Age += elapsedSeconds;
                if (star.Age >= star.Lifetime) particles.RemoveAt(i);
                else particles[i] = star;
            }
            if (!initialized)
            {
                initialized = true;
                Emit(cursor);
            }
            else if (elapsedSeconds > 0.25)
            {
                elapsedSinceEmission = 0;
            }
            else
            {
                elapsedSinceEmission += elapsedSeconds;
                if (elapsedSinceEmission >= EmissionInterval)
                {
                    Emit(cursor);
                    elapsedSinceEmission %= EmissionInterval;
                }
            }
            UpdateColor();
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.StarlightArrowOpacity);
            foreach (var star in particles) DrawStar(context, star, currentStarBrush, currentGlowBrush);
            context.Pop();
        }

        private void UpdateColor()
        {
            if (cachedColor == settings.StarlightArrowColor) return;
            cachedColor = settings.StarlightArrowColor;
            if (cachedColor == MouseEffectSetting.DefaultStarlightArrowColor)
            {
                currentStarBrush = StarBrush;
                currentGlowBrush = GlowBrush;
                return;
            }
            var color = (Color)ColorConverter.ConvertFromString(cachedColor);
            currentStarBrush = CreateStarBrush(color);
            currentGlowBrush = CreateGlowBrush(color);
        }

        public void Clear()
        {
            particles.Clear();
            initialized = false;
            elapsedSinceEmission = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new Star
            {
                Origin = point,
                Size = 2.5 + random.NextDouble() * 2,
                Lifetime = 0.9 + random.NextDouble() * 0.5,
                Velocity = new Vector(random.NextDouble() * 24 - 12, 12 + random.NextDouble() * 10),
                Phase = random.NextDouble() * Math.PI * 2,
                RotationSpeed = random.NextDouble() * 60 - 30
            });
        }

        private static void DrawStar(DrawingContext context, Star star, Brush starBrush = null, Brush glowBrush = null)
        {
            var progress = star.Age / star.Lifetime;
            var center = star.Origin + star.Velocity * star.Age + new Vector(
                3 * (Math.Sin(star.Phase + star.Age * 3) - Math.Sin(star.Phase)), 32 * star.Age * star.Age);
            var size = star.Size * (1 - progress * 0.5);
            var transform = Matrix.Identity;
            transform.Scale(size, size);
            transform.Rotate(star.RotationSpeed * star.Age);
            transform.Translate(center.X, center.Y);
            var twinkle = 0.7 + 0.3 * Math.Sin(star.Phase + star.Age * 10);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.4) * twinkle);
            context.PushTransform(new MatrixTransform(transform));
            context.DrawEllipse(glowBrush ?? GlowBrush, null, new Point(), 2.6, 2.6);
            context.DrawGeometry(starBrush ?? StarBrush, null, StarShape);
            context.DrawEllipse(Brushes.White, null, new Point(), 0.18, 0.18);
            context.Pop();
            context.Pop();
        }

        private static Geometry CreateStar()
        {
            var shape = new StreamGeometry();
            using (var context = shape.Open())
            {
                context.BeginFigure(new Point(0, -1), true, true);
                context.LineTo(new Point(0.22, -0.22), true, false);
                context.LineTo(new Point(1, 0), true, false);
                context.LineTo(new Point(0.22, 0.22), true, false);
                context.LineTo(new Point(0, 1), true, false);
                context.LineTo(new Point(-0.22, 0.22), true, false);
                context.LineTo(new Point(-1, 0), true, false);
                context.LineTo(new Point(-0.22, -0.22), true, false);
            }
            shape.Freeze();
            return shape;
        }

        private static Brush CreateStarBrush(Color? color = null)
        {
            var brush = new LinearGradientBrush(Colors.White, color ?? Color.FromRgb(255, 213, 112), 90);
            brush.Freeze();
            return brush;
        }

        private static Brush CreateGlowBrush(Color? color = null)
        {
            var tint = color ?? Color.FromRgb(255, 213, 112);
            var highlight = color.HasValue
                ? Color.FromRgb((byte)((tint.R + 255) / 2), (byte)((tint.G + 255) / 2), (byte)((tint.B + 255) / 2))
                : Color.FromRgb(255, 234, 161);
            var brush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(170, highlight.R, highlight.G, highlight.B), 0),
                    new GradientStop(Color.FromArgb(55, tint.R, tint.G, tint.B), 0.4),
                    new GradientStop(Color.FromArgb(0, tint.R, tint.G, tint.B), 1)
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(28, 29, 45)), null, new Rect(0, 0, 260, 170), 10, 10);
                    var tip = new Point(130, 43);
                    for (var i = 8; i >= 0; i--)
                        DrawStar(context, new Star
                        {
                            Origin = tip, Size = 4.5, Age = i * 0.12, Lifetime = 1.4,
                            Velocity = new Vector((i % 3 - 1) * 18, 32), Phase = i * 0.8, RotationSpeed = 25
                        });
                    var arrow = new StreamGeometry();
                    using (var path = arrow.Open())
                    {
                        path.BeginFigure(tip, true, true);
                        path.LineTo(tip + new Vector(2, 29), true, false);
                        path.LineTo(tip + new Vector(9, 22), true, false);
                        path.LineTo(tip + new Vector(16, 35), true, false);
                        path.LineTo(tip + new Vector(22, 32), true, false);
                        path.LineTo(tip + new Vector(15, 19), true, false);
                        path.LineTo(tip + new Vector(25, 18), true, false);
                    }
                    context.DrawGeometry(Brushes.White, new Pen(Brushes.SlateGray, 1), arrow);
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Star
        {
            public Point Origin;
            public Vector Velocity;
            public double Size;
            public double Lifetime;
            public double Age;
            public double Phase;
            public double RotationSpeed;
        }
    }
}
