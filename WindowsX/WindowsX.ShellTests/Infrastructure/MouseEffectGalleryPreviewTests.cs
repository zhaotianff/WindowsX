using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    public class MouseEffectGalleryPreviewTests
    {
        [TestMethod]
        public void FollowingPreviewUsesDefaultGradientOnFixedCanvas()
        {
            RunSta(() =>
            {
                var image = FollowingDotEffect.PreviewImage;
                Assert.IsTrue(image.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), image.Drawing.Bounds);
                var shapes = Shapes(image.Drawing).ToArray();
                Assert.AreEqual(2, shapes.Length);
                var background = (RectangleGeometry)shapes[0].Geometry;
                Assert.AreEqual(10.0, background.RadiusX);
                var glow = (EllipseGeometry)shapes[1].Geometry;
                Assert.AreEqual(new Point(130, 85), glow.Center);
                Assert.AreEqual(MouseEffectSetting.DefaultFollowingSize / 2, glow.RadiusX);
                var expected = FollowingDotEffect.CreateGradientBrush(new MouseEffectSetting());
                var actual = (RadialGradientBrush)shapes[1].Brush;
                CollectionAssert.AreEqual(expected.GradientStops.Select(s => s.Color).ToArray(), actual.GradientStops.Select(s => s.Color).ToArray());
            });
        }

        [TestMethod]
        public void TrailGalleryPreviewPreservesSettingsPreviewAndCurves()
        {
            RunSta(() =>
            {
                var image = CursorTrailEffect.PreviewImage;
                Assert.IsTrue(image.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), image.Drawing.Bounds);
                var shapes = Shapes(image.Drawing).ToArray();
                Assert.IsInstanceOfType(shapes[0].Geometry, typeof(RectangleGeometry));
                var expected = Shapes(CursorTrailEffect.CreatePreviewImage(new MouseEffectSetting()).Drawing).ToArray();
                Assert.AreEqual(expected.Length + 1, shapes.Length);
                for (var i = 0; i < expected.Length; i++)
                {
                    var actual = shapes[i + 1];
                    Assert.IsNull(actual.Brush);
                    Assert.AreEqual(expected[i].Geometry.ToString(), actual.Geometry.ToString());
                    Assert.AreEqual(expected[i].Pen.Thickness, actual.Pen.Thickness);
                    var expectedBrush = (LinearGradientBrush)expected[i].Pen.Brush;
                    var actualBrush = (LinearGradientBrush)actual.Pen.Brush;
                    CollectionAssert.AreEqual(expectedBrush.GradientStops.Select(s => s.Color).ToArray(), actualBrush.GradientStops.Select(s => s.Color).ToArray());
                }
            });
        }

        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child)) yield return shape;
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
