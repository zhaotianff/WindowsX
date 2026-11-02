using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsX.Config.Model;
using Drawing = System.Drawing;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class GdiParticleCursorEffect : UIElement, IDisposable
    {
        public const int MaxParticles = 240;

        private static readonly Drawing.Color[] SparkColors =
        {
            Drawing.Color.Gold, Drawing.Color.DeepPink, Drawing.Color.DeepSkyBlue,
            Drawing.Color.SpringGreen, Drawing.Color.Orange, Drawing.Color.MediumOrchid
        };

        private readonly bool fireworks;
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Particle> particles = new List<Particle>(MaxParticles);
        private readonly Random random = new Random();
        private readonly Drawing.SolidBrush brush;
        private readonly Drawing.Pen pen;
        private Drawing.Bitmap bitmap;
        private Drawing.Graphics graphics;
        private WriteableBitmap image;
        private DpiScale bufferDpi;
        private Point? lastEmissionPoint;
        private double emissionDelay;
        private string catkinColor;
        private Drawing.Color catkinFill;
        private Drawing.Color catkinHalo;
        private Drawing.Color catkinStem;

        public int ParticleCount => particles.Count;
        public bool IsDisposed { get; private set; }
        protected override int VisualChildrenCount => 1;

        public GdiParticleCursorEffect(MouseEffectType effectType, MouseEffectSetting settings = null)
        {
            if (effectType != MouseEffectType.FireworksGdiPlus && effectType != MouseEffectType.WillowCatkinsGdiPlus)
                throw new ArgumentOutOfRangeException(nameof(effectType));

            fireworks = effectType == MouseEffectType.FireworksGdiPlus;
            this.settings = settings ?? new MouseEffectSetting();
            brush = new Drawing.SolidBrush(Drawing.Color.White);
            pen = new Drawing.Pen(Drawing.Color.White)
            {
                StartCap = Drawing.Drawing2D.LineCap.Round,
                EndCap = Drawing.Drawing2D.LineCap.Round
            };
            IsHitTestVisible = false;
            AddVisualChild(visual);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            return visual;
        }

        public void Update(Point cursor, double elapsedSeconds, Size viewport, DpiScale dpi)
        {
            VerifyAccess();
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(GdiParticleCursorEffect));
            if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y))
                return;
            if (!double.IsFinite(viewport.Width) || !double.IsFinite(viewport.Height) || viewport.Width <= 0 || viewport.Height <= 0)
            {
                Clear();
                return;
            }
            if (!double.IsFinite(dpi.DpiScaleX) || !double.IsFinite(dpi.DpiScaleY) || dpi.DpiScaleX <= 0 || dpi.DpiScaleY <= 0)
                throw new ArgumentOutOfRangeException(nameof(dpi));

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
                if (fireworks)
                {
                    particle.Velocity *= Math.Exp(-0.65 * motionStep);
                    particle.Velocity.Y += 100 * motionStep;
                    particle.Position += particle.Velocity * motionStep;
                }
                else
                {
                    particle.Position.X += (particle.Velocity.X + Math.Sin(particle.Age * 2.2 + particle.Phase) * 22) * motionStep;
                    particle.Position.Y += particle.Velocity.Y * motionStep;
                }
                particles[i] = particle;
            }

            emissionDelay = Math.Max(0, emissionDelay - elapsedSeconds);
            if (emissionDelay == 0 && new Rect(viewport).Contains(cursor) &&
                (!lastEmissionPoint.HasValue || (cursor - lastEmissionPoint.Value).LengthSquared >= 9))
            {
                Emit(cursor);
                lastEmissionPoint = cursor;
                emissionDelay = fireworks ? 0.14 : 0.06;
            }
            Draw(viewport, dpi);
        }

        private void Emit(Point cursor)
        {
            var count = fireworks ? 28 : 3;
            for (var i = 0; i < count && particles.Count < MaxParticles; i++)
            {
                var angle = (i + random.NextDouble() * 0.5) * Math.PI * 2 / count;
                var speed = (65 + random.NextDouble() * 110) * settings.FireworksExpansion;
                particles.Add(new Particle
                {
                    Position = fireworks ? cursor : new Point(cursor.X + random.NextDouble() * 16 - 8, cursor.Y + random.NextDouble() * 12 - 6),
                    Velocity = fireworks ? new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed)
                        : new Vector(8 + random.NextDouble() * 14, 12 + random.NextDouble() * 18),
                    Lifetime = fireworks ? 0.75 + random.NextDouble() * 0.6 : 2.5 + random.NextDouble() * 1.5,
                    Size = fireworks ? 1.2 + random.NextDouble() : 2 + random.NextDouble() * 2,
                    Phase = random.NextDouble() * Math.PI * 2,
                    ColorIndex = random.Next(SparkColors.Length)
                });
            }
        }

        private void Draw(Size viewport, DpiScale dpi)
        {
            if (!fireworks && catkinColor != settings.WillowCatkinsColor)
            {
                catkinColor = settings.WillowCatkinsColor;
                catkinFill = Drawing.ColorTranslator.FromHtml(catkinColor);
                var original = catkinColor == MouseEffectSetting.DefaultWillowCatkinsColor;
                catkinHalo = original ? Drawing.Color.FromArgb(244, 255, 220) : catkinFill;
                catkinStem = original ? Drawing.Color.FromArgb(118, 137, 100)
                    : Drawing.Color.FromArgb((int)(catkinFill.R * 0.5), (int)(catkinFill.G * 0.5), (int)(catkinFill.B * 0.5));
            }
            var bounds = Rect.Empty;
            foreach (var particle in particles)
            {
                bounds.Union(particle.Position);
                if (fireworks)
                    bounds.Union(particle.Position - particle.Velocity * 0.035);
            }
            if (!bounds.IsEmpty)
            {
                var padding = fireworks ? 12 : Math.Max(12, 8 * settings.WillowCatkinsSizeScale + 2);
                bounds.Inflate(padding, padding);
                bounds.Intersect(new Rect(viewport));
            }
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
            {
                using (visual.RenderOpen()) { }
                return;
            }

            var origin = new Point(Math.Floor(bounds.Left * dpi.DpiScaleX) / dpi.DpiScaleX,
                Math.Floor(bounds.Top * dpi.DpiScaleY) / dpi.DpiScaleY);
            var width = checked((int)Math.Ceiling((bounds.Right - origin.X) * dpi.DpiScaleX));
            var height = checked((int)Math.Ceiling((bounds.Bottom - origin.Y) * dpi.DpiScaleY));
            EnsureBuffer(width, height, dpi);
            graphics.ResetTransform();
            graphics.ResetClip();
            graphics.Clear(Drawing.Color.Transparent);
            graphics.ScaleTransform((float)dpi.DpiScaleX, (float)dpi.DpiScaleY);
            graphics.TranslateTransform(-(float)origin.X, -(float)origin.Y);
            graphics.SetClip(new Drawing.RectangleF(0, 0, (float)viewport.Width, (float)viewport.Height));
            foreach (var particle in particles)
                DrawParticle(particle);
            graphics.Flush(Drawing.Drawing2D.FlushIntention.Sync);

            // GDI+ PArgb and WPF Pbgra32 both store premultiplied BGRA pixels.
            var pixels = bitmap.LockBits(new Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                Drawing.Imaging.ImageLockMode.ReadOnly, Drawing.Imaging.PixelFormat.Format32bppPArgb);
            try
            {
                image.WritePixels(new Int32Rect(0, 0, bitmap.Width, bitmap.Height), pixels.Scan0,
                    checked(pixels.Stride * bitmap.Height), pixels.Stride);
            }
            finally
            {
                bitmap.UnlockBits(pixels);
            }
            using var context = visual.RenderOpen();
            context.DrawImage(image, new Rect(origin, new Size(bitmap.Width / dpi.DpiScaleX, bitmap.Height / dpi.DpiScaleY)));
        }

        private void DrawParticle(Particle particle)
        {
            var opacity = Math.Max(0, 1 - particle.Age / particle.Lifetime);
            opacity *= fireworks ? settings.FireworksOpacity : settings.WillowCatkinsOpacity;
            var x = (float)particle.Position.X;
            var y = (float)particle.Position.Y;
            var size = (float)particle.Size;
            if (fireworks)
            {
                var color = Drawing.Color.FromArgb((int)(255 * opacity), SparkColors[particle.ColorIndex]);
                pen.Color = color;
                pen.Width = 1.5f;
                brush.Color = color;
                graphics.DrawLine(pen, x - (float)particle.Velocity.X * 0.035f, y - (float)particle.Velocity.Y * 0.035f, x, y);
                graphics.FillEllipse(brush, x - size, y - size, size * 2, size * 2);
            }
            else
            {
                opacity *= Math.Min(1, particle.Age / 0.15);
                size *= (float)settings.WillowCatkinsSizeScale;
                brush.Color = Drawing.Color.FromArgb((int)(40 * opacity), catkinHalo);
                graphics.FillEllipse(brush, x - size * 2, y - size * 1.6f, size * 4, size * 3.2f);
                brush.Color = Drawing.Color.FromArgb((int)(255 * opacity), catkinFill);
                graphics.FillEllipse(brush, x - size * 0.55f, y - size * 0.3f, size * 1.1f, size * 0.6f);
                pen.Width = 0.8f * (float)settings.WillowCatkinsSizeScale;
                pen.Color = Drawing.Color.FromArgb((int)(255 * opacity), catkinStem);
                graphics.DrawEllipse(pen, x - size * 0.55f, y - size * 0.3f, size * 1.1f, size * 0.6f);
                pen.Color = brush.Color;
                for (var i = 0; i < 4; i++)
                {
                    var angle = i * Math.PI / 4 + Math.Sin(particle.Age * 1.5 + particle.Phase) * 0.5;
                    var dx = (float)Math.Cos(angle) * size * 1.4f;
                    var dy = (float)Math.Sin(angle) * size * 1.4f;
                    graphics.DrawLine(pen, x - dx, y - dy, x + dx, y + dy);
                }
            }
        }

        private void EnsureBuffer(int width, int height, DpiScale dpi)
        {
            if (bitmap != null && bitmap.Width >= width && bitmap.Height >= height &&
                bufferDpi.DpiScaleX == dpi.DpiScaleX && bufferDpi.DpiScaleY == dpi.DpiScaleY)
                return;

            width = Math.Max(width, bitmap?.Width ?? 0);
            height = Math.Max(height, bitmap?.Height ?? 0);
            ReleaseBuffer();
            try
            {
                bitmap = new Drawing.Bitmap(width, height, Drawing.Imaging.PixelFormat.Format32bppPArgb);
                graphics = Drawing.Graphics.FromImage(bitmap);
                graphics.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                image = new WriteableBitmap(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32, null);
                bufferDpi = dpi;
            }
            catch
            {
                ReleaseBuffer();
                throw;
            }
        }

        public void Clear()
        {
            VerifyAccess();
            particles.Clear();
            lastEmissionPoint = null;
            emissionDelay = 0;
            using (visual.RenderOpen()) { }
            ReleaseBuffer();
        }

        private void ReleaseBuffer()
        {
            graphics?.Dispose();
            graphics = null;
            bitmap?.Dispose();
            bitmap = null;
            image = null;
        }

        public void Dispose()
        {
            VerifyAccess();
            if (IsDisposed)
                return;
            Clear();
            pen.Dispose();
            brush.Dispose();
            IsDisposed = true;
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
