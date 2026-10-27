using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    public class MouseEffectTests
    {
        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.WillowCatkins)]
        [DataRow(MouseEffectType.Snowflakes)]
        [DataRow(MouseEffectType.FivePointedStars)]
        public void ParticlesAnimateAndExpireWithoutMoreMouseMovement(MouseEffectType type)
        {
            RunSta(() =>
            {
                var effect = new ParticleCursorEffect(type);
                var cursor = new Point(100, 100);
                effect.Update(cursor, 1.0 / 60);
                Assert.IsTrue(effect.ParticleCount > 0);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
                var initialBounds = visual.Drawing.Bounds;
                effect.Update(cursor, 0.05);
                Assert.AreNotEqual(initialBounds, visual.Drawing.Bounds);

                effect.Update(cursor, 5);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(visual.Drawing == null || visual.Drawing.Bounds.IsEmpty);

                effect.Update(new Point(120, 100), 1.0 / 60);
                Assert.IsTrue(effect.ParticleCount > 0);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.WillowCatkins)]
        [DataRow(MouseEffectType.Snowflakes)]
        [DataRow(MouseEffectType.FivePointedStars)]
        public void ContinuousMovementKeepsParticleAndVisualCountsBounded(MouseEffectType type)
        {
            RunSta(() =>
            {
                var effect = new ParticleCursorEffect(type);
                for (var i = 0; i < 180; i++)
                {
                    effect.Update(new Point(i * 10, 100), 0.05);
                    Assert.IsTrue(effect.ParticleCount > 0);
                    Assert.IsTrue(effect.ParticleCount <= ParticleCursorEffect.MaxParticles);
                    Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                }

                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
                Assert.IsTrue(visual.Drawing == null || visual.Drawing.Bounds.IsEmpty);
                effect.Update(new Point(1790, 100), 0.01);
                Assert.IsTrue(effect.ParticleCount > 0);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.WillowCatkins)]
        [DataRow(MouseEffectType.Snowflakes)]
        [DataRow(MouseEffectType.FivePointedStars)]
        public void InvalidElapsedTimesDoNotEmitParticles(MouseEffectType type)
        {
            RunSta(() =>
            {
                var effect = new ParticleCursorEffect(type);
                foreach (var elapsed in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity })
                    effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ParticleCursorEffect(MouseEffectType.FollowingDot));
            });
        }

        [TestMethod]
        public void WindowSwitchingAndRepeatedStoppingClearPreviousVisuals()
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    var canvas = (Canvas)window.FindName("canvas");
                    window.StartMouseEffect();
                    window.StartMouseEffect();
                    Assert.AreEqual(0, canvas.Children.Count);

                    foreach (var type in new[] { MouseEffectType.Fireworks, MouseEffectType.WillowCatkins, MouseEffectType.Snowflakes, MouseEffectType.FivePointedStars, MouseEffectType.CursorTrail, MouseEffectType.FollowingDot })
                    {
                        canvas.Children.Add(new FollowingDotEffect(new Point(20, 20)));
                        window.UpdateMouseEffectType(type);
                        Assert.AreEqual(0, canvas.Children.Count);
                    }

                    window.UpdateMouseEffectType(MouseEffectType.CursorTrail);
                    Assert.IsInstanceOfType(window.GetMouseCursorEffect(new Point()), typeof(CursorTrailEffect));
                    window.UpdateMouseEffectType(MouseEffectType.FollowingDot);
                    Assert.IsInstanceOfType(window.GetMouseCursorEffect(new Point()), typeof(FollowingDotEffect));
                    Assert.ThrowsException<ArgumentOutOfRangeException>(() => window.UpdateMouseEffectType((MouseEffectType)99));

                    canvas.Children.Add(new ParticleCursorEffect(MouseEffectType.Fireworks));
                    window.StopMouseEffect();
                    window.StopMouseEffect();
                    Assert.AreEqual(0, canvas.Children.Count);
                    window.StartMouseEffect();
                }
                finally
                {
                    window.Close();
                }
                Assert.AreEqual(0, ((Canvas)window.FindName("canvas")).Children.Count);
                window.StartMouseEffect();
                window.StopMouseEffect();
            });
        }

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
                finally { System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                Assert.Fail(failure.ToString());
        }
    }
}
