using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class ParticleCursorEffect : UIElement,ICursorEffect
    {
        public const int MaxParticles = 240;

        private static readonly Brush[] SparkBrushes =
        {
            CreateBrush(Colors.Gold), CreateBrush(Colors.DeepPink),
            CreateBrush(Colors.DeepSkyBlue), CreateBrush(Colors.SpringGreen),
            CreateBrush(Colors.Orange), CreateBrush(Colors.MediumOrchid)
        };
        private static readonly Pen[] SparkPens = CreateSparkPens();
        private Brush catkinBrush;
        private Brush catkinHalo;
        private Pen catkinPen;
        private Pen catkinStem;
        private string catkinColor;
        private Brush starBrush;
        private string starColor;
        private static readonly Pen SnowflakePen = CreatePen(CreateBrush(Color.FromRgb(225, 245, 255)), 1);
        private static readonly Geometry StarGeometry = CreateStarGeometry();

        private readonly MouseEffectType effectType;
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Particle> particles = new List<Particle>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double emissionDelay;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public ParticleCursorEffect(MouseEffectType effectType, MouseEffectSetting settings = null)
        {
            if (effectType != MouseEffectType.Fireworks && effectType != MouseEffectType.WillowCatkins &&
                effectType != MouseEffectType.Snowflakes && effectType != MouseEffectType.FivePointedStars)
                throw new ArgumentOutOfRangeException(nameof(effectType));

            this.effectType = effectType;
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

            var motionStep = Math.Min(elapsedSeconds, 0.05);
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.Age += elapsedSeconds;
                if (particle.Age >= particle.Lifetime)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                if (effectType == MouseEffectType.Fireworks)
                {
                    particle.Velocity *= Math.Exp(-0.65 * motionStep);
                    particle.Velocity.Y += 100 * motionStep;
                    particle.Position += particle.Velocity * motionStep;
                }
                else if (effectType == MouseEffectType.FivePointedStars)
                {
                    particle.Velocity *= Math.Exp(-0.6 * motionStep);
                    particle.Velocity.Y += 35 * motionStep;
                    particle.Position += particle.Velocity * motionStep;
                }
                else
                {
                    var snowflake = effectType == MouseEffectType.Snowflakes;
                    var sway = Math.Sin(particle.Age * (snowflake ? 1.5 : 2.2) + particle.Phase) * (snowflake ? 14 : 22);
                    particle.Position.X += (particle.Velocity.X + sway) * motionStep;
                    particle.Position.Y += particle.Velocity.Y * motionStep;
                }

                particles[i] = particle;
            }

            emissionDelay = Math.Max(0, emissionDelay - elapsedSeconds);
            if (emissionDelay == 0 && (!lastEmissionPoint.HasValue || (cursor - lastEmissionPoint.Value).LengthSquared >= 9))
            {
                Emit(cursor);
                lastEmissionPoint = cursor;
                emissionDelay = effectType == MouseEffectType.Fireworks ? 0.14 : effectType == MouseEffectType.Snowflakes ? 0.08 : 0.06;
            }

            Draw();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            emissionDelay = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point cursor)
        {
            if (effectType == MouseEffectType.Snowflakes || effectType == MouseEffectType.FivePointedStars)
            {
                EmitShapes(cursor);
                return;
            }

            var fireworks = effectType == MouseEffectType.Fireworks;
            var count = fireworks ? 28 : 3;
            for (var i = 0; i < count && particles.Count < MaxParticles; i++)
            {
                var angle = (i + random.NextDouble() * 0.5) * Math.PI * 2 / count;
                var speed = (65 + random.NextDouble() * 110) * settings.FireworksExpansion;
                particles.Add(new Particle
                {
                    Position = fireworks ? cursor : new Point(cursor.X + random.NextDouble() * 16 - 8, cursor.Y + random.NextDouble() * 12 - 6),
                    Velocity = fireworks
                        ? new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed)
                        : new Vector(8 + random.NextDouble() * 14, 12 + random.NextDouble() * 18),
                    Lifetime = fireworks ? 0.75 + random.NextDouble() * 0.6 : 2.5 + random.NextDouble() * 1.5,
                    Size = fireworks ? 1.2 + random.NextDouble() : 2 + random.NextDouble() * 2,
                    Phase = random.NextDouble() * Math.PI * 2,
                    ColorIndex = random.Next(SparkBrushes.Length)
                });
            }
        }

        private void EmitShapes(Point cursor)
        {
            var snowflake = effectType == MouseEffectType.Snowflakes;
            for (var i = 0; i < 2 && particles.Count < MaxParticles; i++)
            {
                particles.Add(new Particle
                {
                    Position = new Point(cursor.X + random.NextDouble() * 16 - 8, cursor.Y + random.NextDouble() * 12 - 6),
                    Velocity = snowflake
                        ? new Vector(random.NextDouble() * 12 - 6, 18 + random.NextDouble() * 14)
                        : new Vector(random.NextDouble() * 50 - 25, random.NextDouble() * 25 - 15),
                    Lifetime = snowflake ? 2.5 + random.NextDouble() * 1.5 : 1 + random.NextDouble() * 0.8,
                    Size = snowflake ? 2 + random.NextDouble() * 6 : 4 + random.NextDouble() * 3,
                    Phase = random.NextDouble() * Math.PI * 2,
                    ColorIndex = random.Next(SparkBrushes.Length)
                });
            }
        }

        private void Draw()
        {
            if (effectType == MouseEffectType.WillowCatkins)
                UpdateCatkinBrushes();
            if (effectType == MouseEffectType.FivePointedStars && !settings.FivePointedStarsUseRandomColors
                && starColor != settings.FivePointedStarsColor)
            {
                starColor = settings.FivePointedStarsColor;
                starBrush = CreateBrush((Color)ColorConverter.ConvertFromString(starColor));
            }
            using var context = visual.RenderOpen();
            foreach (var particle in particles)
            {
                var opacity = Math.Max(0, 1 - particle.Age / particle.Lifetime);
                opacity *= effectType switch
                {
                    MouseEffectType.Fireworks => settings.FireworksOpacity,
                    MouseEffectType.Snowflakes => settings.SnowflakesOpacity,
                    MouseEffectType.FivePointedStars => settings.FivePointedStarsOpacity,
                    _ => settings.WillowCatkinsOpacity
                };
                if (effectType == MouseEffectType.Fireworks)
                {
                    context.PushOpacity(opacity);
                    context.DrawLine(SparkPens[particle.ColorIndex], particle.Position - particle.Velocity * 0.035, particle.Position);
                    context.DrawEllipse(SparkBrushes[particle.ColorIndex], null, particle.Position, particle.Size, particle.Size);
                }
                else if (effectType == MouseEffectType.Snowflakes)
                {
                    context.PushOpacity(opacity * Math.Min(1, particle.Age / 0.15));
                    var scale = settings.SnowflakesSizeScale;
                    context.PushTransform(new ScaleTransform(scale, scale, particle.Position.X, particle.Position.Y));
                    DrawSnowflake(context, particle);
                    context.Pop();
                }
                else if (effectType == MouseEffectType.FivePointedStars)
                {
                    context.PushOpacity(opacity * Math.Min(1, particle.Age / 0.08));
                    var transform = Matrix.Identity;
                    var size = particle.Size * settings.FivePointedStarsSizeScale;
                    transform.Scale(size, size);
                    transform.Rotate(particle.Phase * 180 / Math.PI + particle.Age * 75);
                    transform.Translate(particle.Position.X, particle.Position.Y);
                    context.PushTransform(new MatrixTransform(transform));
                    context.DrawGeometry(settings.FivePointedStarsUseRandomColors ? SparkBrushes[particle.ColorIndex] : starBrush, null, StarGeometry);
                    context.Pop();
                }
                else
                {
                    context.PushOpacity(opacity * Math.Min(1, particle.Age / 0.15));
                    var scale = settings.WillowCatkinsSizeScale;
                    context.PushTransform(new ScaleTransform(scale, scale, particle.Position.X, particle.Position.Y));
                    context.DrawEllipse(catkinHalo, null, particle.Position, particle.Size * 2, particle.Size * 1.6);
                    context.DrawEllipse(catkinBrush, catkinStem, particle.Position, particle.Size * 0.55, particle.Size * 0.3);
                    for (var i = 0; i < 4; i++)
                    {
                        var angle = i * Math.PI / 4 + Math.Sin(particle.Age * 1.5 + particle.Phase) * 0.5;
                        var strand = new Vector(Math.Cos(angle), Math.Sin(angle)) * particle.Size * 1.4;
                        context.DrawLine(catkinPen, particle.Position - strand, particle.Position + strand);
                    }
                    context.Pop();
                }
                context.Pop();
            }
        }

        private void UpdateCatkinBrushes()
        {
            if (catkinColor == settings.WillowCatkinsColor)
                return;

            catkinColor = settings.WillowCatkinsColor;
            var color = (Color)ColorConverter.ConvertFromString(catkinColor);
            var original = catkinColor == MouseEffectSetting.DefaultWillowCatkinsColor;
            catkinBrush = CreateBrush(color);
            catkinHalo = CreateBrush(original ? Color.FromArgb(40, 244, 255, 220) : Color.FromArgb(40, color.R, color.G, color.B));
            catkinPen = CreatePen(catkinBrush, 0.8);
            catkinStem = CreatePen(CreateBrush(original ? Color.FromRgb(118, 137, 100)
                : Color.FromRgb((byte)(color.R * 0.5), (byte)(color.G * 0.5), (byte)(color.B * 0.5))), 0.8);
        }

        private static void DrawSnowflake(DrawingContext context, Particle particle)
        {
            for (var i = 0; i < 6; i++)
            {
                var angle = i * Math.PI / 3 + particle.Phase + particle.Age * 0.35;
                var arm = new Vector(Math.Cos(angle), Math.Sin(angle)) * particle.Size;
                var side = new Vector(-arm.Y, arm.X) * 0.2;
                var branch = particle.Position + arm * 0.65;
                context.DrawLine(SnowflakePen, particle.Position, particle.Position + arm);
                context.DrawLine(SnowflakePen, branch, particle.Position + arm * 0.4 + side);
                context.DrawLine(SnowflakePen, branch, particle.Position + arm * 0.4 - side);
            }
        }

        private static Geometry CreateStarGeometry()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                for (var i = 0; i < 10; i++)
                {
                    var angle = -Math.PI / 2 + i * Math.PI / 5;
                    var radius = i % 2 == 0 ? 1 : 0.45;
                    var point = new Point(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
                    if (i == 0)
                        context.BeginFigure(point, true, true);
                    else
                        context.LineTo(point, true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static Brush CreateBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static Pen CreatePen(Brush brush, double thickness)
        {
            var pen = new Pen(brush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            pen.Freeze();
            return pen;
        }

        private static Pen[] CreateSparkPens()
        {
            var pens = new Pen[SparkBrushes.Length];
            for (var i = 0; i < pens.Length; i++)
                pens[i] = CreatePen(SparkBrushes[i], 1.5);
            return pens;
        }

        private struct Particle
        {
            public Point Position;
            public Vector Velocity;
            public double Age;
            public double Lifetime;
            public double Size;
            public double Phase;
            public int ColorIndex;
        }
    }
}
