using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsX.Config.Helper;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;

namespace WindowsX.Shell.Windows
{
    public partial class MouseEffectSettingWindow : Window
    {
        private readonly MainConfig mainConfig;
        private readonly MouseEffectType effectType;
        private readonly bool fireworks;
        private readonly bool following;
        private readonly bool trail;
        private readonly bool catkins;
        private bool isInitialized;

        public MouseEffectSettingWindow(MouseEffectType effectType, MainConfig mainConfig)
        {
            if (!Enum.IsDefined(typeof(MouseEffectType), effectType))
                throw new ArgumentOutOfRangeException(nameof(effectType));

            this.mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));
            this.effectType = effectType;
            fireworks = effectType == MouseEffectType.Fireworks || effectType == MouseEffectType.FireworksGdiPlus;
            following = effectType == MouseEffectType.FollowingDot;
            trail = effectType == MouseEffectType.CursorTrail;
            catkins = effectType == MouseEffectType.WillowCatkins || effectType == MouseEffectType.WillowCatkinsGdiPlus;
            InitializeComponent();
            Title = effectType switch
            {
                MouseEffectType.Snowflakes => "雪花设置",
                MouseEffectType.FivePointedStars => "五角星设置",
                MouseEffectType.InkFluid => "墨水流体设置",
                MouseEffectType.NeonRibbon => "霓虹缎带设置",
                MouseEffectType.WaterRipple => "水波涟漪设置",
                MouseEffectType.Stardust => "星尘粒子设置",
                MouseEffectType.FallingPetals => "花瓣飘落设置",
                MouseEffectType.ColorfulMist => "彩色雾设置",
                MouseEffectType.FireworksTrail => "烟花尾迹设置",
                MouseEffectType.FallingLeaves => "落叶设置",
                MouseEffectType.IconRain => "图标雨设置",
                MouseEffectType.HaloRing => "光晕圈设置",
                MouseEffectType.FollowingAnimal => "跟随小动物设置",
                MouseEffectType.CottonCandy => "棉花糖粒子设置",
                MouseEffectType.Fireflies => "萤火虫设置",
                MouseEffectType.PixelBlocks => "像素方块设置",
                MouseEffectType.Butterflies => "蝴蝶跟随设置",
                MouseEffectType.Cloud => "云朵设置",
                MouseEffectType.MusicNotes => "音符轨迹设置",
                MouseEffectType.BinaryTrail => "二进制字符光轨设置",
                MouseEffectType.OrbitParticles => "环绕粒子设置",
                MouseEffectType.StarlightArrow => "星光箭头设置",
                MouseEffectType.SwordAura => "剑意设置",
                MouseEffectType.Drizzle => "斜风细雨设置",
                MouseEffectType.Flame => "火焰设置",
                _ => trail ? "轨迹设置" : following ? "跟随设置" : fireworks ? "烟花设置" : "春日柳絮设置"
            };
            var settings = mainConfig.MouseEffectSetting;
            slider_Opacity.Value = (effectType switch
            {
                MouseEffectType.Snowflakes => settings.SnowflakesOpacity,
                MouseEffectType.FivePointedStars => settings.FivePointedStarsOpacity,
                MouseEffectType.InkFluid => settings.InkFluidOpacity,
                MouseEffectType.NeonRibbon => settings.NeonRibbonOpacity,
                MouseEffectType.WaterRipple => settings.WaterRippleOpacity,
                MouseEffectType.Stardust => settings.StardustOpacity,
                MouseEffectType.FallingPetals => settings.FallingPetalsOpacity,
                MouseEffectType.ColorfulMist => settings.ColorfulMistOpacity,
                MouseEffectType.FireworksTrail => settings.FireworksTrailOpacity,
                MouseEffectType.FallingLeaves => settings.FallingLeavesOpacity,
                MouseEffectType.IconRain => settings.IconRainOpacity,
                MouseEffectType.HaloRing => settings.HaloRingOpacity,
                MouseEffectType.FollowingAnimal => settings.FollowingAnimalOpacity,
                MouseEffectType.CottonCandy => settings.CottonCandyOpacity,
                MouseEffectType.Fireflies => settings.FirefliesOpacity,
                MouseEffectType.PixelBlocks => settings.PixelBlocksOpacity,
                MouseEffectType.Butterflies => settings.ButterfliesOpacity,
                MouseEffectType.Cloud => settings.CloudOpacity,
                MouseEffectType.MusicNotes => settings.MusicNotesOpacity,
                MouseEffectType.BinaryTrail => settings.BinaryTrailOpacity,
                MouseEffectType.OrbitParticles => settings.OrbitParticlesOpacity,
                MouseEffectType.StarlightArrow => settings.StarlightArrowOpacity,
                MouseEffectType.SwordAura => settings.SwordAuraOpacity,
                MouseEffectType.Drizzle => settings.DrizzleOpacity,
                MouseEffectType.Flame => settings.FlameOpacity,
                _ => fireworks ? settings.FireworksOpacity : settings.WillowCatkinsOpacity
            }) * 100;
            slider_Expansion.Value = settings.FireworksExpansion * 100;
            panel_Flame.Visibility = effectType == MouseEffectType.Flame ? Visibility.Visible : Visibility.Collapsed;
            slider_FlameSize.Value = settings.FlameSizeScale * 100;
            slider_FlameSpeed.Value = settings.FlameSpeed * 100;
            slider_FlameRadius.Value = settings.FlameRadius;
            panel_SwordAura.Visibility = effectType == MouseEffectType.SwordAura ? Visibility.Visible : Visibility.Collapsed;
            slider_SwordAuraSize.Value = settings.SwordAuraSizeScale * 100;
            text_SwordAuraColor1.Text = settings.SwordAuraColor1;
            text_SwordAuraColor2.Text = settings.SwordAuraColor2;
            text_SwordAuraColor3.Text = settings.SwordAuraColor3;
            panel_StarlightArrowColor.Visibility = effectType == MouseEffectType.StarlightArrow ? Visibility.Visible : Visibility.Collapsed;
            text_StarlightArrowColor.Text = settings.StarlightArrowColor;
            panel_OrbitParticles.Visibility = effectType == MouseEffectType.OrbitParticles ? Visibility.Visible : Visibility.Collapsed;
            slider_OrbitParticlesLayers.Value = settings.OrbitParticlesLayers;
            slider_OrbitParticlesRadius.Value = settings.OrbitParticlesRadius;
            slider_OrbitParticlesSpeed.Value = settings.OrbitParticlesSpeed * 100;
            text_OrbitParticlesColor1.Text = settings.OrbitParticlesColor1;
            text_OrbitParticlesColor2.Text = settings.OrbitParticlesColor2;
            text_OrbitParticlesColor3.Text = settings.OrbitParticlesColor3;
            panel_BinaryTrail.Visibility = effectType == MouseEffectType.BinaryTrail ? Visibility.Visible : Visibility.Collapsed;
            check_BinaryTrailDefaultColors.IsChecked = settings.BinaryTrailUseDefaultColors;
            text_BinaryTrailColor.Text = settings.BinaryTrailColor;
            slider_BinaryTrailSize.Value = settings.BinaryTrailSizeScale * 100;
            combo_BinaryTrailFont.SelectedValue = settings.BinaryTrailFontFamily;
            panel_Cloud.Visibility = effectType == MouseEffectType.Cloud ? Visibility.Visible : Visibility.Collapsed;
            slider_CloudSize.Value = settings.CloudSizeScale * 100;
            combo_CloudPosition.SelectedValue = settings.CloudPosition;
            slider_CloudHorizontalOffset.Value = settings.CloudHorizontalOffset;
            slider_CloudVerticalOffset.Value = settings.CloudVerticalOffset;
            slider_PixelBlocksSize.Value = settings.PixelBlocksSizeScale * 100;
            panel_PixelBlocksSize.Visibility = effectType == MouseEffectType.PixelBlocks ? Visibility.Visible : Visibility.Collapsed;
            slider_FirefliesFollowSpeed.Value = settings.FirefliesFollowSpeed * 100;
            panel_FirefliesFollowSpeed.Visibility = effectType == MouseEffectType.Fireflies ? Visibility.Visible : Visibility.Collapsed;
            slider_CottonCandySize.Value = settings.CottonCandySizeScale * 100;
            slider_ColorfulMistSize.Value = settings.ColorfulMistSizeScale * 100;
            panel_ColorfulMistSize.Visibility = effectType == MouseEffectType.ColorfulMist ? Visibility.Visible : Visibility.Collapsed;
            panel_CottonCandySize.Visibility = effectType == MouseEffectType.CottonCandy ? Visibility.Visible : Visibility.Collapsed;
            slider_FollowingAnimalSize.Value = settings.FollowingAnimalSizeScale * 100;
            panel_FollowingAnimalSize.Visibility = effectType == MouseEffectType.FollowingAnimal ? Visibility.Visible : Visibility.Collapsed;
            slider_IconRainSize.Value = settings.IconRainSizeScale * 100;
            panel_IconRainSize.Visibility = effectType == MouseEffectType.IconRain ? Visibility.Visible : Visibility.Collapsed;
            slider_FallingLeavesSize.Value = settings.FallingLeavesSizeScale * 100;
            panel_FallingLeavesSize.Visibility = effectType == MouseEffectType.FallingLeaves ? Visibility.Visible : Visibility.Collapsed;
            slider_FallingPetalsSize.Value = settings.FallingPetalsSizeScale * 100;
            combo_FallingPetalsColor.SelectedValue = settings.FallingPetalsColor;
            panel_FallingPetals.Visibility = effectType == MouseEffectType.FallingPetals ? Visibility.Visible : Visibility.Collapsed;
            slider_WaterRippleSize.Value = settings.WaterRippleSizeScale * 100;
            slider_WaterRippleFrequency.Value = settings.WaterRippleFrequency;
            text_WaterRippleColor.Text = settings.WaterRippleColor;
            panel_WaterRipple.Visibility = effectType == MouseEffectType.WaterRipple ? Visibility.Visible : Visibility.Collapsed;
            text_CatkinsColor.Text = settings.WillowCatkinsColor;
            slider_CatkinsSize.Value = settings.WillowCatkinsSizeScale * 100;
            panel_Catkins.Visibility = catkins ? Visibility.Visible : Visibility.Collapsed;
            slider_SnowflakesSize.Value = settings.SnowflakesSizeScale * 100;
            panel_SnowflakesSize.Visibility = effectType == MouseEffectType.Snowflakes ? Visibility.Visible : Visibility.Collapsed;
            slider_FivePointedStarsSize.Value = settings.FivePointedStarsSizeScale * 100;
            text_FivePointedStarsColor.Text = settings.FivePointedStarsColor;
            check_FivePointedStarsRandomColors.IsChecked = settings.FivePointedStarsUseRandomColors;
            panel_FivePointedStars.Visibility = effectType == MouseEffectType.FivePointedStars ? Visibility.Visible : Visibility.Collapsed;
            slider_StardustSize.Value = settings.StardustSizeScale * 100;
            panel_StardustSize.Visibility = effectType == MouseEffectType.Stardust ? Visibility.Visible : Visibility.Collapsed;
            slider_HaloRingSize.Value = settings.HaloRingSizeScale * 100;
            slider_HaloRingBreathingFrequency.Value = settings.HaloRingBreathingFrequency;
            text_HaloRingColor1.Text = settings.HaloRingColor1;
            text_HaloRingColor2.Text = settings.HaloRingColor2;
            text_HaloRingColor3.Text = settings.HaloRingColor3;
            panel_HaloRing.Visibility = effectType == MouseEffectType.HaloRing ? Visibility.Visible : Visibility.Collapsed;
            panel_Expansion.Visibility = fireworks ? Visibility.Visible : Visibility.Collapsed;
            panel_Following.Visibility = following ? Visibility.Visible : Visibility.Collapsed;
            panel_Trail.Visibility = trail ? Visibility.Visible : Visibility.Collapsed;
            panel_InkFluid.Visibility = effectType == MouseEffectType.InkFluid ? Visibility.Visible : Visibility.Collapsed;
            panel_NeonRibbon.Visibility = effectType == MouseEffectType.NeonRibbon ? Visibility.Visible : Visibility.Collapsed;
            panel_ParticleOpacity.Visibility = following || trail ? Visibility.Collapsed : Visibility.Visible;
            text_SharedSettings.Visibility = fireworks || effectType == MouseEffectType.WillowCatkins || effectType == MouseEffectType.WillowCatkinsGdiPlus
                ? Visibility.Visible : Visibility.Collapsed;
            text_FollowingColor.Text = settings.FollowingColor;
            slider_FollowingCenterOpacity.Value = settings.FollowingCenterOpacity * 100;
            slider_FollowingEdgeOpacity.Value = settings.FollowingEdgeOpacity * 100;
            slider_FollowingSize.Value = settings.FollowingSize;
            slider_FollowingDelay.Value = settings.FollowingDelayMilliseconds;
            text_TrailColor.Text = settings.TrailColor;
            slider_TrailHeadOpacity.Value = settings.TrailHeadOpacity * 100;
            slider_TrailTailOpacity.Value = settings.TrailTailOpacity * 100;
            slider_TrailSize.Value = settings.TrailSize;
            text_InkFluidColor1.Text = settings.InkFluidColor1;
            text_InkFluidColor2.Text = settings.InkFluidColor2;
            text_InkFluidColor3.Text = settings.InkFluidColor3;
            text_NeonRibbonColor1.Text = settings.NeonRibbonColor1;
            text_NeonRibbonColor2.Text = settings.NeonRibbonColor2;
            text_NeonRibbonColor3.Text = settings.NeonRibbonColor3;
            isInitialized = true;
            UpdateSwordAuraColors();
            UpdateStarlightArrowColor();
            UpdateOrbitParticlesColors();
            UpdateBinaryTrailColor();
            UpdateWaterRipplePreview();
            UpdateCatkinsPreview();
            UpdateFollowingPreview();
            UpdateTrailPreview();
            UpdateInkFluidPreview();
            UpdateNeonRibbonPreview();
            UpdateHaloRingPreview();
            UpdateFivePointedStarsPreview();
        }

        private bool HasValidSwordAuraColors()
        {
            return MouseEffectSetting.IsValidColor(text_SwordAuraColor1.Text)
                && MouseEffectSetting.IsValidColor(text_SwordAuraColor2.Text)
                && MouseEffectSetting.IsValidColor(text_SwordAuraColor3.Text);
        }

        private void SwordAuraColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSwordAuraColors();
        }

        private void UpdateSwordAuraColors()
        {
            if (!isInitialized || effectType != MouseEffectType.SwordAura) return;
            var valid = HasValidSwordAuraColors();
            text_SwordAuraColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_SwordAuraColor1, text_SwordAuraColor1.Text);
            UpdateColorSwatch(preview_SwordAuraColor2, text_SwordAuraColor2.Text);
            UpdateColorSwatch(preview_SwordAuraColor3, text_SwordAuraColor3.Text);
        }

        private void StarlightArrowColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateStarlightArrowColor();
        }

        private void UpdateStarlightArrowColor()
        {
            if (!isInitialized || effectType != MouseEffectType.StarlightArrow) return;
            var valid = MouseEffectSetting.IsValidColor(text_StarlightArrowColor.Text);
            text_StarlightArrowColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_StarlightArrowColor, text_StarlightArrowColor.Text);
        }

        private bool HasValidOrbitParticlesColors()
        {
            return MouseEffectSetting.IsValidColor(text_OrbitParticlesColor1.Text)
                && MouseEffectSetting.IsValidColor(text_OrbitParticlesColor2.Text)
                && MouseEffectSetting.IsValidColor(text_OrbitParticlesColor3.Text);
        }

        private void OrbitParticlesColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateOrbitParticlesColors();
        }

        private void UpdateOrbitParticlesColors()
        {
            if (!isInitialized || effectType != MouseEffectType.OrbitParticles) return;
            var valid = HasValidOrbitParticlesColors();
            text_OrbitParticlesColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_OrbitParticlesColor1, text_OrbitParticlesColor1.Text);
            UpdateColorSwatch(preview_OrbitParticlesColor2, text_OrbitParticlesColor2.Text);
            UpdateColorSwatch(preview_OrbitParticlesColor3, text_OrbitParticlesColor3.Text);
        }

        private void BinaryTrailColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBinaryTrailColor();
        }

        private void BinaryTrailColorMode_Changed(object sender, RoutedEventArgs e)
        {
            UpdateBinaryTrailColor();
        }

        private void UpdateBinaryTrailColor()
        {
            if (!isInitialized || effectType != MouseEffectType.BinaryTrail) return;
            var useDefault = check_BinaryTrailDefaultColors.IsChecked == true;
            var valid = useDefault || MouseEffectSetting.IsValidColor(text_BinaryTrailColor.Text);
            text_BinaryTrailColor.IsEnabled = !useDefault;
            preview_BinaryTrailColor.Opacity = useDefault ? 0.4 : 1;
            text_BinaryTrailColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_BinaryTrailColor, text_BinaryTrailColor.Text);
        }

        private void WaterRippleColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWaterRipplePreview();
        }

        private void UpdateWaterRipplePreview()
        {
            if (!isInitialized || effectType != MouseEffectType.WaterRipple)
                return;

            var valid = MouseEffectSetting.IsValidColor(text_WaterRippleColor.Text);
            text_WaterRippleColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_WaterRippleColor, text_WaterRippleColor.Text);
        }

        private void FivePointedStarsColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFivePointedStarsPreview();
        }

        private void FivePointedStarsColorMode_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFivePointedStarsPreview();
        }

        private void UpdateFivePointedStarsPreview()
        {
            if (!isInitialized || effectType != MouseEffectType.FivePointedStars)
                return;

            var randomColors = check_FivePointedStarsRandomColors.IsChecked == true;
            var valid = randomColors || MouseEffectSetting.IsValidColor(text_FivePointedStarsColor.Text);
            text_FivePointedStarsColor.IsEnabled = !randomColors;
            preview_FivePointedStarsColor.Opacity = randomColors ? 0.4 : 1;
            text_FivePointedStarsColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_FivePointedStarsColor, text_FivePointedStarsColor.Text);
        }

        private void HaloRingColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateHaloRingPreview();
        }

        private bool HasValidHaloRingColors()
        {
            return MouseEffectSetting.IsValidColor(text_HaloRingColor1.Text)
                && MouseEffectSetting.IsValidColor(text_HaloRingColor2.Text)
                && MouseEffectSetting.IsValidColor(text_HaloRingColor3.Text);
        }

        private void UpdateHaloRingPreview()
        {
            if (!isInitialized || effectType != MouseEffectType.HaloRing)
                return;

            var valid = HasValidHaloRingColors();
            text_HaloRingColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_HaloRingColor1, text_HaloRingColor1.Text);
            UpdateColorSwatch(preview_HaloRingColor2, text_HaloRingColor2.Text);
            UpdateColorSwatch(preview_HaloRingColor3, text_HaloRingColor3.Text);
        }

        private void CatkinsColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCatkinsPreview();
        }

        private void UpdateCatkinsPreview()
        {
            if (!isInitialized || !catkins)
                return;

            var valid = MouseEffectSetting.IsValidColor(text_CatkinsColor.Text);
            text_CatkinsColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_CatkinsColor, text_CatkinsColor.Text);
        }

        private void NeonRibbonColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateNeonRibbonPreview();
        }

        private void NeonRibbonOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateNeonRibbonPreview();
        }

        private bool HasValidNeonRibbonColors()
        {
            return MouseEffectSetting.IsValidColor(text_NeonRibbonColor1.Text)
                && MouseEffectSetting.IsValidColor(text_NeonRibbonColor2.Text)
                && MouseEffectSetting.IsValidColor(text_NeonRibbonColor3.Text);
        }

        private void UpdateNeonRibbonPreview()
        {
            if (!isInitialized || effectType != MouseEffectType.NeonRibbon)
                return;

            var valid = HasValidNeonRibbonColors();
            text_NeonRibbonColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            image_NeonRibbonPreview.Source = valid ? NeonRibbonCursorEffect.CreatePreviewImage(new MouseEffectSetting
            {
                NeonRibbonColor1 = text_NeonRibbonColor1.Text,
                NeonRibbonColor2 = text_NeonRibbonColor2.Text,
                NeonRibbonColor3 = text_NeonRibbonColor3.Text,
                NeonRibbonOpacity = slider_Opacity.Value / 100
            }) : null;
        }

        private void InkFluidColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateInkFluidPreview();
        }

        private bool HasValidInkFluidColors()
        {
            return MouseEffectSetting.IsValidColor(text_InkFluidColor1.Text)
                && MouseEffectSetting.IsValidColor(text_InkFluidColor2.Text)
                && MouseEffectSetting.IsValidColor(text_InkFluidColor3.Text);
        }

        private void UpdateInkFluidPreview()
        {
            if (!isInitialized || effectType != MouseEffectType.InkFluid)
                return;

            var valid = HasValidInkFluidColors();
            text_InkFluidColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            UpdateColorSwatch(preview_InkFluidColor1, text_InkFluidColor1.Text);
            UpdateColorSwatch(preview_InkFluidColor2, text_InkFluidColor2.Text);
            UpdateColorSwatch(preview_InkFluidColor3, text_InkFluidColor3.Text);
        }

        private static void UpdateColorSwatch(Border preview, string color)
        {
            if (!MouseEffectSetting.IsValidColor(color))
            {
                preview.Background = null;
                return;
            }

            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color.Trim()));
            brush.Freeze();
            preview.Background = brush;
        }

        private void TrailPresetColor_Click(object sender, RoutedEventArgs e)
        {
            text_TrailColor.Text = (string)((Button)sender).Tag;
        }

        private void TrailColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTrailPreview();
        }

        private void TrailSetting_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateTrailPreview();
        }

        private void UpdateTrailPreview()
        {
            if (!isInitialized || !trail)
                return;

            var valid = MouseEffectSetting.IsValidColor(text_TrailColor.Text);
            text_TrailColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            image_TrailPreview.Source = valid ? CursorTrailEffect.CreatePreviewImage(new MouseEffectSetting
            {
                TrailColor = text_TrailColor.Text,
                TrailHeadOpacity = slider_TrailHeadOpacity.Value / 100,
                TrailTailOpacity = slider_TrailTailOpacity.Value / 100,
                TrailSize = slider_TrailSize.Value
            }) : null;
        }

        private void PresetColor_Click(object sender, RoutedEventArgs e)
        {
            text_FollowingColor.Text = (string)((Button)sender).Tag;
        }

        private void FollowingColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFollowingPreview();
        }

        private void FollowingOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFollowingPreview();
        }

        private void UpdateFollowingPreview()
        {
            if (!isInitialized || !following)
                return;

            var valid = MouseEffectSetting.IsValidFollowingColor(text_FollowingColor.Text);
            text_ColorError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
            btn_Save.IsEnabled = valid;
            ellipse_FollowingPreview.Fill = valid ? FollowingDotEffect.CreateGradientBrush(new MouseEffectSetting
            {
                FollowingColor = text_FollowingColor.Text,
                FollowingCenterOpacity = slider_FollowingCenterOpacity.Value / 100,
                FollowingEdgeOpacity = slider_FollowingEdgeOpacity.Value / 100
            }) : null;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (trail)
            {
                text_TrailColor.Text = MouseEffectSetting.DefaultTrailColor;
                slider_TrailHeadOpacity.Value = MouseEffectSetting.DefaultTrailHeadOpacity * 100;
                slider_TrailTailOpacity.Value = MouseEffectSetting.DefaultTrailTailOpacity * 100;
                slider_TrailSize.Value = MouseEffectSetting.DefaultTrailSize;
                return;
            }

            if (following)
            {
                text_FollowingColor.Text = MouseEffectSetting.DefaultFollowingColor;
                slider_FollowingCenterOpacity.Value = MouseEffectSetting.DefaultFollowingCenterOpacity * 100;
                slider_FollowingEdgeOpacity.Value = MouseEffectSetting.DefaultFollowingEdgeOpacity * 100;
                slider_FollowingSize.Value = MouseEffectSetting.DefaultFollowingSize;
                slider_FollowingDelay.Value = MouseEffectSetting.DefaultFollowingDelayMilliseconds;
                return;
            }

            slider_Opacity.Value = 100;
            slider_Expansion.Value = 100;
            if (effectType == MouseEffectType.Flame)
            {
                slider_FlameSize.Value = MouseEffectSetting.DefaultFlameSizeScale * 100;
                slider_FlameSpeed.Value = MouseEffectSetting.DefaultFlameSpeed * 100;
                slider_FlameRadius.Value = MouseEffectSetting.DefaultFlameRadius;
            }
            if (effectType == MouseEffectType.SwordAura)
            {
                slider_SwordAuraSize.Value = MouseEffectSetting.DefaultSwordAuraSizeScale * 100;
                text_SwordAuraColor1.Text = MouseEffectSetting.DefaultSwordAuraColor1;
                text_SwordAuraColor2.Text = MouseEffectSetting.DefaultSwordAuraColor2;
                text_SwordAuraColor3.Text = MouseEffectSetting.DefaultSwordAuraColor3;
            }
            if (effectType == MouseEffectType.ColorfulMist)
                slider_ColorfulMistSize.Value = MouseEffectSetting.DefaultColorfulMistSizeScale * 100;
            if (effectType == MouseEffectType.StarlightArrow)
                text_StarlightArrowColor.Text = MouseEffectSetting.DefaultStarlightArrowColor;
            if (effectType == MouseEffectType.OrbitParticles)
            {
                slider_OrbitParticlesLayers.Value = MouseEffectSetting.DefaultOrbitParticlesLayers;
                slider_OrbitParticlesRadius.Value = MouseEffectSetting.DefaultOrbitParticlesRadius;
                slider_OrbitParticlesSpeed.Value = MouseEffectSetting.DefaultOrbitParticlesSpeed * 100;
                text_OrbitParticlesColor1.Text = MouseEffectSetting.DefaultOrbitParticlesColor1;
                text_OrbitParticlesColor2.Text = MouseEffectSetting.DefaultOrbitParticlesColor2;
                text_OrbitParticlesColor3.Text = MouseEffectSetting.DefaultOrbitParticlesColor3;
            }
            if (effectType == MouseEffectType.BinaryTrail)
            {
                check_BinaryTrailDefaultColors.IsChecked = true;
                text_BinaryTrailColor.Text = MouseEffectSetting.DefaultBinaryTrailColor;
                slider_BinaryTrailSize.Value = MouseEffectSetting.DefaultBinaryTrailSizeScale * 100;
                combo_BinaryTrailFont.SelectedValue = MouseEffectSetting.DefaultBinaryTrailFontFamily;
            }
            if (effectType == MouseEffectType.Cloud)
            {
                slider_CloudSize.Value = MouseEffectSetting.DefaultCloudSizeScale * 100;
                combo_CloudPosition.SelectedValue = MouseEffectSetting.DefaultCloudPosition;
                slider_CloudHorizontalOffset.Value = MouseEffectSetting.DefaultCloudHorizontalOffset;
                slider_CloudVerticalOffset.Value = MouseEffectSetting.DefaultCloudVerticalOffset;
            }
            if (effectType == MouseEffectType.PixelBlocks)
                slider_PixelBlocksSize.Value = MouseEffectSetting.DefaultPixelBlocksSizeScale * 100;
            if (effectType == MouseEffectType.Fireflies)
                slider_FirefliesFollowSpeed.Value = MouseEffectSetting.DefaultFirefliesFollowSpeed * 100;
            if (effectType == MouseEffectType.CottonCandy)
                slider_CottonCandySize.Value = MouseEffectSetting.DefaultCottonCandySizeScale * 100;
            if (effectType == MouseEffectType.FollowingAnimal)
                slider_FollowingAnimalSize.Value = MouseEffectSetting.DefaultFollowingAnimalSizeScale * 100;
            if (effectType == MouseEffectType.IconRain)
                slider_IconRainSize.Value = MouseEffectSetting.DefaultIconRainSizeScale * 100;
            if (effectType == MouseEffectType.FallingLeaves)
                slider_FallingLeavesSize.Value = MouseEffectSetting.DefaultFallingLeavesSizeScale * 100;
            if (effectType == MouseEffectType.FallingPetals)
            {
                slider_FallingPetalsSize.Value = MouseEffectSetting.DefaultFallingPetalsSizeScale * 100;
                combo_FallingPetalsColor.SelectedValue = MouseEffectSetting.DefaultFallingPetalsColor;
            }
            if (effectType == MouseEffectType.WaterRipple)
            {
                slider_WaterRippleSize.Value = MouseEffectSetting.DefaultWaterRippleSizeScale * 100;
                slider_WaterRippleFrequency.Value = MouseEffectSetting.DefaultWaterRippleFrequency;
                text_WaterRippleColor.Text = MouseEffectSetting.DefaultWaterRippleColor;
            }
            if (catkins)
            {
                text_CatkinsColor.Text = MouseEffectSetting.DefaultWillowCatkinsColor;
                slider_CatkinsSize.Value = MouseEffectSetting.DefaultWillowCatkinsSizeScale * 100;
            }
            if (effectType == MouseEffectType.Stardust)
                slider_StardustSize.Value = MouseEffectSetting.DefaultStardustSizeScale * 100;
            if (effectType == MouseEffectType.Snowflakes)
                slider_SnowflakesSize.Value = MouseEffectSetting.DefaultSnowflakesSizeScale * 100;
            if (effectType == MouseEffectType.FivePointedStars)
            {
                slider_FivePointedStarsSize.Value = MouseEffectSetting.DefaultFivePointedStarsSizeScale * 100;
                text_FivePointedStarsColor.Text = MouseEffectSetting.DefaultFivePointedStarsColor;
                check_FivePointedStarsRandomColors.IsChecked = MouseEffectSetting.DefaultFivePointedStarsUseRandomColors;
            }
            if (effectType == MouseEffectType.HaloRing)
            {
                slider_HaloRingSize.Value = MouseEffectSetting.DefaultHaloRingSizeScale * 100;
                slider_HaloRingBreathingFrequency.Value = MouseEffectSetting.DefaultHaloRingBreathingFrequency;
                text_HaloRingColor1.Text = MouseEffectSetting.DefaultHaloRingColor1;
                text_HaloRingColor2.Text = MouseEffectSetting.DefaultHaloRingColor2;
                text_HaloRingColor3.Text = MouseEffectSetting.DefaultHaloRingColor3;
            }
            if (effectType == MouseEffectType.NeonRibbon)
            {
                text_NeonRibbonColor1.Text = MouseEffectSetting.DefaultNeonRibbonColor1;
                text_NeonRibbonColor2.Text = MouseEffectSetting.DefaultNeonRibbonColor2;
                text_NeonRibbonColor3.Text = MouseEffectSetting.DefaultNeonRibbonColor3;
            }
            if (effectType == MouseEffectType.InkFluid)
            {
                text_InkFluidColor1.Text = MouseEffectSetting.DefaultInkFluidColor1;
                text_InkFluidColor2.Text = MouseEffectSetting.DefaultInkFluidColor2;
                text_InkFluidColor3.Text = MouseEffectSetting.DefaultInkFluidColor3;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (effectType == MouseEffectType.SwordAura && !HasValidSwordAuraColors())
            {
                UpdateSwordAuraColors();
                if (!MouseEffectSetting.IsValidColor(text_SwordAuraColor1.Text)) text_SwordAuraColor1.Focus();
                else if (!MouseEffectSetting.IsValidColor(text_SwordAuraColor2.Text)) text_SwordAuraColor2.Focus();
                else text_SwordAuraColor3.Focus();
                return;
            }
            if (effectType == MouseEffectType.StarlightArrow && !MouseEffectSetting.IsValidColor(text_StarlightArrowColor.Text))
            {
                UpdateStarlightArrowColor();
                text_StarlightArrowColor.Focus();
                return;
            }
            if (effectType == MouseEffectType.OrbitParticles && !HasValidOrbitParticlesColors())
            {
                UpdateOrbitParticlesColors();
                if (!MouseEffectSetting.IsValidColor(text_OrbitParticlesColor1.Text)) text_OrbitParticlesColor1.Focus();
                else if (!MouseEffectSetting.IsValidColor(text_OrbitParticlesColor2.Text)) text_OrbitParticlesColor2.Focus();
                else text_OrbitParticlesColor3.Focus();
                return;
            }
            if (effectType == MouseEffectType.BinaryTrail && check_BinaryTrailDefaultColors.IsChecked != true
                && !MouseEffectSetting.IsValidColor(text_BinaryTrailColor.Text))
            {
                UpdateBinaryTrailColor();
                text_BinaryTrailColor.Focus();
                return;
            }
            if (effectType == MouseEffectType.WaterRipple && !MouseEffectSetting.IsValidColor(text_WaterRippleColor.Text))
            {
                UpdateWaterRipplePreview();
                text_WaterRippleColor.Focus();
                return;
            }
            if (effectType == MouseEffectType.FivePointedStars && check_FivePointedStarsRandomColors.IsChecked != true
                && !MouseEffectSetting.IsValidColor(text_FivePointedStarsColor.Text))
            {
                UpdateFivePointedStarsPreview();
                text_FivePointedStarsColor.Focus();
                return;
            }
            if (effectType == MouseEffectType.HaloRing && !HasValidHaloRingColors())
            {
                UpdateHaloRingPreview();
                var invalidInput = !MouseEffectSetting.IsValidColor(text_HaloRingColor1.Text) ? text_HaloRingColor1
                    : !MouseEffectSetting.IsValidColor(text_HaloRingColor2.Text) ? text_HaloRingColor2 : text_HaloRingColor3;
                invalidInput.Focus();
                return;
            }
            if (catkins && !MouseEffectSetting.IsValidColor(text_CatkinsColor.Text))
            {
                UpdateCatkinsPreview();
                text_CatkinsColor.Focus();
                return;
            }
            if (effectType == MouseEffectType.NeonRibbon && !HasValidNeonRibbonColors())
            {
                UpdateNeonRibbonPreview();
                var invalidInput = !MouseEffectSetting.IsValidColor(text_NeonRibbonColor1.Text) ? text_NeonRibbonColor1
                    : !MouseEffectSetting.IsValidColor(text_NeonRibbonColor2.Text) ? text_NeonRibbonColor2 : text_NeonRibbonColor3;
                invalidInput.Focus();
                return;
            }
            if (effectType == MouseEffectType.InkFluid && !HasValidInkFluidColors())
            {
                UpdateInkFluidPreview();
                var invalidInput = !MouseEffectSetting.IsValidColor(text_InkFluidColor1.Text) ? text_InkFluidColor1
                    : !MouseEffectSetting.IsValidColor(text_InkFluidColor2.Text) ? text_InkFluidColor2 : text_InkFluidColor3;
                invalidInput.Focus();
                return;
            }

            if (trail && !MouseEffectSetting.IsValidColor(text_TrailColor.Text))
            {
                UpdateTrailPreview();
                text_TrailColor.Focus();
                return;
            }

            if (following && !MouseEffectSetting.IsValidFollowingColor(text_FollowingColor.Text))
            {
                UpdateFollowingPreview();
                text_FollowingColor.Focus();
                return;
            }

            var settings = mainConfig.MouseEffectSetting;
            var previousFireworksOpacity = settings.FireworksOpacity;
            var previousCatkinsOpacity = settings.WillowCatkinsOpacity;
            var previousCatkinsColor = settings.WillowCatkinsColor;
            var previousCatkinsSize = settings.WillowCatkinsSizeScale;
            var previousSnowflakesOpacity = settings.SnowflakesOpacity;
            var previousSnowflakesSize = settings.SnowflakesSizeScale;
            var previousStarsOpacity = settings.FivePointedStarsOpacity;
            var previousStarsSize = settings.FivePointedStarsSizeScale;
            var previousStarsColor = settings.FivePointedStarsColor;
            var previousStarsRandomColors = settings.FivePointedStarsUseRandomColors;
            var previousInkFluidOpacity = settings.InkFluidOpacity;
            var previousNeonRibbonOpacity = settings.NeonRibbonOpacity;
            var previousWaterRippleOpacity = settings.WaterRippleOpacity;
            var previousWaterRippleSize = settings.WaterRippleSizeScale;
            var previousWaterRippleFrequency = settings.WaterRippleFrequency;
            var previousWaterRippleColor = settings.WaterRippleColor;
            var previousStardustOpacity = settings.StardustOpacity;
            var previousFallingPetalsOpacity = settings.FallingPetalsOpacity;
            var previousFallingPetalsSize = settings.FallingPetalsSizeScale;
            var previousFallingPetalsColor = settings.FallingPetalsColor;
            var previousColorfulMistOpacity = settings.ColorfulMistOpacity;
            var previousColorfulMistSize = settings.ColorfulMistSizeScale;
            var previousFireworksTrailOpacity = settings.FireworksTrailOpacity;
            var previousFallingLeavesOpacity = settings.FallingLeavesOpacity;
            var previousFallingLeavesSize = settings.FallingLeavesSizeScale;
            var previousIconRainOpacity = settings.IconRainOpacity;
            var previousIconRainSize = settings.IconRainSizeScale;
            var previousHaloRingOpacity = settings.HaloRingOpacity;
            var previousHaloRingSize = settings.HaloRingSizeScale;
            var previousHaloRingFrequency = settings.HaloRingBreathingFrequency;
            var previousHaloRingColor1 = settings.HaloRingColor1;
            var previousHaloRingColor2 = settings.HaloRingColor2;
            var previousHaloRingColor3 = settings.HaloRingColor3;
            var previousFollowingAnimalOpacity = settings.FollowingAnimalOpacity;
            var previousFollowingAnimalSize = settings.FollowingAnimalSizeScale;
            var previousCottonCandyOpacity = settings.CottonCandyOpacity;
            var previousFirefliesOpacity = settings.FirefliesOpacity;
            var previousFirefliesFollowSpeed = settings.FirefliesFollowSpeed;
            var previousPixelBlocksOpacity = settings.PixelBlocksOpacity;
            var previousButterfliesOpacity = settings.ButterfliesOpacity;
            var previousCloudOpacity = settings.CloudOpacity;
            var previousMusicNotesOpacity = settings.MusicNotesOpacity;
            var previousBinaryTrailOpacity = settings.BinaryTrailOpacity;
            var previousOrbitParticlesOpacity = settings.OrbitParticlesOpacity;
            var previousStarlightArrowOpacity = settings.StarlightArrowOpacity;
            var previousSwordAuraOpacity = settings.SwordAuraOpacity;
            var previousDrizzleOpacity = settings.DrizzleOpacity;
            var previousFlameOpacity = settings.FlameOpacity;
            var previousFlameSize = settings.FlameSizeScale;
            var previousFlameSpeed = settings.FlameSpeed;
            var previousFlameRadius = settings.FlameRadius;
            var previousSwordAuraSize = settings.SwordAuraSizeScale;
            var previousSwordAuraColor1 = settings.SwordAuraColor1;
            var previousSwordAuraColor2 = settings.SwordAuraColor2;
            var previousSwordAuraColor3 = settings.SwordAuraColor3;
            var previousStarlightArrowColor = settings.StarlightArrowColor;
            var previousOrbitParticlesLayers = settings.OrbitParticlesLayers;
            var previousOrbitParticlesRadius = settings.OrbitParticlesRadius;
            var previousOrbitParticlesSpeed = settings.OrbitParticlesSpeed;
            var previousOrbitParticlesColor1 = settings.OrbitParticlesColor1;
            var previousOrbitParticlesColor2 = settings.OrbitParticlesColor2;
            var previousOrbitParticlesColor3 = settings.OrbitParticlesColor3;
            var previousBinaryTrailDefaultColors = settings.BinaryTrailUseDefaultColors;
            var previousBinaryTrailColor = settings.BinaryTrailColor;
            var previousBinaryTrailSize = settings.BinaryTrailSizeScale;
            var previousBinaryTrailFont = settings.BinaryTrailFontFamily;
            var previousCloudSize = settings.CloudSizeScale;
            var previousCloudPosition = settings.CloudPosition;
            var previousCloudHorizontalOffset = settings.CloudHorizontalOffset;
            var previousCloudVerticalOffset = settings.CloudVerticalOffset;
            var previousPixelBlocksSize = settings.PixelBlocksSizeScale;
            var previousCottonCandySize = settings.CottonCandySizeScale;
            var previousStardustSizeScale = settings.StardustSizeScale;
            var previousNeonRibbonColor1 = settings.NeonRibbonColor1;
            var previousNeonRibbonColor2 = settings.NeonRibbonColor2;
            var previousNeonRibbonColor3 = settings.NeonRibbonColor3;
            var previousInkFluidColor1 = settings.InkFluidColor1;
            var previousInkFluidColor2 = settings.InkFluidColor2;
            var previousInkFluidColor3 = settings.InkFluidColor3;
            var previousExpansion = settings.FireworksExpansion;
            var previousFollowingColor = settings.FollowingColor;
            var previousCenterOpacity = settings.FollowingCenterOpacity;
            var previousEdgeOpacity = settings.FollowingEdgeOpacity;
            var previousSize = settings.FollowingSize;
            var previousDelay = settings.FollowingDelayMilliseconds;
            var previousTrailColor = settings.TrailColor;
            var previousTrailSize = settings.TrailSize;
            var previousTrailHeadOpacity = settings.TrailHeadOpacity;
            var previousTrailTailOpacity = settings.TrailTailOpacity;
            if (trail)
            {
                settings.TrailColor = text_TrailColor.Text;
                settings.TrailSize = slider_TrailSize.Value;
                settings.TrailHeadOpacity = slider_TrailHeadOpacity.Value / 100;
                settings.TrailTailOpacity = slider_TrailTailOpacity.Value / 100;
            }
            else if (following)
            {
                settings.FollowingColor = text_FollowingColor.Text;
                settings.FollowingCenterOpacity = slider_FollowingCenterOpacity.Value / 100;
                settings.FollowingEdgeOpacity = slider_FollowingEdgeOpacity.Value / 100;
                settings.FollowingSize = slider_FollowingSize.Value;
                settings.FollowingDelayMilliseconds = slider_FollowingDelay.Value;
            }
            else if (fireworks)
            {
                settings.FireworksOpacity = slider_Opacity.Value / 100;
                settings.FireworksExpansion = slider_Expansion.Value / 100;
            }
            else if (effectType == MouseEffectType.Snowflakes)
            {
                settings.SnowflakesOpacity = slider_Opacity.Value / 100;
                settings.SnowflakesSizeScale = slider_SnowflakesSize.Value / 100;
            }
            else if (effectType == MouseEffectType.FivePointedStars)
            {
                settings.FivePointedStarsOpacity = slider_Opacity.Value / 100;
                settings.FivePointedStarsSizeScale = slider_FivePointedStarsSize.Value / 100;
                settings.FivePointedStarsUseRandomColors = check_FivePointedStarsRandomColors.IsChecked == true;
                if (MouseEffectSetting.IsValidColor(text_FivePointedStarsColor.Text))
                    settings.FivePointedStarsColor = text_FivePointedStarsColor.Text;
            }
            else if (effectType == MouseEffectType.NeonRibbon)
            {
                settings.NeonRibbonOpacity = slider_Opacity.Value / 100;
                settings.NeonRibbonColor1 = text_NeonRibbonColor1.Text;
                settings.NeonRibbonColor2 = text_NeonRibbonColor2.Text;
                settings.NeonRibbonColor3 = text_NeonRibbonColor3.Text;
            }
            else if (effectType == MouseEffectType.WaterRipple)
            {
                settings.WaterRippleOpacity = slider_Opacity.Value / 100;
                settings.WaterRippleSizeScale = slider_WaterRippleSize.Value / 100;
                settings.WaterRippleFrequency = slider_WaterRippleFrequency.Value;
                settings.WaterRippleColor = text_WaterRippleColor.Text;
            }
            else if (effectType == MouseEffectType.Stardust)
            {
                settings.StardustOpacity = slider_Opacity.Value / 100;
                settings.StardustSizeScale = slider_StardustSize.Value / 100;
            }
            else if (effectType == MouseEffectType.InkFluid)
            {
                settings.InkFluidOpacity = slider_Opacity.Value / 100;
                settings.InkFluidColor1 = text_InkFluidColor1.Text;
                settings.InkFluidColor2 = text_InkFluidColor2.Text;
                settings.InkFluidColor3 = text_InkFluidColor3.Text;
            }
            else if (effectType == MouseEffectType.FallingPetals)
            {
                settings.FallingPetalsOpacity = slider_Opacity.Value / 100;
                settings.FallingPetalsSizeScale = slider_FallingPetalsSize.Value / 100;
                settings.FallingPetalsColor = combo_FallingPetalsColor.SelectedValue as string;
            }
            else if (effectType == MouseEffectType.ColorfulMist)
            {
                settings.ColorfulMistOpacity = slider_Opacity.Value / 100;
                settings.ColorfulMistSizeScale = slider_ColorfulMistSize.Value / 100;
            }
            else if (effectType == MouseEffectType.FireworksTrail)
            {
                settings.FireworksTrailOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.FallingLeaves)
            {
                settings.FallingLeavesOpacity = slider_Opacity.Value / 100;
                settings.FallingLeavesSizeScale = slider_FallingLeavesSize.Value / 100;
            }
            else if (effectType == MouseEffectType.IconRain)
            {
                settings.IconRainOpacity = slider_Opacity.Value / 100;
                settings.IconRainSizeScale = slider_IconRainSize.Value / 100;
            }
            else if (effectType == MouseEffectType.HaloRing)
            {
                settings.HaloRingOpacity = slider_Opacity.Value / 100;
                settings.HaloRingSizeScale = slider_HaloRingSize.Value / 100;
                settings.HaloRingBreathingFrequency = slider_HaloRingBreathingFrequency.Value;
                settings.HaloRingColor1 = text_HaloRingColor1.Text;
                settings.HaloRingColor2 = text_HaloRingColor2.Text;
                settings.HaloRingColor3 = text_HaloRingColor3.Text;
            }
            else if (effectType == MouseEffectType.FollowingAnimal)
            {
                settings.FollowingAnimalOpacity = slider_Opacity.Value / 100;
                settings.FollowingAnimalSizeScale = slider_FollowingAnimalSize.Value / 100;
            }
            else if (effectType == MouseEffectType.Fireflies)
            {
                settings.FirefliesOpacity = slider_Opacity.Value / 100;
                settings.FirefliesFollowSpeed = slider_FirefliesFollowSpeed.Value / 100;
            }
            else if (effectType == MouseEffectType.PixelBlocks)
            {
                settings.PixelBlocksOpacity = slider_Opacity.Value / 100;
                settings.PixelBlocksSizeScale = slider_PixelBlocksSize.Value / 100;
            }
            else if (effectType == MouseEffectType.Butterflies)
            {
                settings.ButterfliesOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.MusicNotes)
            {
                settings.MusicNotesOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.Drizzle)
            {
                settings.DrizzleOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.Flame)
            {
                settings.FlameOpacity = slider_Opacity.Value / 100;
                settings.FlameSizeScale = slider_FlameSize.Value / 100;
                settings.FlameSpeed = slider_FlameSpeed.Value / 100;
                settings.FlameRadius = slider_FlameRadius.Value;
            }
            else if (effectType == MouseEffectType.SwordAura)
            {
                settings.SwordAuraOpacity = slider_Opacity.Value / 100;
                settings.SwordAuraSizeScale = slider_SwordAuraSize.Value / 100;
                settings.SwordAuraColor1 = text_SwordAuraColor1.Text;
                settings.SwordAuraColor2 = text_SwordAuraColor2.Text;
                settings.SwordAuraColor3 = text_SwordAuraColor3.Text;
            }
            else if (effectType == MouseEffectType.StarlightArrow)
            {
                settings.StarlightArrowOpacity = slider_Opacity.Value / 100;
                settings.StarlightArrowColor = text_StarlightArrowColor.Text;
            }
            else if (effectType == MouseEffectType.OrbitParticles)
            {
                settings.OrbitParticlesOpacity = slider_Opacity.Value / 100;
                settings.OrbitParticlesLayers = (int)slider_OrbitParticlesLayers.Value;
                settings.OrbitParticlesRadius = slider_OrbitParticlesRadius.Value;
                settings.OrbitParticlesSpeed = slider_OrbitParticlesSpeed.Value / 100;
                settings.OrbitParticlesColor1 = text_OrbitParticlesColor1.Text;
                settings.OrbitParticlesColor2 = text_OrbitParticlesColor2.Text;
                settings.OrbitParticlesColor3 = text_OrbitParticlesColor3.Text;
            }
            else if (effectType == MouseEffectType.BinaryTrail)
            {
                settings.BinaryTrailOpacity = slider_Opacity.Value / 100;
                settings.BinaryTrailUseDefaultColors = check_BinaryTrailDefaultColors.IsChecked == true;
                if (MouseEffectSetting.IsValidColor(text_BinaryTrailColor.Text))
                    settings.BinaryTrailColor = text_BinaryTrailColor.Text;
                settings.BinaryTrailSizeScale = slider_BinaryTrailSize.Value / 100;
                settings.BinaryTrailFontFamily = combo_BinaryTrailFont.SelectedValue as string;
            }
            else if (effectType == MouseEffectType.Cloud)
            {
                settings.CloudOpacity = slider_Opacity.Value / 100;
                settings.CloudSizeScale = slider_CloudSize.Value / 100;
                settings.CloudPosition = combo_CloudPosition.SelectedValue as string;
                settings.CloudHorizontalOffset = slider_CloudHorizontalOffset.Value;
                settings.CloudVerticalOffset = slider_CloudVerticalOffset.Value;
            }
            else if (effectType == MouseEffectType.CottonCandy)
            {
                settings.CottonCandyOpacity = slider_Opacity.Value / 100;
                settings.CottonCandySizeScale = slider_CottonCandySize.Value / 100;
            }
            else
            {
                settings.WillowCatkinsOpacity = slider_Opacity.Value / 100;
                settings.WillowCatkinsColor = text_CatkinsColor.Text;
                settings.WillowCatkinsSizeScale = slider_CatkinsSize.Value / 100;
            }

            bool saved;
            try
            {
                saved = ConfigHelper.SaveMainConfig(mainConfig);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                saved = false;
            }

            if (!saved)
            {
                settings.FireworksOpacity = previousFireworksOpacity;
                settings.WillowCatkinsOpacity = previousCatkinsOpacity;
                settings.WillowCatkinsColor = previousCatkinsColor;
                settings.WillowCatkinsSizeScale = previousCatkinsSize;
                settings.SnowflakesOpacity = previousSnowflakesOpacity;
                settings.SnowflakesSizeScale = previousSnowflakesSize;
                settings.FivePointedStarsOpacity = previousStarsOpacity;
                settings.FivePointedStarsSizeScale = previousStarsSize;
                settings.FivePointedStarsColor = previousStarsColor;
                settings.FivePointedStarsUseRandomColors = previousStarsRandomColors;
                settings.InkFluidOpacity = previousInkFluidOpacity;
                settings.NeonRibbonOpacity = previousNeonRibbonOpacity;
                settings.WaterRippleOpacity = previousWaterRippleOpacity;
                settings.WaterRippleSizeScale = previousWaterRippleSize;
                settings.WaterRippleFrequency = previousWaterRippleFrequency;
                settings.WaterRippleColor = previousWaterRippleColor;
                settings.StardustOpacity = previousStardustOpacity;
                settings.FallingPetalsOpacity = previousFallingPetalsOpacity;
                settings.FallingPetalsSizeScale = previousFallingPetalsSize;
                settings.FallingPetalsColor = previousFallingPetalsColor;
                settings.ColorfulMistOpacity = previousColorfulMistOpacity;
                settings.ColorfulMistSizeScale = previousColorfulMistSize;
                settings.FireworksTrailOpacity = previousFireworksTrailOpacity;
                settings.FallingLeavesOpacity = previousFallingLeavesOpacity;
                settings.FallingLeavesSizeScale = previousFallingLeavesSize;
                settings.IconRainOpacity = previousIconRainOpacity;
                settings.IconRainSizeScale = previousIconRainSize;
                settings.HaloRingOpacity = previousHaloRingOpacity;
                settings.HaloRingSizeScale = previousHaloRingSize;
                settings.HaloRingBreathingFrequency = previousHaloRingFrequency;
                settings.HaloRingColor1 = previousHaloRingColor1;
                settings.HaloRingColor2 = previousHaloRingColor2;
                settings.HaloRingColor3 = previousHaloRingColor3;
                settings.FollowingAnimalOpacity = previousFollowingAnimalOpacity;
                settings.FollowingAnimalSizeScale = previousFollowingAnimalSize;
                settings.CottonCandyOpacity = previousCottonCandyOpacity;
                settings.FirefliesOpacity = previousFirefliesOpacity;
                settings.FirefliesFollowSpeed = previousFirefliesFollowSpeed;
                settings.PixelBlocksOpacity = previousPixelBlocksOpacity;
                settings.ButterfliesOpacity = previousButterfliesOpacity;
                settings.CloudOpacity = previousCloudOpacity;
                settings.MusicNotesOpacity = previousMusicNotesOpacity;
                settings.BinaryTrailOpacity = previousBinaryTrailOpacity;
                settings.OrbitParticlesOpacity = previousOrbitParticlesOpacity;
                settings.StarlightArrowOpacity = previousStarlightArrowOpacity;
                settings.SwordAuraOpacity = previousSwordAuraOpacity;
                settings.DrizzleOpacity = previousDrizzleOpacity;
                settings.FlameOpacity = previousFlameOpacity;
                settings.FlameSizeScale = previousFlameSize;
                settings.FlameSpeed = previousFlameSpeed;
                settings.FlameRadius = previousFlameRadius;
                settings.SwordAuraSizeScale = previousSwordAuraSize;
                settings.SwordAuraColor1 = previousSwordAuraColor1;
                settings.SwordAuraColor2 = previousSwordAuraColor2;
                settings.SwordAuraColor3 = previousSwordAuraColor3;
                settings.StarlightArrowColor = previousStarlightArrowColor;
                settings.OrbitParticlesLayers = previousOrbitParticlesLayers;
                settings.OrbitParticlesRadius = previousOrbitParticlesRadius;
                settings.OrbitParticlesSpeed = previousOrbitParticlesSpeed;
                settings.OrbitParticlesColor1 = previousOrbitParticlesColor1;
                settings.OrbitParticlesColor2 = previousOrbitParticlesColor2;
                settings.OrbitParticlesColor3 = previousOrbitParticlesColor3;
                settings.BinaryTrailUseDefaultColors = previousBinaryTrailDefaultColors;
                settings.BinaryTrailColor = previousBinaryTrailColor;
                settings.BinaryTrailSizeScale = previousBinaryTrailSize;
                settings.BinaryTrailFontFamily = previousBinaryTrailFont;
                settings.CloudSizeScale = previousCloudSize;
                settings.CloudPosition = previousCloudPosition;
                settings.CloudHorizontalOffset = previousCloudHorizontalOffset;
                settings.CloudVerticalOffset = previousCloudVerticalOffset;
                settings.PixelBlocksSizeScale = previousPixelBlocksSize;
                settings.CottonCandySizeScale = previousCottonCandySize;
                settings.StardustSizeScale = previousStardustSizeScale;
                settings.NeonRibbonColor1 = previousNeonRibbonColor1;
                settings.NeonRibbonColor2 = previousNeonRibbonColor2;
                settings.NeonRibbonColor3 = previousNeonRibbonColor3;
                settings.InkFluidColor1 = previousInkFluidColor1;
                settings.InkFluidColor2 = previousInkFluidColor2;
                settings.InkFluidColor3 = previousInkFluidColor3;
                settings.FireworksExpansion = previousExpansion;
                settings.FollowingColor = previousFollowingColor;
                settings.FollowingCenterOpacity = previousCenterOpacity;
                settings.FollowingEdgeOpacity = previousEdgeOpacity;
                settings.FollowingSize = previousSize;
                settings.FollowingDelayMilliseconds = previousDelay;
                settings.TrailColor = previousTrailColor;
                settings.TrailSize = previousTrailSize;
                settings.TrailHeadOpacity = previousTrailHeadOpacity;
                settings.TrailTailOpacity = previousTrailTailOpacity;
                MessageBox.Show(this, "无法保存特效设置，请检查配置目录的写入权限后重试。", "保存失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}
