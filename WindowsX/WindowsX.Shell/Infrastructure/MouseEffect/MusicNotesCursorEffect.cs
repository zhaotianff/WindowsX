using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class MusicNotesCursorEffect : UIElement, ICursorEffect
    {
        public const int MaxParticles = 120;
        public const int MaxEmissionPerUpdate = 8;
        private static readonly Geometry[] NoteShapes = { CreateNote(false), CreateNote(true) };
        private static readonly Brush[] NoteBrushes = CreateBrushes();
        private readonly MouseEffectSetting settings;
        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly List<Note> particles = new List<Note>(MaxParticles);
        private readonly Random random = new Random();
        private Point? lastEmissionPoint;
        private double elapsedSinceEmission;
        private int nextKind;
        private int nextColor;

        public int ParticleCount => particles.Count;
        protected override int VisualChildrenCount => 1;

        public MusicNotesCursorEffect(MouseEffectSetting settings = null)
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
                var note = particles[i];
                note.Age += elapsedSeconds;
                if (note.Age >= note.Lifetime) particles.RemoveAt(i);
                else particles[i] = note;
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
                if (double.IsFinite(distance) && distance >= 12 && elapsedSinceEmission >= 0.045)
                {
                    var count = elapsedSinceEmission > 0.25 ? 1 : (int)Math.Min(MaxEmissionPerUpdate, Math.Ceiling(distance / 22));
                    for (var i = 1; i <= count; i++) Emit(lastEmissionPoint.Value + movement * ((double)i / count));
                    lastEmissionPoint = cursor;
                    elapsedSinceEmission = 0;
                }
            }
            using var context = visual.RenderOpen();
            context.PushOpacity(settings.MusicNotesOpacity);
            foreach (var note in particles) DrawNote(context, note);
            context.Pop();
        }

        public void Clear()
        {
            particles.Clear();
            lastEmissionPoint = null;
            elapsedSinceEmission = 0;
            nextKind = 0;
            nextColor = 0;
            using (visual.RenderOpen()) { }
        }

        private void Emit(Point point)
        {
            if (particles.Count == MaxParticles) particles.RemoveAt(0);
            particles.Add(new Note
            {
                Origin = point, Size = 0.65 + random.NextDouble() * 0.35,
                Lifetime = 1.3 + random.NextDouble() * 0.5,
                Velocity = new Vector(random.NextDouble() * 12 - 6, -20 - random.NextDouble() * 12),
                Phase = random.NextDouble() * Math.PI * 2, Kind = nextKind, ColorIndex = nextColor
            });
            nextKind = (nextKind + 1) % NoteShapes.Length;
            nextColor = (nextColor + 1) % NoteBrushes.Length;
        }

        private static void DrawNote(DrawingContext context, Note note)
        {
            var progress = note.Age / note.Lifetime;
            var center = note.Origin + note.Velocity * note.Age + new Vector(5 * (Math.Sin(note.Phase + note.Age * 2.5) - Math.Sin(note.Phase)), 0);
            var transform = Matrix.Identity;
            transform.Scale(note.Size, note.Size);
            transform.Rotate(Math.Sin(note.Phase + note.Age * 3) * 14);
            transform.Translate(center.X, center.Y);
            context.PushOpacity(Math.Pow(Math.Max(0, 1 - progress), 1.3));
            context.PushTransform(new MatrixTransform(transform));
            context.DrawGeometry(NoteBrushes[note.ColorIndex], null, NoteShapes[note.Kind]);
            context.Pop();
            context.Pop();
        }

        private static Geometry CreateNote(bool paired)
        {
            var group = new GeometryGroup { FillRule = FillRule.Nonzero };
            group.Children.Add(new EllipseGeometry(new Point(-4, 6), 4, 2.7) { Transform = new RotateTransform(-20, -4, 6) });
            group.Children.Add(new RectangleGeometry(new Rect(-1.5, -10, 2, 16)));
            if (paired)
            {
                group.Children.Add(new EllipseGeometry(new Point(7, 4), 4, 2.7) { Transform = new RotateTransform(-20, 7, 4) });
                group.Children.Add(new RectangleGeometry(new Rect(9.5, -12, 2, 16)));
                var beam = new StreamGeometry();
                using (var context = beam.Open())
                {
                    context.BeginFigure(new Point(-1.5, -10), true, true);
                    context.LineTo(new Point(11.5, -12), true, false);
                    context.LineTo(new Point(11.5, -8.5), true, false);
                    context.LineTo(new Point(-1.5, -6.5), true, false);
                }
                group.Children.Add(beam);
            }
            else
            {
                var flag = new StreamGeometry();
                using (var context = flag.Open())
                {
                    context.BeginFigure(new Point(0, -10), true, true);
                    context.BezierTo(new Point(3, -6), new Point(11, -7), new Point(6, 1), true, false);
                    context.BezierTo(new Point(8, -5), new Point(2, -3), new Point(0, -6), true, false);
                }
                group.Children.Add(flag);
            }
            group.Freeze();
            return group;
        }

        private static Brush[] CreateBrushes()
        {
            var colors = new[] { Color.FromRgb(255, 155, 204), Color.FromRgb(129, 219, 255), Color.FromRgb(192, 162, 255), Color.FromRgb(255, 222, 125) };
            var brushes = new Brush[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var brush = new LinearGradientBrush(Colors.White, colors[i], 90);
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
                    context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(35, 30, 55)), null, new Rect(0, 0, 260, 170), 10, 10);
                    for (var i = 0; i < 9; i++)
                        DrawNote(context, new Note
                        {
                            Origin = new Point(30 + i * 25, 88 + Math.Sin(i * 0.6) * 19),
                            Size = 1, Lifetime = 1.8, Age = (8 - i) * 0.1,
                            Velocity = new Vector(0, -22), Phase = i * 0.8, Kind = i % 2, ColorIndex = i % NoteBrushes.Length
                        });
                }
                var image = new DrawingImage(drawing);
                image.Freeze();
                return image;
            }
        }

        private struct Note
        {
            public Point Origin;
            public Vector Velocity;
            public double Size;
            public double Lifetime;
            public double Age;
            public double Phase;
            public int Kind;
            public int ColorIndex;
        }
    }
}
