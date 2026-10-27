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
        private ParticleCursorEffect particleEffect;
        private GdiParticleCursorEffect gdiParticleEffect;
        private CursorTrailEffect trailEffect;
        private InkFluidCursorEffect inkFluidEffect;
        private NeonRibbonCursorEffect neonRibbonEffect;
        private WaterRippleCursorEffect waterRippleEffect;
        private StardustCursorEffect stardustEffect;
        private FallingPetalsCursorEffect fallingPetalsEffect;
        private ColorfulMistCursorEffect colorfulMistEffect;
        private FireworksTrailCursorEffect fireworksTrailEffect;
        private FallingLeavesCursorEffect fallingLeavesEffect;
        private IconRainCursorEffect iconRainEffect;
        private HaloRingCursorEffect haloRingEffect;
        private FollowingAnimalCursorEffect followingAnimalEffect;
        private CottonCandyCursorEffect cottonCandyEffect;
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

            if (currentEffectType == MouseEffectType.NeonRibbon)
            {
                if (neonRibbonEffect == null)
                {
                    neonRibbonEffect = new NeonRibbonCursorEffect(settings);
                    canvas.Children.Add(neonRibbonEffect);
                }
                neonRibbonEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.WaterRipple)
            {
                if (waterRippleEffect == null)
                {
                    waterRippleEffect = new WaterRippleCursorEffect(settings);
                    canvas.Children.Add(waterRippleEffect);
                }
                waterRippleEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.Stardust)
            {
                if (stardustEffect == null)
                {
                    stardustEffect = new StardustCursorEffect(settings);
                    canvas.Children.Add(stardustEffect);
                }
                stardustEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.FallingPetals)
            {
                if (fallingPetalsEffect == null)
                {
                    fallingPetalsEffect = new FallingPetalsCursorEffect(settings);
                    canvas.Children.Add(fallingPetalsEffect);
                }
                fallingPetalsEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.ColorfulMist)
            {
                if (colorfulMistEffect == null)
                {
                    colorfulMistEffect = new ColorfulMistCursorEffect(settings);
                    canvas.Children.Add(colorfulMistEffect);
                }
                colorfulMistEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.FireworksTrail)
            {
                if (fireworksTrailEffect == null)
                {
                    fireworksTrailEffect = new FireworksTrailCursorEffect(settings);
                    canvas.Children.Add(fireworksTrailEffect);
                }
                fireworksTrailEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.FallingLeaves)
            {
                if (fallingLeavesEffect == null)
                {
                    fallingLeavesEffect = new FallingLeavesCursorEffect(settings);
                    canvas.Children.Add(fallingLeavesEffect);
                }
                fallingLeavesEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.IconRain)
            {
                if (iconRainEffect == null)
                {
                    iconRainEffect = new IconRainCursorEffect(settings);
                    canvas.Children.Add(iconRainEffect);
                }
                iconRainEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.HaloRing)
            {
                if (haloRingEffect == null)
                {
                    haloRingEffect = new HaloRingCursorEffect(settings);
                    canvas.Children.Add(haloRingEffect);
                }
                haloRingEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.FollowingAnimal)
            {
                if (followingAnimalEffect == null)
                {
                    followingAnimalEffect = new FollowingAnimalCursorEffect(settings);
                    canvas.Children.Add(followingAnimalEffect);
                }
                followingAnimalEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.CottonCandy)
            {
                if (cottonCandyEffect == null)
                {
                    cottonCandyEffect = new CottonCandyCursorEffect(settings);
                    canvas.Children.Add(cottonCandyEffect);
                }
                cottonCandyEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.InkFluid)
            {
                if (inkFluidEffect == null)
                {
                    inkFluidEffect = new InkFluidCursorEffect(settings);
                    canvas.Children.Add(inkFluidEffect);
                }
                inkFluidEffect.Update(point, elapsed);
                return;
            }

            if (currentEffectType == MouseEffectType.CursorTrail)
            {
                if (trailEffect == null)
                {
                    trailEffect = new CursorTrailEffect(point, settings);
                    canvas.Children.Add(trailEffect);
                }
                else
                {
                    trailEffect.Update(point, elapsed);
                }
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

            if (currentEffectType == MouseEffectType.Fireworks || currentEffectType == MouseEffectType.WillowCatkins ||
                currentEffectType == MouseEffectType.Snowflakes || currentEffectType == MouseEffectType.FivePointedStars)
            {
                if (particleEffect == null)
                {
                    particleEffect = new ParticleCursorEffect(currentEffectType, settings);
                    canvas.Children.Add(particleEffect);
                }

                particleEffect.Update(point, elapsed);
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
            cottonCandyEffect?.Clear();
            cottonCandyEffect = null;
            followingAnimalEffect?.Clear();
            followingAnimalEffect = null;
            haloRingEffect?.Clear();
            haloRingEffect = null;
            iconRainEffect?.Clear();
            iconRainEffect = null;
            fallingLeavesEffect?.Clear();
            fallingLeavesEffect = null;
            fireworksTrailEffect?.Clear();
            fireworksTrailEffect = null;
            colorfulMistEffect?.Clear();
            colorfulMistEffect = null;
            fallingPetalsEffect?.Clear();
            fallingPetalsEffect = null;
            stardustEffect?.Clear();
            stardustEffect = null;
            waterRippleEffect?.Clear();
            waterRippleEffect = null;
            neonRibbonEffect?.Clear();
            neonRibbonEffect = null;
            followingPositionHistory.Clear();
            inkFluidEffect?.Clear();
            inkFluidEffect = null;
            trailEffect?.Clear();
            trailEffect = null;
            particleEffect?.Clear();
            particleEffect = null;
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
