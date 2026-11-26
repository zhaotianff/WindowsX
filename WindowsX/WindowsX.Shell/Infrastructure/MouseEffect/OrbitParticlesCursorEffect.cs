using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class OrbitParticlesCursorEffect : UIElement, ICursorEffect
    {
        public const int OrbitParticleCount = 12;
        public const int ParticlesPerLayer = 6;
        public const int MaxTrailSamples = 16;
        public const double TrailLifetime = 0.45;
        public const double MaxTrailLength = 160;
        private const double SampleInterval = 0.03;
        private const double CycleDuration = 6;
        private static readonly Color[] Colors = { Color.FromRgb(92, 225, 255), Color.FromRgb(179, 139, 255), Color.FromRgb(255, 145, 205) };
        private static readonly Brush[] CoreBrushes = CreateBrushes(false);
        private static readonly Brush[] GlowBrushes = CreateBrushes(true);
        private static readonly Pen[,,] TrailPens = CreateTrailPens();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<TrailSample> history = new List<TrailSample>(MaxTrailSamples);
        private Point? center;
        private double cycleTime;
        private double orbitTime;
        private double timeSinceSample;
        private int currentLayers = MouseEffectSetting.DefaultOrbitParticlesLayers;
        private double currentRadius = MouseEffectSetting.DefaultOrbitParticlesRadius;
        private string cachedColor1 = MouseEffectSetting.DefaultOrbitParticlesColor1;
        private string cachedColor2 = MouseEffectSetting.DefaultOrbitParticlesColor2;
        private string cachedColor3 = MouseEffectSetting.DefaultOrbitParticlesColor3;
        private Brush[] currentCores = CoreBrushes;
        private Brush[] currentGlows = GlowBrushes;
        private Pen[,,] currentPens = TrailPens;

        public int ParticleCount => center.HasValue ? currentLayers * ParticlesPerLayer : 0;
        public int TrailSampleCount => history.Count;
        protected override int VisualChildrenCount => 1;

        public OrbitParticlesCursorEffect(MouseEffectSetting settings = null)
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

            if (center.HasValue)
            {
                var delta = cursor - center.Value;
                if (!double.IsFinite(delta.X) || !double.IsFinite(delta.Y)) return;
                center += delta * (1 - Math.Exp(-elapsedSeconds * 12));
            }
            else center = cursor;

            cycleTime = (cycleTime + elapsedSeconds % CycleDuration) % CycleDuration;
            var period = CycleDuration / settings.OrbitParticlesSpeed;
            orbitTime = (orbitTime + elapsedSeconds % period * settings.OrbitParticlesSpeed) % CycleDuration;
            UpdateAppearance();
            UpdateHistory(elapsedSeconds);
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.OrbitParticlesOpacity);
            DrawParticles(context, center.Value, orbitTime, history, currentLayers, currentRadius, currentCores, currentGlows, currentPens, cycleTime);
            context.Pop();
        }

        private void UpdateAppearance()
        {
            if (currentLayers != settings.OrbitParticlesLayers || currentRadius != settings.OrbitParticlesRadius)
            {
                currentLayers = settings.OrbitParticlesLayers;
                currentRadius = settings.OrbitParticlesRadius;
                history.Clear();
                timeSinceSample = 0;
            }
            if (cachedColor1 == settings.OrbitParticlesColor1 && cachedColor2 == settings.OrbitParticlesColor2
                && cachedColor3 == settings.OrbitParticlesColor3) return;

            cachedColor1 = settings.OrbitParticlesColor1;
            cachedColor2 = settings.OrbitParticlesColor2;
            cachedColor3 = settings.OrbitParticlesColor3;
            if (cachedColor1 == MouseEffectSetting.DefaultOrbitParticlesColor1
                && cachedColor2 == MouseEffectSetting.DefaultOrbitParticlesColor2
                && cachedColor3 == MouseEffectSetting.DefaultOrbitParticlesColor3)
            {
                currentCores = CoreBrushes;
                currentGlows = GlowBrushes;
                currentPens = TrailPens;
                return;
            }
            var colors = new[] { (Color)ColorConverter.ConvertFromString(cachedColor1),
                (Color)ColorConverter.ConvertFromString(cachedColor2), (Color)ColorConverter.ConvertFromString(cachedColor3) };
            currentCores = CreateBrushes(false, colors);
            currentGlows = CreateBrushes(true, colors);
            currentPens = CreateTrailPens(colors);
        }

        private void UpdateHistory(double elapsedSeconds)
        {
            if (elapsedSeconds > 0.25)
            {
                history.Clear();
                timeSinceSample = 0;
            }
            for (var i = history.Count - 1; i >= 0; i--)
            {
                var sample = history[i];
                sample.Age += elapsedSeconds;
                if (sample.Age >= TrailLifetime) history.RemoveAt(i);
                else history[i] = sample;
            }
            timeSinceSample += Math.Min(elapsedSeconds, SampleInterval);
            if (history.Count == 0 || timeSinceSample >= SampleInterval)
            {
                if (history.Count == MaxTrailSamples) history.RemoveAt(0);
                history.Add(new TrailSample { Center = center.Value, Time = orbitTime });
                timeSinceSample = 0;
            }
        }

        public void Clear()
        {
            center = null;
            cycleTime = 0;
            orbitTime = 0;
            history.Clear();
            timeSinceSample = 0;
            using (visual.RenderOpen()) { }
        }

        private static Vector Offset(int index, double time, double innerRadius)
        {
            var layer = index / ParticlesPerLayer;
            var angle = index % ParticlesPerLayer * Math.PI / 3 + layer * Math.PI / 6
                + time * Math.PI * 2 / CycleDuration * (layer % 2 == 1 ? -1 : 1);
            var radius = innerRadius + layer * 15;
            return new Vector(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
        }

        private static void DrawParticles(DrawingContext context, Point origin, double time, List<TrailSample> samples = null,
            int layers = MouseEffectSetting.DefaultOrbitParticlesLayers, double innerRadius = MouseEffectSetting.DefaultOrbitParticlesRadius,
            Brush[] cores = null, Brush[] glows = null, Pen[,,] pens = null, double? pulseTime = null)
        {
            cores ??= CoreBrushes;
            glows ??= GlowBrushes;
            pens ??= TrailPens;
            var count = layers * ParticlesPerLayer;
            if (samples != null)
                for (var i = 0; i < count; i++)
                    DrawTrail(context, origin + Offset(i, time, innerRadius), i, samples, innerRadius, pens);
            for (var i = 0; i < count; i++)
            {
                var colorIndex = i % Colors.Length;
                var radius = 1.8 + i % 3 * 0.3;
                var point = origin + Offset(i, time, innerRadius);
                context.PushOpacity(0.8 + 0.2 * Math.Sin((pulseTime ?? time) * Math.PI * 2 / CycleDuration * 2 + i));
                context.DrawEllipse(glows[colorIndex], null, point, radius * 6, radius * 6);
                context.DrawEllipse(cores[colorIndex], null, point, radius, radius);
                context.DrawEllipse(Brushes.White, null, point, radius * 0.5, radius * 0.5);
                context.Pop();
            }
        }

        private static void DrawTrail(DrawingContext context, Point newest, int index, List<TrailSample> samples, double innerRadius, Pen[,,] pens)
        {
            var length = 0.0;
            var newerAge = 0.0;
            for (var j = samples.Count - 1; j >= 0; j--)
            {
                var sample = samples[j];
                var older = sample.Center + Offset(index, sample.Time, innerRadius);
                var distance = (older - newest).Length;
                if (!double.IsFinite(distance) || length + distance > MaxTrailLength) break;
                if (distance > 0.001)
                {
                    var progress = Math.Clamp((sample.Age + newerAge) / (2 * TrailLifetime), 0, 1);
                    var level = Math.Min(MaxTrailSamples - 1, (int)(progress * MaxTrailSamples));
                    context.PushOpacity(Math.Pow(1 - progress, 1.6));
                    context.DrawLine(pens[index % Colors.Length, level, 0], newest, older);
                    context.DrawLine(pens[index % Colors.Length, level, 1], newest, older);
                    context.Pop();
                }
                length += distance;
                newest = older;
                newerAge = sample.Age;
            }
        }

        private static Pen[,,] CreateTrailPens(Color[] colors = null)
        {
            colors ??= Colors;
            var pens = new Pen[colors.Length, MaxTrailSamples, 2];
            for (var colorIndex = 0; colorIndex < colors.Length; colorIndex++)
                for (var level = 0; level < MaxTrailSamples; level++)
                    for (var layer = 0; layer < 2; layer++)
                    {
                        var color = colors[colorIndex];
                        var brush = new SolidColorBrush(Color.FromArgb(layer == 0 ? (byte)40 : (byte)210, color.R, color.G, color.B));
                        var pen = new Pen(brush, (layer == 0 ? 7 : 2) * (1 - level / (double)MaxTrailSamples))
                        {
                            StartLineCap = PenLineCap.Round,
                            EndLineCap = PenLineCap.Round,
                            LineJoin = PenLineJoin.Round
                        };
                        pen.Freeze();
                        pens[colorIndex, level, layer] = pen;
                    }
            return pens;
        }

        private struct TrailSample
        {
            public Point Center;
            public double Time;
            public double Age;
        }

        private static Brush[] CreateBrushes(bool glow, Color[] colors = null)
        {
            colors ??= Colors;
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                Brush brush = glow ? new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(210, color.R, color.G, color.B), 0),
                        new GradientStop(Color.FromArgb(85, color.R, color.G, color.B), 0.35),
                        new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1)
                    }
                } : new SolidColorBrush(color);
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(22, 25, 42)), null, new Rect(0, 0, 260, 170), 10, 10);
                    context.PushTransform(new MatrixTransform(1.35, 0, 0, 1.35, 130, 85));
                    var samples = new List<TrailSample>(MaxTrailSamples);
                    for (var i = MaxTrailSamples - 1; i >= 0; i--)
                    {
                        var age = i * SampleInterval;
                        samples.Add(new TrailSample { Center = new Point(), Time = 0.4 - age, Age = age });
                    }
                    DrawParticles(context, new Point(), 0.4, samples);
                    context.Pop();
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }
    }
}
