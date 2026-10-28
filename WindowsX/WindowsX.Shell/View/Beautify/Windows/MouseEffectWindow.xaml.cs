using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Config.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;

namespace WindowsX.Shell.Windows
{
    /// <summary>
    /// Interaction logic for MouseEffectWindow.xaml
    /// </summary>
    public partial class MouseEffectWindow : Window
    {
        private readonly MouseEffectSetting settings;
        private readonly FollowingPositionHistory followingPositionHistory = new FollowingPositionHistory();
        private bool isRunning = false;
        private bool isClosed = false;
        private int fixedCount = 1;
        private MouseEffectType currentEffectType = MouseEffectType.FollowingDot;
        private GdiParticleCursorEffect gdiParticleEffect;
        private ICursorEffect cursorEffect;
        private TimeSpan? lastRenderingTime;
        private double legacyElapsed;

        public MouseEffectWindow() : this(null)
        {
        }

        public MouseEffectWindow(MouseEffectSetting settings)
        {
            this.settings = settings ?? new MouseEffectSetting();
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var extendedStyle = PInvoke.User32.GetWindowLong(handle, PInvoke.User32.GWL_EXSTYLE);
            PInvoke.User32.SetWindowLong(handle, PInvoke.User32.GWL_EXSTYLE, extendedStyle | PInvoke.User32.WS_EX_TRANSPARENT);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartMouseEffect();
        }

        public void StartMouseEffect()
        {
            if (isRunning || isClosed)
                return;

            isRunning = true;
            ClearEffects();
            CompositionTarget.Rendering += RenderMouseEffect;
        }

        private void CreateEffects()
        {
            if (cursorEffect != null)
                return;

            switch(currentEffectType)
            {
                case MouseEffectType.NeonRibbon:
                    cursorEffect = new NeonRibbonCursorEffect(settings);
                    break;
                case MouseEffectType.WaterRipple:
                    cursorEffect = new WaterRippleCursorEffect(settings);
                    break;
                case MouseEffectType.Stardust:
                    cursorEffect = new StardustCursorEffect(settings);
                    break;
                case MouseEffectType.FallingPetals:
                    cursorEffect = new FallingPetalsCursorEffect(settings);
                    break;
                case MouseEffectType.ColorfulMist:
                    cursorEffect = new ColorfulMistCursorEffect(settings);
                    break;
                case MouseEffectType.FireworksTrail:
                    cursorEffect = new FireworksTrailCursorEffect(settings);
                    break;
                case MouseEffectType.FallingLeaves:
                    cursorEffect = new FallingLeavesCursorEffect(settings);
                    break;
                case MouseEffectType.IconRain:
                    cursorEffect = new IconRainCursorEffect(settings);
                    break;
                case MouseEffectType.HaloRing:
                    cursorEffect = new HaloRingCursorEffect(settings);
                    break;
                case MouseEffectType.FollowingAnimal:
                    cursorEffect = new FollowingAnimalCursorEffect(settings);
                    break;
                case MouseEffectType.CottonCandy:
                    cursorEffect = new CottonCandyCursorEffect(settings);
                    break;
                case MouseEffectType.InkFluid:
                    cursorEffect = new InkFluidCursorEffect(settings);
                    break;
                case MouseEffectType.CursorTrail:
                    var screenPoint = new PInvoke.POINT();
                    PInvoke.User32.GetCursorPos(ref screenPoint);
                    var point = canvas.PointFromScreen(new Point(screenPoint.x, screenPoint.y));
                    cursorEffect = new CursorTrailEffect(point,settings);
                    break;
                case MouseEffectType.Fireworks:
                case MouseEffectType.WillowCatkins:
                case MouseEffectType.Snowflakes:
                case MouseEffectType.FivePointedStars:
                    cursorEffect = new ParticleCursorEffect(currentEffectType, settings);
                    break;
            }

            if(cursorEffect !=  null)
            {
                canvas.Children.Add((UIElement)cursorEffect);
            }
        }

        private void RenderMouseEffect(object sender, EventArgs e)
        {
            if (!isRunning || !IsVisible)
                return;

            var renderingTime = ((RenderingEventArgs)e).RenderingTime;
            if (lastRenderingTime == renderingTime)
                return;

            var elapsed = lastRenderingTime.HasValue ? (renderingTime - lastRenderingTime.Value).TotalSeconds : 1.0 / 60;
            lastRenderingTime = renderingTime;
            var screenPoint = new PInvoke.POINT();
            if (PInvoke.User32.GetCursorPos(ref screenPoint) == 0)
                return;

            var point = canvas.PointFromScreen(new Point(screenPoint.x, screenPoint.y));
            if (currentEffectType == MouseEffectType.FollowingDot)
                point = followingPositionHistory.Update(point, renderingTime, settings.FollowingDelayMilliseconds);

            CreateEffects();

            switch(currentEffectType)
            {
                case MouseEffectType.NeonRibbon:
                case MouseEffectType.WaterRipple:
                case MouseEffectType.Stardust:
                case MouseEffectType.FallingPetals:
                case MouseEffectType.ColorfulMist:
                case MouseEffectType.FireworksTrail:
                case MouseEffectType.FallingLeaves:
                case MouseEffectType.IconRain:
                case MouseEffectType.HaloRing:
                case MouseEffectType.FollowingAnimal:
                case MouseEffectType.CottonCandy:
                case MouseEffectType.InkFluid:
                case MouseEffectType.CursorTrail:
                case MouseEffectType.Fireworks:
                case MouseEffectType.WillowCatkins:
                case MouseEffectType.Snowflakes:
                case MouseEffectType.FivePointedStars:
                    cursorEffect?.Update(point, elapsed);
                    return;
            }

            if (currentEffectType == MouseEffectType.FireworksGdiPlus || currentEffectType == MouseEffectType.WillowCatkinsGdiPlus)
            {
                if (gdiParticleEffect == null)
                {
                    gdiParticleEffect = new GdiParticleCursorEffect(currentEffectType, settings);
                    canvas.Children.Add(gdiParticleEffect);
                }

                gdiParticleEffect.Update(point, elapsed, new Size(canvas.ActualWidth, canvas.ActualHeight), VisualTreeHelper.GetDpi(canvas));
                return;
            }

            legacyElapsed += elapsed;
            if (legacyElapsed < 0.01)
                return;

            legacyElapsed = 0;
            var effect = GetMouseCursorEffect(point);
            while (canvas.Children.Count >= fixedCount)
                canvas.Children.RemoveAt(0);

            canvas.Children.Add(effect);
        }

        public void StopMouseEffect()
        {
            isRunning = false;
            CompositionTarget.Rendering -= RenderMouseEffect;
            ClearEffects();
        }

        protected override void OnClosed(EventArgs e)
        {
            isClosed = true;
            StopMouseEffect();
            base.OnClosed(e);
        }

        private void ClearEffects()
        {
            cursorEffect?.Clear();
            cursorEffect = null;
            followingPositionHistory.Clear();
            gdiParticleEffect?.Dispose();
            gdiParticleEffect = null;
            canvas.Children.Clear();
            lastRenderingTime = null;
            legacyElapsed = 0;
        }

        public void UpdateMouseEffectType(MouseEffectType mouseEffectType)
        {
            if (!Enum.IsDefined(typeof(MouseEffectType), mouseEffectType))
                throw new ArgumentOutOfRangeException(nameof(mouseEffectType));

            if (currentEffectType == mouseEffectType)
                return;

            ClearEffects();
            currentEffectType = mouseEffectType;
        }

        public MouseCursorEffectAbstract GetMouseCursorEffect(Point point)
        {
            switch(currentEffectType)
            {
                case MouseEffectType.FollowingDot:
                    fixedCount = 1;
                    return new FollowingDotEffect(point, settings);
                case MouseEffectType.CursorTrail:
                    fixedCount = 1;
                    return new CursorTrailEffect(point, settings);
                default:
                    fixedCount = 1;
                    return new FollowingDotEffect(point, settings);
            }
        }
    }
}
