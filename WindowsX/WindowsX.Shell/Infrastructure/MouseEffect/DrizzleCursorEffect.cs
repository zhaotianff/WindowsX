using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class DrizzleCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxParticles = 72;
        public const int MaxEmissionPerUpdate = 4;
        public const double EmissionInterval = 0.06;
        public const int MaxSplashes = 24;
        public const double WaterSurfaceY = 65;
        public const double SplashLifetime = 0.8;
        private static readonly Pen[] RainPens = CreatePens();
        private static readonly Geometry RainArea = CreateArea();
        private static readonly Brush WaterBrush = CreateWaterBrush();
        private static readonly Pen RipplePen = CreateRipplePen();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Drop> particles = new List<Drop>(MaxParticles);
        private readonly List<Splash> splashes = new List<Splash>(MaxSplashes);
        private readonly Random random = new Random();
        private bool initialized;
        private double elapsedSinceEmission;
        private double waterAmount;

        public int ParticleCount => particles.Count;
        public int SplashCount => splashes.Count;
        public double WaterAmount => waterAmount;
        protected override int VisualChildrenCount => 1;

        public DrizzleCursorEffect(MouseEffectSetting settings = null)
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
            waterAmount = Math.Max(0, waterAmount - elapsedSeconds * 0.08);
            for (var i = splashes.Count - 1; i >= 0; i--)
            {
                var splash = splashes[i];
                splash.Age += elapsedSeconds;
                if (splash.Age >= SplashLifetime) splashes.RemoveAt(i);
                else splashes[i] = splash;
            }
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var drop = particles[i];
                var impactTime = (WaterSurfaceY - drop.Origin.Y) / drop.Velocity.Y;
                var previousAge = drop.Age;
                drop.Age += elapsedSeconds;
                if (drop.Age >= impactTime)
                {
                    if (previousAge < impactTime && elapsedSeconds <= 0.25)
                    {
                        var impact = Position(drop, impactTime);
                        if (Math.Abs(impact.X) <= 72)
                        {
                            if (splashes.Count == MaxSplashes) splashes.RemoveAt(0);
                            splashes.Add(new Splash { Center = new Point(impact.X, WaterSurfaceY), Age = drop.Age - impactTime });
                            waterAmount = Math.Min(1, waterAmount + 0.06);
                        }
                    }
                    particles.RemoveAt(i);
                }
                else if (drop.Age >= drop.Lifetime) particles.RemoveAt(i);
                else particles[i] = drop;
            }
            if (!initialized)
            {
                initialized = true;
                for (var i = 0; i < MaxEmissionPerUpdate; i++) Emit(0.1);
            }
            else if (elapsedSeconds > 0.25)
            {
                elapsedSinceEmission = 0;
            }
            else
            {
                elapsedSinceEmission += elapsedSeconds;
                var count = Math.Min(MaxEmissionPerUpdate, (int)(elapsedSinceEmission / EmissionInterval) * 2);
                if (count > 0)
                {
                    elapsedSinceEmission %= EmissionInterval;
                    for (var i = 0; i < count; i++) Emit(0);
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.DrizzleOpacity);
            context.PushTransform(new TranslateTransform(cursor.X, cursor.Y));
            DrawWater(context, waterAmount);
            context.PushClip(RainArea);
            foreach (var drop in particles) DrawDrop(context, drop);
            context.Pop();
            foreach (var splash in splashes) DrawSplash(context, splash);
            context.Pop();
            context.Pop();
        }

        public void Clear()
        {
            particles.Clear();
            splashes.Clear();
            waterAmount = 0;
            initialized = false;
            elapsedSinceEmission = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(double age)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            var drop = new Drop
            {
                Origin = new Point(random.NextDouble() * 140 - 85, random.NextDouble() * 80 - 65),
                Velocity = new Vector(24 + random.NextDouble() * 14, 55 + random.NextDouble() * 25),
                Length = 7 + random.NextDouble() * 7,
                Lifetime = 1.2 + random.NextDouble() * 0.6,
                Age = age,
                Phase = random.NextDouble() * Math.PI * 2,
                PenIndex = random.Next(RainPens.Length)
            };
            drop.Lifetime = Math.Max(drop.Lifetime, (WaterSurfaceY - drop.Origin.Y) / drop.Velocity.Y + 0.4);
            particles.Add(drop);
        }

        private static Point Position(Drop drop, double age) => drop.Origin + drop.Velocity * age
            + new Vector(4 * (Math.Sin(drop.Phase + age * 2) - Math.Sin(drop.Phase)), 0);

        private static void DrawDrop(DrawingContext context, Drop drop)
        {
            var angle = drop.Phase + drop.Age * 2;
            var center = Position(drop, drop.Age);
            var direction = drop.Velocity + new Vector(8 * Math.Cos(angle), 0);
            direction.Normalize();
            var distance = Math.Sqrt(center.X * center.X / (120 * 120) + center.Y * center.Y / (95 * 95));
            var edge = Math.Clamp((1 - distance) / 0.3, 0, 1);
            var fade = Math.Min(1, drop.Age / 0.15) * Math.Min(1, (drop.Lifetime - drop.Age) / 0.4);
            context.PushOpacity(Math.Max(0, fade) * edge);
            context.DrawLine(RainPens[drop.PenIndex], center - direction * drop.Length, center);
            context.Pop();
        }

        private static void DrawWater(DrawingContext context, double amount)
        {
            if (amount <= 0) return;
            context.PushOpacity(amount);
            context.DrawEllipse(WaterBrush, null, new Point(0, WaterSurfaceY), 96, 13);
            context.Pop();
        }

        private static void DrawSplash(DrawingContext context, Splash splash)
        {
            var progress = splash.Age / SplashLifetime;
            var radius = 2 + progress * 19;
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.5));
            context.DrawEllipse(null, RipplePen, splash.Center, radius, radius * 0.24);
            if (splash.Age < 0.3)
            {
                var lift = 80 * splash.Age * (0.3 - splash.Age);
                context.PushOpacity(1 - splash.Age / 0.3);
                for (var i = -1; i <= 1; i++)
                {
                    var center = splash.Center + new Vector(i * (2 + splash.Age * 22), -lift * (i == 0 ? 1.5 : 1));
                    context.DrawEllipse(RipplePen.Brush, null, center, 0.7, 1.1);
                }
                context.Pop();
            }
            context.Pop();
        }

        private static Brush CreateWaterBrush()
        {
            var brush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(85, 151, 199, 222), 0),
                    new GradientStop(Color.FromArgb(48, 129, 182, 211), 0.65),
                    new GradientStop(Color.FromArgb(0, 129, 182, 211), 1)
                }
            };
            brush.Freeze();
            return brush;
        }

        private static Pen CreateRipplePen()
        {
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(150, 200, 229, 243)), 0.7);
            pen.Freeze();
            return pen;
        }

        private static Geometry CreateArea()
        {
            var area = new EllipseGeometry(new Point(), 120, 95);
            area.Freeze();
            return area;
        }

        private static Pen[] CreatePens()
        {
            var pens = new Pen[3];
            for (var i = 0; i < pens.Length; i++)
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0), EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(0, 180, 210, 225), 0),
                        new GradientStop(Color.FromArgb((byte)(95 + i * 30), 180, 210, 225), 0.65),
                        new GradientStop(Color.FromArgb((byte)(135 + i * 25), 220, 237, 245), 1)
                    }
                };
                pens[i] = new Pen(brush, 0.6 + i * 0.2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                pens[i].Freeze();
            }
            return pens;
        }

        public static DrawingImage PreviewImage
        {
            get
            {
                var drawing = new DrawingGroup();
                using (var context = drawing.Open())
                {
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(29, 39, 47)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new TranslateTransform(130, 85));
                    context.PushTransform(new ScaleTransform(0.8, 0.8));
                    DrawWater(context, 0.9);
                    context.PushClip(RainArea);
                    for (var i = 0; i < 36; i++)
                        DrawDrop(context, new Drop
                        {
                            Origin = new Point(-100 + i * 37 % 165, -90 + i * 29 % 120),
                            Velocity = new Vector(30, 65), Length = 10 + i % 5,
                            Lifetime = 1.8, Age = 0.3 + i % 7 * 0.1, Phase = i * 0.7, PenIndex = i % 3
                        });
                    context.Pop();
                    for (var i = 0; i < 5; i++)
                        DrawSplash(context, new Splash { Center = new Point(-54 + i * 26, WaterSurfaceY), Age = 0.08 + i * 0.12 });
                    context.Pop();
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Splash
        {
            public Point Center;
            public double Age;
        }

        private struct Drop
        {
            public Point Origin;
            public Vector Velocity;
            public double Length;
            public double Lifetime;
            public double Age;
            public double Phase;
            public int PenIndex;
        }
    }
}
