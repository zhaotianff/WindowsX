using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WindowsX.Config.Helper;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    [DoNotParallelize]
    public class SnowflakeAndStarEffectTests
    {
        [TestMethod]
        public void NewTypesPreserveExistingEnumValues()
        {
            Assert.AreEqual(0, (int)MouseEffectType.FollowingDot);
            Assert.AreEqual(1, (int)MouseEffectType.CursorTrail);
            Assert.AreEqual(2, (int)MouseEffectType.Fireworks);
            Assert.AreEqual(3, (int)MouseEffectType.WillowCatkins);
            Assert.AreEqual(4, (int)MouseEffectType.FireworksGdiPlus);
            Assert.AreEqual(5, (int)MouseEffectType.WillowCatkinsGdiPlus);
            Assert.AreEqual(6, (int)MouseEffectType.Snowflakes);
            Assert.AreEqual(7, (int)MouseEffectType.FivePointedStars);
        }

        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FireworksOpacity\":0.3}}")]
        public void OldConfigurationsProvideDefaultOpacity(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual(1.0, settings.SnowflakesOpacity);
            Assert.AreEqual(1.0, settings.FivePointedStarsOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityValuesAreNormalized(double value, double expected)
        {
            var settings = new MouseEffectSetting { SnowflakesOpacity = value, FivePointedStarsOpacity = value };
            Assert.AreEqual(expected, settings.SnowflakesOpacity);
            Assert.AreEqual(expected, settings.FivePointedStarsOpacity);
        }

        [TestMethod]
        public void SnowflakesHaveSixBranchedArms()
        {
            RunSta(() =>
            {
                var effect = new ParticleCursorEffect(MouseEffectType.Snowflakes);
                effect.Update(new Point(200, 200), 0.016);
                effect.Update(new Point(200, 200), 0.1);
                var drawings = GetGeometryDrawings(GetDrawing(effect)).ToArray();
                Assert.AreEqual(effect.ParticleCount * 18, drawings.Length);
                for (var particle = 0; particle < effect.ParticleCount; particle++)
                {
                    var first = (LineGeometry)drawings[particle * 18].Geometry;
                    var firstArm = first.EndPoint - first.StartPoint;
                    for (var arm = 0; arm < 6; arm++)
                    {
                        var line = (LineGeometry)drawings[particle * 18 + arm * 3].Geometry;
                        Assert.AreEqual(first.StartPoint, line.StartPoint);
                        Assert.AreEqual(firstArm.Length, (line.EndPoint - line.StartPoint).Length, 0.0001);
                    }
                    var second = (LineGeometry)drawings[particle * 18 + 3].Geometry;
                    var secondArm = second.EndPoint - second.StartPoint;
                    Assert.AreEqual(0.5, Vector.Multiply(firstArm, secondArm) / (firstArm.Length * secondArm.Length), 0.0001);
                }
            });
        }

        [TestMethod]
        public void SnowflakeSizesVaryWithinOriginalMaximumAndRemainStable()
        {
            RunSta(() =>
            {
                var effect = new ParticleCursorEffect(MouseEffectType.Snowflakes);
                typeof(ParticleCursorEffect).GetField("random", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(effect, new Random(12345));
                for (var i = 0; i < 20; i++)
                    effect.Update(new Point(200 + i * 10, 200), 0.08);

                double[] ReadSizes() => GetGeometryDrawings(GetDrawing(effect))
                    .Where((drawing, index) => index % 18 == 0)
                    .Select(drawing => (LineGeometry)drawing.Geometry)
                    .Select(line => (line.EndPoint - line.StartPoint).Length)
                    .ToArray();

                var sizes = ReadSizes();
                Assert.AreEqual(effect.ParticleCount, sizes.Length);
                Assert.IsTrue(sizes.All(size => size >= 2 - 0.0001 && size <= 8 + 0.0001));
                Assert.IsTrue(sizes.Min() < 5, "Snowflakes should include smaller sizes than the previous range.");
                Assert.IsTrue(sizes.Max() - sizes.Min() > 1);

                effect.Update(new Point(390, 200), 0.05);
                var updatedSizes = ReadSizes();
                Assert.AreEqual(sizes.Length, updatedSizes.Length);
                for (var i = 0; i < sizes.Length; i++)
                    Assert.AreEqual(sizes[i], updatedSizes[i], 0.0001);
            });
        }

        [TestMethod]
        public void StarsHaveFiveOuterAndFiveInnerVertices()
        {
            RunSta(() =>
            {
                var effect = new ParticleCursorEffect(MouseEffectType.FivePointedStars);
                effect.Update(new Point(200, 200), 0.016);
                effect.Update(new Point(200, 200), 0.1);
                var drawings = GetGeometryDrawings(GetDrawing(effect)).ToArray();
                Assert.AreEqual(effect.ParticleCount, drawings.Length);
                foreach (var drawing in drawings)
                {
                    Assert.IsNotNull(drawing.Brush);
                    Assert.IsTrue(drawing.Geometry.IsFrozen);
                    var figure = PathGeometry.CreateFromGeometry(drawing.Geometry).Figures.Single();
                    Assert.IsTrue(figure.IsClosed && figure.IsFilled);
                    var points = new List<Point> { figure.StartPoint };
                    foreach (var segment in figure.Segments)
                    {
                        if (segment is PolyLineSegment polyline) points.AddRange(polyline.Points);
                        else points.Add(((LineSegment)segment).Point);
                    }
                    Assert.AreEqual(10, points.Count);
                    for (var i = 0; i < points.Count; i++)
                        Assert.AreEqual(i % 2 == 0 ? 1.0 : 0.45, ((Vector)points[i]).Length, 0.0001);
                }
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Snowflakes)]
        [DataRow(MouseEffectType.FivePointedStars)]
        public void RunningParticlesRespectIndependentOpacity(MouseEffectType type)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new ParticleCursorEffect(type, settings);
                effect.Measure(new Size(640, 480));
                effect.Arrange(new Rect(0, 0, 640, 480));
                var cursor = new Point(320, 240);
                effect.Update(cursor, 0.016);
                effect.Update(cursor, 0.1);
                var full = GetTotalAlpha(effect);
                Assert.IsTrue(full > 0);
                SetOpacity(settings, type, 0.2);
                effect.Update(cursor, 0.05);
                var reduced = GetTotalAlpha(effect);
                Assert.IsTrue(reduced > 0 && reduced < full);
                SetOpacity(settings, type, 0);
                effect.Update(cursor, 0.05);
                Assert.AreEqual(0L, GetTotalAlpha(effect));
                SetOpacity(settings, type, 0.8);
                effect.Update(cursor, 0.05);
                Assert.IsTrue(GetTotalAlpha(effect) > 0);
                Assert.AreEqual(1.0, type == MouseEffectType.Snowflakes ? settings.FivePointedStarsOpacity : settings.SnowflakesOpacity);
                Assert.AreEqual(1.0, settings.FireworksOpacity);
                Assert.AreEqual(1.0, settings.WillowCatkinsOpacity);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Snowflakes, "switch")]
        [DataRow(MouseEffectType.Snowflakes, "stop")]
        [DataRow(MouseEffectType.Snowflakes, "close")]
        [DataRow(MouseEffectType.FivePointedStars, "switch")]
        [DataRow(MouseEffectType.FivePointedStars, "stop")]
        [DataRow(MouseEffectType.FivePointedStars, "close")]
        public void WindowClearsNewParticleEffects(MouseEffectType type, string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                var effect = new ParticleCursorEffect(type);
                try
                {
                    window.UpdateMouseEffectType(type);
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("particleEffect", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FollowingDot);
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                    Assert.IsNull(field.GetValue(window));
                }
                finally { window.Close(); }
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Snowflakes, "雪花设置", 70.0)]
        [DataRow(MouseEffectType.FivePointedStars, "五角星设置", 40.0)]
        public void DialogLoadsResetsAndDiscardsEdits(MouseEffectType type, string title, double opacity)
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.SnowflakesOpacity = 0.7;
                config.MouseEffectSetting.FivePointedStarsOpacity = 0.4;
                var window = new MouseEffectSettingWindow(type, config);
                try
                {
                    Assert.AreEqual(title, window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(opacity, slider.Value, 0.001);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_ParticleOpacity")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_Expansion")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((TextBlock)window.FindName("text_SharedSettings")).Visibility);
                    slider.Value = 10;
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.7, config.MouseEffectSetting.SnowflakesOpacity);
                Assert.AreEqual(0.4, config.MouseEffectSetting.FivePointedStarsOpacity);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Snowflakes)]
        [DataRow(MouseEffectType.FivePointedStars)]
        public void DialogPersistsOnlySelectedEffect(MouseEffectType type)
        {
            RunSta(() =>
            {
                var originalDirectory = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-ShapeTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.SnowflakesOpacity = 0.7;
                    config.MouseEffectSetting.FivePointedStarsOpacity = 0.4;
                    config.MouseEffectSetting.WillowCatkinsOpacity = 0.6;
                    config.MouseEffectSetting.FollowingSize = 80;
                    var window = new MouseEffectSettingWindow(type, config);
                    Exception? callbackFailure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { callbackFailure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    var result = window.ShowDialog();
                    if (callbackFailure != null) Assert.Fail(callbackFailure.ToString());
                    Assert.AreEqual(true, result);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(type == MouseEffectType.Snowflakes ? 0.25 : 0.7, restored.MouseEffectSetting.SnowflakesOpacity);
                    Assert.AreEqual(type == MouseEffectType.FivePointedStars ? 0.25 : 0.4, restored.MouseEffectSetting.FivePointedStarsOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.WillowCatkinsOpacity);
                    Assert.AreEqual(80.0, restored.MouseEffectSetting.FollowingSize);
                    var reopened = new MouseEffectSettingWindow(type, restored);
                    try { Assert.AreEqual(25.0, ((Slider)reopened.FindName("slider_Opacity")).Value); }
                    finally { reopened.Close(); }
                }
                finally
                {
                    Environment.CurrentDirectory = originalDirectory;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static void SetOpacity(MouseEffectSetting settings, MouseEffectType type, double value)
        {
            if (type == MouseEffectType.Snowflakes) settings.SnowflakesOpacity = value;
            else settings.FivePointedStarsOpacity = value;
        }

        private static DrawingGroup GetDrawing(ParticleCursorEffect effect)
        {
            return ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        }

        private static IEnumerable<GeometryDrawing> GetGeometryDrawings(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
            {
                foreach (var child in group.Children)
                    foreach (var result in GetGeometryDrawings(child))
                        yield return result;
            }
        }

        private static long GetTotalAlpha(ParticleCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(640, 480, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[640 * 480 * 4];
            bitmap.CopyPixels(pixels, 640 * 4, 0);
            long alpha = 0;
            for (var i = 3; i < pixels.Length; i += 4) alpha += pixels[i];
            return alpha;
        }

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
                finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null) Assert.Fail(failure.ToString());
        }
    }
}
