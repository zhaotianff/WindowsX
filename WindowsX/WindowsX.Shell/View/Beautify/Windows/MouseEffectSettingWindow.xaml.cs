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
                _ => fireworks ? settings.FireworksOpacity : settings.WillowCatkinsOpacity
            }) * 100;
            slider_Expansion.Value = settings.FireworksExpansion * 100;
            text_CatkinsColor.Text = settings.WillowCatkinsColor;
            slider_CatkinsSize.Value = settings.WillowCatkinsSizeScale * 100;
            panel_Catkins.Visibility = catkins ? Visibility.Visible : Visibility.Collapsed;
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
            UpdateCatkinsPreview();
            UpdateFollowingPreview();
            UpdateTrailPreview();
            UpdateInkFluidPreview();
            UpdateNeonRibbonPreview();
            UpdateHaloRingPreview();
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
            if (catkins)
            {
                text_CatkinsColor.Text = MouseEffectSetting.DefaultWillowCatkinsColor;
                slider_CatkinsSize.Value = MouseEffectSetting.DefaultWillowCatkinsSizeScale * 100;
            }
            if (effectType == MouseEffectType.Stardust)
                slider_StardustSize.Value = MouseEffectSetting.DefaultStardustSizeScale * 100;
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
            var previousStarsOpacity = settings.FivePointedStarsOpacity;
            var previousInkFluidOpacity = settings.InkFluidOpacity;
            var previousNeonRibbonOpacity = settings.NeonRibbonOpacity;
            var previousWaterRippleOpacity = settings.WaterRippleOpacity;
            var previousStardustOpacity = settings.StardustOpacity;
            var previousFallingPetalsOpacity = settings.FallingPetalsOpacity;
            var previousColorfulMistOpacity = settings.ColorfulMistOpacity;
            var previousFireworksTrailOpacity = settings.FireworksTrailOpacity;
            var previousFallingLeavesOpacity = settings.FallingLeavesOpacity;
            var previousIconRainOpacity = settings.IconRainOpacity;
            var previousHaloRingOpacity = settings.HaloRingOpacity;
            var previousHaloRingSize = settings.HaloRingSizeScale;
            var previousHaloRingFrequency = settings.HaloRingBreathingFrequency;
            var previousHaloRingColor1 = settings.HaloRingColor1;
            var previousHaloRingColor2 = settings.HaloRingColor2;
            var previousHaloRingColor3 = settings.HaloRingColor3;
            var previousFollowingAnimalOpacity = settings.FollowingAnimalOpacity;
            var previousCottonCandyOpacity = settings.CottonCandyOpacity;
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
            }
            else if (effectType == MouseEffectType.FivePointedStars)
            {
                settings.FivePointedStarsOpacity = slider_Opacity.Value / 100;
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
            }
            else if (effectType == MouseEffectType.ColorfulMist)
            {
                settings.ColorfulMistOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.FireworksTrail)
            {
                settings.FireworksTrailOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.FallingLeaves)
            {
                settings.FallingLeavesOpacity = slider_Opacity.Value / 100;
            }
            else if (effectType == MouseEffectType.IconRain)
            {
                settings.IconRainOpacity = slider_Opacity.Value / 100;
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
            }
            else if (effectType == MouseEffectType.CottonCandy)
            {
                settings.CottonCandyOpacity = slider_Opacity.Value / 100;
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
                settings.FivePointedStarsOpacity = previousStarsOpacity;
                settings.InkFluidOpacity = previousInkFluidOpacity;
                settings.NeonRibbonOpacity = previousNeonRibbonOpacity;
                settings.WaterRippleOpacity = previousWaterRippleOpacity;
                settings.StardustOpacity = previousStardustOpacity;
                settings.FallingPetalsOpacity = previousFallingPetalsOpacity;
                settings.ColorfulMistOpacity = previousColorfulMistOpacity;
                settings.FireworksTrailOpacity = previousFireworksTrailOpacity;
                settings.FallingLeavesOpacity = previousFallingLeavesOpacity;
                settings.IconRainOpacity = previousIconRainOpacity;
                settings.HaloRingOpacity = previousHaloRingOpacity;
                settings.HaloRingSizeScale = previousHaloRingSize;
                settings.HaloRingBreathingFrequency = previousHaloRingFrequency;
                settings.HaloRingColor1 = previousHaloRingColor1;
                settings.HaloRingColor2 = previousHaloRingColor2;
                settings.HaloRingColor3 = previousHaloRingColor3;
                settings.FollowingAnimalOpacity = previousFollowingAnimalOpacity;
                settings.CottonCandyOpacity = previousCottonCandyOpacity;
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
