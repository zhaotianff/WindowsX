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
    public class CottonCandyEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"ColorfulMistOpacity\":0.4}}")]
        public void DefaultsAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(18, (int)MouseEffectType.FollowingAnimal);
            Assert.AreEqual(19, (int)MouseEffectType.CottonCandy);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.CottonCandyOpacity);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.CottonCandySizeScale);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { CottonCandyOpacity = value }.CottonCandyOpacity);
        }

        [TestMethod]
        public void PuffsHaveSoftLobesHighlightsAndFloatExpandAndFade()
        {
            RunSta(() =>
            {
                var effect = new CottonCandyCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var initial = Drawing(effect);
                var shapes = Shapes(initial).ToArray();
                Assert.AreEqual(7, shapes.Length);
                Assert.IsTrue(shapes.All(s => s.Pen == null && s.Geometry is EllipseGeometry && s.Brush.IsFrozen));
                Assert.IsTrue(shapes.Select(s => ((EllipseGeometry)s.Geometry).Center).Distinct().Count() > 4);
                var brush = (RadialGradientBrush)shapes[0].Brush;
                Assert.AreEqual((byte)0, brush.GradientStops.Last().Color.A);
                Assert.IsTrue(brush.GradientStops[0].Color.A > 200);
                Assert.IsTrue(brush.GradientStops[0].Color.G > brush.GradientStops[1].Color.G);
                var before = Groups(initial).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                effect.Update(point, 0.4);
                var after = Drawing(effect);
                Assert.IsTrue(((EllipseGeometry)Shapes(after).First().Geometry).RadiusX > ((EllipseGeometry)shapes[0].Geometry).RadiusX);
                Assert.IsTrue(Groups(after).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value.OffsetY < before.OffsetY);
                Assert.IsTrue(Groups(after).Min(g => g.Opacity) < Groups(initial).Min(g => g.Opacity));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
                var preview = CottonCandyCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(3, Shapes(preview.Drawing).Select(s => s.Brush).OfType<RadialGradientBrush>()
                    .Select(b => b.GradientStops[1].Color).Distinct().Count());
            });
        }

        [TestMethod]
        public void EmissionIsInterpolatedThrottledBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new CottonCandyCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Update(new Point(200, 200), 0.02);
                Assert.AreEqual(11, effect.ParticleCount);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Transform != null && g.Transform.Value.OffsetX > 130 && g.Transform.Value.OffsetX < 180));
                effect.Update(new Point(10000, 200), 0.03);
                Assert.AreEqual(11 + CottonCandyCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 100; i++)
                {
                    effect.Update(new Point(i * 200, 200), 0.03);
                    Assert.IsTrue(effect.ParticleCount <= CottonCandyCursorEffect.MaxParticles);
                }
                Assert.AreEqual(64, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(19800, 200), 4);
                effect.Update(new Point(19800, 200), 0.03);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.03);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputIsIgnoredAndOpacityUpdatesLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new CottonCandyCursorEffect(settings);
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(200, 200), elapsed);
                effect.Update(new Point(double.NaN, 200), 0.016);
                effect.Update(new Point(200, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.016);
                var initial = Alpha(effect);
                Assert.IsTrue(initial > 0);
                settings.CottonCandyOpacity = 0;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.CottonCandyOpacity = 0.25;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < initial);
                Assert.AreEqual(1.0, settings.ColorfulMistOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsPuffs(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.CottonCandy);
                    var effect = new CottonCandyCursorEffect();
                    effect.Update(new Point(100, 100), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("cursorEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.ColorfulMist);
                    else if (action == "start") window.StartMouseEffect();
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.IsFalse(Shapes(Drawing(effect)).Any());
                    Assert.AreEqual(0, canvas.Children.Count);
                    Assert.IsNull(field.GetValue(window));
                }
                finally { window.Close(); }
            });
        }

        [TestMethod]
        public void DialogLoadsResetsAndDiscardsDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.CottonCandyOpacity = 0.6;
                config.MouseEffectSetting.CottonCandySizeScale = 2;
                var window = new MouseEffectSettingWindow(MouseEffectType.CottonCandy, config);
                try
                {
                    Assert.AreEqual("棉花糖粒子设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    var size = (Slider)window.FindName("slider_CottonCandySize");
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual(50.0, size.Minimum);
                    Assert.AreEqual(300.0, size.Maximum);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_CottonCandySize")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    foreach (var name in new[] { "panel_Following", "panel_Trail", "panel_Expansion", "panel_StardustSize", "panel_InkFluid", "panel_NeonRibbon" })
                        Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    size.Value = 300;
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.CottonCandyOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.CottonCandySizeScale);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyCottonCandyAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-CottonTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.ColorfulMistOpacity = 0.6;
                    settings.FollowingAnimalSizeScale = 1.5;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.CottonCandy;
                    var window = new MouseEffectSettingWindow(MouseEffectType.CottonCandy, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_CottonCandySize")).Value = 250;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    Assert.AreEqual(0.25, settings.CottonCandyOpacity);
                    Assert.AreEqual(2.5, settings.CottonCandySizeScale);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.CottonCandyOpacity);
                    Assert.AreEqual(2.5, restored.MouseEffectSetting.CottonCandySizeScale);
                    Assert.AreEqual(1.5, restored.MouseEffectSetting.FollowingAnimalSizeScale);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.ColorfulMistOpacity);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(19, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.CottonCandy, restored);
                    try
                    {
                        Assert.AreEqual(25.0, ((Slider)reopened.FindName("slider_Opacity")).Value);
                        Assert.AreEqual(250.0, ((Slider)reopened.FindName("slider_CottonCandySize")).Value);
                    }
                    finally { reopened.Close(); }
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5)]
        [DataRow(10.0, 3.0)]
        [DataRow(1.75, 1.75)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void SizeIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { CottonCandySizeScale = value }.CottonCandySizeScale);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.75)]
        [DataRow(3.0)]
        public void LiveSizeScalesAllLobesWithoutChangingCentersOrBaseRadii(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new CottonCandyCursorEffect(settings);
                effect.Update(new Point(100, 200), 0.016);
                var point = new Point(120, 200);
                effect.Update(point, 0.03);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                var before = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).ToArray();
                Assert.AreEqual(3, before.Length);
                var particles = (System.Collections.IEnumerable)typeof(CottonCandyCursorEffect)
                    .GetField("particles", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
                var radii = particles.Cast<object>().Select(p => (double)p.GetType().GetField("Radius")!.GetValue(p)!).ToArray();
                Assert.IsTrue(radii.All(r => r >= 9 && r < 13));
                settings.CottonCandySizeScale = scale;
                effect.Update(point, 0.000001);
                var after = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).ToArray();
                Assert.AreEqual(before.Length, after.Length);
                for (var i = 0; i < before.Length; i++)
                {
                    var original = before[i].Transform.Value;
                    var current = after[i].Transform.Value;
                    Assert.AreEqual(original.OffsetX, current.OffsetX, 0.001);
                    Assert.AreEqual(original.OffsetY, current.OffsetY, 0.001);
                    Assert.AreEqual(original.M11 * scale, current.M11, 0.001);
                    Assert.AreEqual(original.M12 * scale, current.M12, 0.001);
                    Assert.AreEqual(original.M21 * scale, current.M21, 0.001);
                    Assert.AreEqual(original.M22 * scale, current.M22, 0.001);
                    var initialShapes = Shapes(before[i]).ToArray();
                    var shapes = Shapes(after[i]).ToArray();
                    Assert.AreEqual(7, shapes.Length);
                    for (var j = 0; j < shapes.Length; j++)
                    {
                        Assert.AreSame(initialShapes[j].Brush, shapes[j].Brush);
                        var initial = (EllipseGeometry)initialShapes[j].Geometry;
                        var ellipse = (EllipseGeometry)shapes[j].Geometry;
                        Assert.AreEqual(initial.RadiusX, ellipse.RadiusX, 0.001);
                        Assert.AreEqual(initial.RadiusY, ellipse.RadiusY, 0.001);
                        Assert.AreEqual(initial.Center.X, ellipse.Center.X, 0.001);
                        Assert.AreEqual(initial.Center.Y, ellipse.Center.Y, 0.001);
                    }
                }
                CollectionAssert.AreEqual(radii, particles.Cast<object>().Select(p => (double)p.GetType().GetField("Radius")!.GetValue(p)!).ToArray());
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                settings.CottonCandySizeScale = 1;
                effect.Update(point, 0.000001);
                var restored = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).ToArray();
                for (var i = 0; i < before.Length; i++)
                    Assert.AreEqual(before[i].Transform.Value.M11, restored[i].Transform.Value.M11, 0.001);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.FollowingAnimal)]
        [DataRow(MouseEffectType.ColorfulMist)]
        public void OtherDialogsHideCottonCandySize(MouseEffectType type)
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(type, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_CottonCandySize")).Visibility); }
                finally { window.Close(); }
            });
        }

        private static DrawingGroup Drawing(CottonCandyCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var item in Groups(child)) yield return item;
            }
        }
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child)) yield return shape;
        }
        private static long Alpha(CottonCandyCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(400, 400, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[400 * 400 * 4];
            bitmap.CopyPixels(pixels, 400 * 4, 0);
            long sum = 0;
            for (var i = 3; i < pixels.Length; i += 4) sum += pixels[i];
            return sum;
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
