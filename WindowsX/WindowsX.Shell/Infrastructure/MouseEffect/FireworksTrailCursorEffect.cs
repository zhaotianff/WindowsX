using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FireworksTrailCursorEffect : UIElement
    {
        public const int MaxParticles = 240;
        public const int ParticlesPerBurst = 20;
        private const double Gravity = 240;
        private const double TrailDuration = 0.18;
        private const int TrailSegments = 6;
        private static readonly Brush[] SparkBrushes = CreateBrushes();
        private static readonly Pen[] TrailPens = CreatePens(1.5);
        private static readonly Pen[] GlowPens = CreatePens(4);
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Spark> particles = new List<Spark>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public FireworksTrailCursorEffect(MouseEffectSetting settings = null)
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
                var spark = particles[i];
                spark.Age += elapsedSeconds;
                if (spark.Age >= spark.Lifetime) particles.RemoveAt(i);
                else particles[i] = spark;
            }
            elapsedSinceEmission = Math.Min(1, elapsedSinceEmission + elapsedSeconds);
            if (!lastEmissionPoint.HasValue ||
                ((cursor - lastEmissionPoint.Value).Length >= 12 && elapsedSinceEmission >= 0.09))
            {
                Emit(cursor);
                lastEmissionPoint = cursor;
                elapsedSinceEmission = 0;
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.FireworksTrailOpacity);
            foreach (var spark in particles) DrawSpark(context, spark);
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
            var phase = random.NextDouble() * Math.PI * 2;
            for (var i = 0; i < ParticlesPerBurst; i++)
            {
                if (particles.Count == MaxParticles) particles.RemoveAt(0);
                var angle = phase + Math.PI * 2 * (i + random.NextDouble() * 0.4) / ParticlesPerBurst;
                var speed = 65 + random.NextDouble() * 65;
                particles.Add(new Spark
                {
                    Origin = point,
                    Velocity = new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed),
                    Lifetime = 1.2 + random.NextDouble() * 0.5,
                    Radius = 1.2 + random.NextDouble() * 0.8,
                    ColorIndex = i % SparkBrushes.Length
                });
            }
        }

        private static Point PositionAt(Spark spark, double time)
        {
            return spark.Origin + spark.Velocity * time + new Vector(0, 0.5 * Gravity * time * time);
        }

        private static void DrawSpark(DrawingContext context, Spark spark)
        {
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - spark.Age / spark.Lifetime), 1.3));
            var startTime = Math.Max(0, spark.Age - TrailDuration);
            if (spark.Age > 0)
            {
                for (var i = 0; i < TrailSegments; i++)
                {
                    var from = PositionAt(spark, startTime + (spark.Age - startTime) * i / TrailSegments);
                    var to = PositionAt(spark, startTime + (spark.Age - startTime) * (i + 1) / TrailSegments);
                    context.PushOpacity((i + 1.0) / TrailSegments * 0.75);
                    context.PushOpacity(0.18);
                    context.DrawLine(GlowPens[spark.ColorIndex], from, to);
                    context.Pop();
                    context.DrawLine(TrailPens[spark.ColorIndex], from, to);
                    context.Pop();
                }
            }
            var head = PositionAt(spark, spark.Age);
            context.DrawEllipse(SparkBrushes[spark.ColorIndex], null, head, spark.Radius, spark.Radius);
            context.DrawEllipse(Brushes.White, null, head, spark.Radius * 0.4, spark.Radius * 0.4);
            context.Pop();
        }

        private static Brush[] CreateBrushes()
        {
            var colors = new[] { Colors.Gold, Colors.HotPink, Colors.DeepSkyBlue, Colors.PaleGreen, Colors.Orange, Colors.Violet };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                brushes[i] = new SolidColorBrush(colors[i]);
                brushes[i].Freeze();
            }
            return brushes;
        }

        private static Pen[] CreatePens(double width)
        {
            var pens = new Pen[SparkBrushes.Length];
            for (var i = 0; i < pens.Length; i++)
            {
                pens[i] = new Pen(SparkBrushes[i], width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(20, 23, 40)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var burst = 0; burst < 2; burst++)
                        for (var i = 0; i < ParticlesPerBurst; i++)
                        {
                            var angle = Math.PI * 2 * i / ParticlesPerBurst;
                            DrawSpark(context, new Spark
                            {
                                Origin = new Point(75 + burst * 105, 55),
                                Velocity = new Vector(Math.Cos(angle) * 90, Math.Sin(angle) * 90),
                                Age = 0.55 - burst * 0.2, Lifetime = 1.5, Radius = 1.7,
                                ColorIndex = i % SparkBrushes.Length
                            });
                        }
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Spark
        {
            public Point Origin;
            public Vector Velocity;
            public double Age;
            public double Lifetime;
            public double Radius;
            public int ColorIndex;
        }
    }
}
