using System;

namespace WindowsX.Config.Model
{
    public class MouseEffectSetting
    {
        public bool IsEnabled { get; set; }
        public int SelectedEffectType { get; set; }

        public const string DefaultFollowingColor = "#0080FF";
        public const double DefaultFollowingCenterOpacity = 0.5;
        public const double DefaultFollowingEdgeOpacity = 0;
        public const double DefaultFollowingSize = 30;
        public const double DefaultFollowingDelayMilliseconds = 0;
        public const double MaxFollowingDelayMilliseconds = 1000;
        public const string DefaultTrailColor = "#0080FF";
        public const double DefaultTrailSize = 20;
        public const double DefaultTrailHeadOpacity = 0.6;
        public const double DefaultTrailTailOpacity = 0;
        public const string DefaultInkFluidColor1 = "#18416E";
        public const string DefaultInkFluidColor2 = "#293168";
        public const string DefaultInkFluidColor3 = "#187085";
        public const string DefaultNeonRibbonColor1 = "#00F0FF";
        public const string DefaultNeonRibbonColor2 = "#9E44FF";
        public const string DefaultNeonRibbonColor3 = "#FF37B4";
        public const double DefaultStardustSizeScale = 1;
        public const double DefaultFallingPetalsSizeScale = 1;
        public const double DefaultFallingLeavesSizeScale = 1;
        public const double DefaultIconRainSizeScale = 1;
        public const double DefaultFollowingAnimalSizeScale = 1;
        public const double DefaultCottonCandySizeScale = 1;
        public const double DefaultColorfulMistSizeScale = 1;
        public const double DefaultFirefliesFollowSpeed = 1;
        public const double DefaultPixelBlocksSizeScale = 1;
        public const int DefaultOrbitParticlesLayers = 2;
        public const int MaxOrbitParticlesLayers = 4;
        public const double DefaultOrbitParticlesRadius = 28;
        public const double DefaultOrbitParticlesSpeed = 1;
        public const string DefaultOrbitParticlesColor1 = "#5CE1FF";
        public const string DefaultOrbitParticlesColor2 = "#B38BFF";
        public const string DefaultOrbitParticlesColor3 = "#FF91CD";
        public const double DefaultBinaryTrailSizeScale = 1;
        public const string DefaultBinaryTrailColor = "#5CFF8F";
        public const string DefaultBinaryTrailFontFamily = "Vector";
        public static System.Collections.Generic.IReadOnlyList<string> BinaryTrailFontFamilies { get; } =
            Array.AsReadOnly(new[] { DefaultBinaryTrailFontFamily, "Consolas", "Courier New", "Segoe UI", "Arial" });
        public const double DefaultCloudSizeScale = 1;
        public const string DefaultCloudPosition = "BottomRight";
        public const double DefaultCloudHorizontalOffset = 30;
        public const double DefaultCloudVerticalOffset = 26;
        public const string DefaultFallingPetalsColor = "#FFB6C1";
        public static System.Collections.Generic.IReadOnlyList<string> FallingPetalsPresetColors { get; } =
            Array.AsReadOnly(new[] { DefaultFallingPetalsColor, "#D8C7F0", "#C2DFF5", "#C8E6D4", "#FFF0BD", "#FFD8BF" });
        public const double DefaultWaterRippleSizeScale = 1;
        public const double DefaultWaterRippleFrequency = 1.0 / 0.14;
        public const string DefaultWaterRippleColor = "#69D2FF";
        public const double DefaultSnowflakesSizeScale = 1;
        public const double DefaultFivePointedStarsSizeScale = 1;
        public const string DefaultFivePointedStarsColor = "#FFD700";
        public const bool DefaultFivePointedStarsUseRandomColors = true;
        public const double DefaultHaloRingSizeScale = 1;
        public const double DefaultHaloRingBreathingFrequency = 1.0 / 2.4;
        public const string DefaultHaloRingColor1 = "#41E1FF";
        public const string DefaultHaloRingColor2 = "#9B6EFF";
        public const string DefaultHaloRingColor3 = "#FF78C8";

        private double fireworksOpacity = 1;
        private double willowCatkinsOpacity = 1;
        public const string DefaultWillowCatkinsColor = "#FFFAF0";
        public const double DefaultWillowCatkinsSizeScale = 1;
        private string willowCatkinsColor = DefaultWillowCatkinsColor;
        private double willowCatkinsSizeScale = DefaultWillowCatkinsSizeScale;
        private double fireworksExpansion = 1;
        private double snowflakesOpacity = 1;
        private double snowflakesSizeScale = DefaultSnowflakesSizeScale;
        private double fivePointedStarsOpacity = 1;
        private double fivePointedStarsSizeScale = DefaultFivePointedStarsSizeScale;
        private string fivePointedStarsColor = DefaultFivePointedStarsColor;
        private double inkFluidOpacity = 1;
        private double neonRibbonOpacity = 1;
        private double waterRippleOpacity = 1;
        private double waterRippleSizeScale = DefaultWaterRippleSizeScale;
        private double waterRippleFrequency = DefaultWaterRippleFrequency;
        private string waterRippleColor = DefaultWaterRippleColor;
        private double stardustOpacity = 1;
        private double fallingPetalsOpacity = 1;
        private double fallingPetalsSizeScale = DefaultFallingPetalsSizeScale;
        private string fallingPetalsColor = DefaultFallingPetalsColor;
        private double colorfulMistOpacity = 1;
        private double colorfulMistSizeScale = DefaultColorfulMistSizeScale;
        private double fireworksTrailOpacity = 1;
        private double fallingLeavesOpacity = 1;
        private double fallingLeavesSizeScale = DefaultFallingLeavesSizeScale;
        private double iconRainOpacity = 1;
        private double iconRainSizeScale = DefaultIconRainSizeScale;
        private double haloRingOpacity = 1;
        private double haloRingSizeScale = DefaultHaloRingSizeScale;
        private double haloRingBreathingFrequency = DefaultHaloRingBreathingFrequency;
        private string haloRingColor1 = DefaultHaloRingColor1;
        private string haloRingColor2 = DefaultHaloRingColor2;
        private string haloRingColor3 = DefaultHaloRingColor3;
        private double followingAnimalOpacity = 1;
        private double followingAnimalSizeScale = DefaultFollowingAnimalSizeScale;
        private double cottonCandyOpacity = 1;
        private double firefliesOpacity = 1;
        private double firefliesFollowSpeed = DefaultFirefliesFollowSpeed;
        private double pixelBlocksOpacity = 1;
        private double butterfliesOpacity = 1;
        private double cloudOpacity = 1;
        private double musicNotesOpacity = 1;
        private double binaryTrailOpacity = 1;
        private double orbitParticlesOpacity = 1;
        private double starlightArrowOpacity = 1;
        private double swordAuraOpacity = 1;
        private double drizzleOpacity = 1;
        private double flameOpacity = 1;
        public const double DefaultFlameSizeScale = 1;
        public const double DefaultFlameSpeed = 1;
        public const double DefaultFlameRadius = 34;
        private double flameRadius = DefaultFlameRadius;
        private double flameSizeScale = DefaultFlameSizeScale;
        private double flameSpeed = DefaultFlameSpeed;
        public const double DefaultSwordAuraSizeScale = 1;
        public const string DefaultSwordAuraColor1 = "#18416E";
        public const string DefaultSwordAuraColor2 = "#293168";
        public const string DefaultSwordAuraColor3 = "#187085";
        private double swordAuraSizeScale = DefaultSwordAuraSizeScale;
        private string swordAuraColor1 = DefaultSwordAuraColor1;
        private string swordAuraColor2 = DefaultSwordAuraColor2;
        private string swordAuraColor3 = DefaultSwordAuraColor3;
        public const string DefaultStarlightArrowColor = "#FFD570";
        private string starlightArrowColor = DefaultStarlightArrowColor;
        private int orbitParticlesLayers = DefaultOrbitParticlesLayers;
        private double orbitParticlesRadius = DefaultOrbitParticlesRadius;
        private double orbitParticlesSpeed = DefaultOrbitParticlesSpeed;
        private string orbitParticlesColor1 = DefaultOrbitParticlesColor1;
        private string orbitParticlesColor2 = DefaultOrbitParticlesColor2;
        private string orbitParticlesColor3 = DefaultOrbitParticlesColor3;
        private double binaryTrailSizeScale = DefaultBinaryTrailSizeScale;
        private string binaryTrailColor = DefaultBinaryTrailColor;
        private string binaryTrailFontFamily = DefaultBinaryTrailFontFamily;
        private double cloudSizeScale = DefaultCloudSizeScale;
        private string cloudPosition = DefaultCloudPosition;
        private double cloudHorizontalOffset = DefaultCloudHorizontalOffset;
        private double cloudVerticalOffset = DefaultCloudVerticalOffset;
        private double pixelBlocksSizeScale = DefaultPixelBlocksSizeScale;
        private double cottonCandySizeScale = DefaultCottonCandySizeScale;
        private double stardustSizeScale = DefaultStardustSizeScale;
        private string neonRibbonColor1 = DefaultNeonRibbonColor1;
        private string neonRibbonColor2 = DefaultNeonRibbonColor2;
        private string neonRibbonColor3 = DefaultNeonRibbonColor3;
        private string inkFluidColor1 = DefaultInkFluidColor1;
        private string inkFluidColor2 = DefaultInkFluidColor2;
        private string inkFluidColor3 = DefaultInkFluidColor3;
        private string followingColor = DefaultFollowingColor;
        private double followingCenterOpacity = DefaultFollowingCenterOpacity;
        private double followingEdgeOpacity = DefaultFollowingEdgeOpacity;
        private double followingSize = DefaultFollowingSize;
        private double followingDelayMilliseconds = DefaultFollowingDelayMilliseconds;
        private string trailColor = DefaultTrailColor;
        private double trailSize = DefaultTrailSize;
        private double trailHeadOpacity = DefaultTrailHeadOpacity;
        private double trailTailOpacity = DefaultTrailTailOpacity;

        public string TrailColor
        {
            get => trailColor;
            set => trailColor = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultTrailColor;
        }

        public double TrailSize
        {
            get => trailSize;
            set => trailSize = Normalize(value, 4, 120, DefaultTrailSize);
        }

        public double TrailHeadOpacity
        {
            get => trailHeadOpacity;
            set => trailHeadOpacity = Normalize(value, 0, 1, DefaultTrailHeadOpacity);
        }

        public double TrailTailOpacity
        {
            get => trailTailOpacity;
            set => trailTailOpacity = Normalize(value, 0, 1, DefaultTrailTailOpacity);
        }

        public double FollowingSize
        {
            get => followingSize;
            set => followingSize = Normalize(value, 10, 120, DefaultFollowingSize);
        }

        public double FollowingDelayMilliseconds
        {
            get => followingDelayMilliseconds;
            set => followingDelayMilliseconds = Normalize(value, 0, MaxFollowingDelayMilliseconds, DefaultFollowingDelayMilliseconds);
        }

        public string FollowingColor
        {
            get => followingColor;
            set => followingColor = IsValidFollowingColor(value) ? value.Trim().ToUpperInvariant() : DefaultFollowingColor;
        }

        public double FollowingCenterOpacity
        {
            get => followingCenterOpacity;
            set => followingCenterOpacity = Normalize(value, 0, 1, DefaultFollowingCenterOpacity);
        }

        public double FollowingEdgeOpacity
        {
            get => followingEdgeOpacity;
            set => followingEdgeOpacity = Normalize(value, 0, 1, DefaultFollowingEdgeOpacity);
        }

        public double FireworksOpacity
        {
            get => fireworksOpacity;
            set => fireworksOpacity = Normalize(value, 0, 1);
        }

        public double OrbitParticlesOpacity
        {
            get => orbitParticlesOpacity;
            set => orbitParticlesOpacity = Normalize(value, 0, 1);
        }

        public double StarlightArrowOpacity
        {
            get => starlightArrowOpacity;
            set => starlightArrowOpacity = Normalize(value, 0, 1);
        }

        public double FlameSizeScale
        {
            get => flameSizeScale;
            set => flameSizeScale = Normalize(value, 0.5, 3, DefaultFlameSizeScale);
        }

        public double FlameRadius
        {
            get => flameRadius;
            set => flameRadius = Normalize(value, 10, 120, DefaultFlameRadius);
        }

        public double FlameSpeed
        {
            get => flameSpeed;
            set => flameSpeed = Normalize(value, 0.25, 3, DefaultFlameSpeed);
        }

        public double FlameOpacity
        {
            get => flameOpacity;
            set => flameOpacity = Normalize(value, 0, 1);
        }

        public double DrizzleOpacity
        {
            get => drizzleOpacity;
            set => drizzleOpacity = Normalize(value, 0, 1);
        }

        public double SwordAuraOpacity
        {
            get => swordAuraOpacity;
            set => swordAuraOpacity = Normalize(value, 0, 1);
        }

        public double SwordAuraSizeScale
        {
            get => swordAuraSizeScale;
            set => swordAuraSizeScale = Normalize(value, 0.5, 3, DefaultSwordAuraSizeScale);
        }

        public string SwordAuraColor1
        {
            get => swordAuraColor1;
            set => swordAuraColor1 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultSwordAuraColor1;
        }

        public string SwordAuraColor2
        {
            get => swordAuraColor2;
            set => swordAuraColor2 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultSwordAuraColor2;
        }

        public string SwordAuraColor3
        {
            get => swordAuraColor3;
            set => swordAuraColor3 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultSwordAuraColor3;
        }

        public string StarlightArrowColor
        {
            get => starlightArrowColor;
            set => starlightArrowColor = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultStarlightArrowColor;
        }

        public int OrbitParticlesLayers
        {
            get => orbitParticlesLayers;
            set => orbitParticlesLayers = Math.Clamp(value, 1, MaxOrbitParticlesLayers);
        }

        public double OrbitParticlesRadius
        {
            get => orbitParticlesRadius;
            set => orbitParticlesRadius = Normalize(value, 10, 120, DefaultOrbitParticlesRadius);
        }

        public double OrbitParticlesSpeed
        {
            get => orbitParticlesSpeed;
            set => orbitParticlesSpeed = Normalize(value, 0.25, 3, DefaultOrbitParticlesSpeed);
        }

        public string OrbitParticlesColor1
        {
            get => orbitParticlesColor1;
            set => orbitParticlesColor1 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultOrbitParticlesColor1;
        }

        public string OrbitParticlesColor2
        {
            get => orbitParticlesColor2;
            set => orbitParticlesColor2 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultOrbitParticlesColor2;
        }

        public string OrbitParticlesColor3
        {
            get => orbitParticlesColor3;
            set => orbitParticlesColor3 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultOrbitParticlesColor3;
        }

        public double WillowCatkinsOpacity
        {
            get => willowCatkinsOpacity;
            set => willowCatkinsOpacity = Normalize(value, 0, 1);
        }

        public string WillowCatkinsColor
        {
            get => willowCatkinsColor;
            set => willowCatkinsColor = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultWillowCatkinsColor;
        }

        public double WillowCatkinsSizeScale
        {
            get => willowCatkinsSizeScale;
            set => willowCatkinsSizeScale = Normalize(value, 0.5, 3, DefaultWillowCatkinsSizeScale);
        }

        public double FireworksExpansion
        {
            get => fireworksExpansion;
            set => fireworksExpansion = Normalize(value, 0.25, 3);
        }

        public double SnowflakesOpacity
        {
            get => snowflakesOpacity;
            set => snowflakesOpacity = Normalize(value, 0, 1);
        }

        public double SnowflakesSizeScale
        {
            get => snowflakesSizeScale;
            set => snowflakesSizeScale = Normalize(value, 0.5, 3, DefaultSnowflakesSizeScale);
        }

        public double FivePointedStarsOpacity
        {
            get => fivePointedStarsOpacity;
            set => fivePointedStarsOpacity = Normalize(value, 0, 1);
        }

        public double FivePointedStarsSizeScale
        {
            get => fivePointedStarsSizeScale;
            set => fivePointedStarsSizeScale = Normalize(value, 0.5, 3, DefaultFivePointedStarsSizeScale);
        }

        public string FivePointedStarsColor
        {
            get => fivePointedStarsColor;
            set => fivePointedStarsColor = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultFivePointedStarsColor;
        }

        public bool FivePointedStarsUseRandomColors { get; set; } = DefaultFivePointedStarsUseRandomColors;

        public double InkFluidOpacity
        {
            get => inkFluidOpacity;
            set => inkFluidOpacity = Normalize(value, 0, 1);
        }

        public double ColorfulMistOpacity
        {
            get => colorfulMistOpacity;
            set => colorfulMistOpacity = Normalize(value, 0, 1);
        }

        public double ColorfulMistSizeScale
        {
            get => colorfulMistSizeScale;
            set => colorfulMistSizeScale = Normalize(value, 0.5, 3, DefaultColorfulMistSizeScale);
        }

        public double FireworksTrailOpacity
        {
            get => fireworksTrailOpacity;
            set => fireworksTrailOpacity = Normalize(value, 0, 1);
        }

        public double FallingLeavesOpacity
        {
            get => fallingLeavesOpacity;
            set => fallingLeavesOpacity = Normalize(value, 0, 1);
        }

        public double FallingLeavesSizeScale
        {
            get => fallingLeavesSizeScale;
            set => fallingLeavesSizeScale = Normalize(value, 0.5, 3, DefaultFallingLeavesSizeScale);
        }

        public double IconRainOpacity
        {
            get => iconRainOpacity;
            set => iconRainOpacity = Normalize(value, 0, 1);
        }

        public double IconRainSizeScale
        {
            get => iconRainSizeScale;
            set => iconRainSizeScale = Normalize(value, 0.5, 3, DefaultIconRainSizeScale);
        }

        public double HaloRingOpacity
        {
            get => haloRingOpacity;
            set => haloRingOpacity = Normalize(value, 0, 1);
        }

        public double HaloRingSizeScale
        {
            get => haloRingSizeScale;
            set => haloRingSizeScale = Normalize(value, 0.5, 3, DefaultHaloRingSizeScale);
        }

        public double HaloRingBreathingFrequency
        {
            get => haloRingBreathingFrequency;
            set => haloRingBreathingFrequency = Normalize(value, 0.1, 2, DefaultHaloRingBreathingFrequency);
        }

        public string HaloRingColor1
        {
            get => haloRingColor1;
            set => haloRingColor1 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultHaloRingColor1;
        }

        public string HaloRingColor2
        {
            get => haloRingColor2;
            set => haloRingColor2 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultHaloRingColor2;
        }

        public string HaloRingColor3
        {
            get => haloRingColor3;
            set => haloRingColor3 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultHaloRingColor3;
        }

        public double FollowingAnimalOpacity
        {
            get => followingAnimalOpacity;
            set => followingAnimalOpacity = Normalize(value, 0, 1);
        }

        public double FollowingAnimalSizeScale
        {
            get => followingAnimalSizeScale;
            set => followingAnimalSizeScale = Normalize(value, 0.5, 3, DefaultFollowingAnimalSizeScale);
        }

        public double FirefliesOpacity
        {
            get => firefliesOpacity;
            set => firefliesOpacity = Normalize(value, 0, 1);
        }

        public double FirefliesFollowSpeed
        {
            get => firefliesFollowSpeed;
            set => firefliesFollowSpeed = Normalize(value, 0.25, 3, DefaultFirefliesFollowSpeed);
        }

        public double PixelBlocksOpacity
        {
            get => pixelBlocksOpacity;
            set => pixelBlocksOpacity = Normalize(value, 0, 1);
        }

        public double ButterfliesOpacity
        {
            get => butterfliesOpacity;
            set => butterfliesOpacity = Normalize(value, 0, 1);
        }

        public double CloudOpacity
        {
            get => cloudOpacity;
            set => cloudOpacity = Normalize(value, 0, 1);
        }

        public double MusicNotesOpacity
        {
            get => musicNotesOpacity;
            set => musicNotesOpacity = Normalize(value, 0, 1);
        }

        public double BinaryTrailOpacity
        {
            get => binaryTrailOpacity;
            set => binaryTrailOpacity = Normalize(value, 0, 1);
        }

        public bool BinaryTrailUseDefaultColors { get; set; } = true;

        public double BinaryTrailSizeScale
        {
            get => binaryTrailSizeScale;
            set => binaryTrailSizeScale = Normalize(value, 0.5, 3, DefaultBinaryTrailSizeScale);
        }

        public string BinaryTrailColor
        {
            get => binaryTrailColor;
            set => binaryTrailColor = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultBinaryTrailColor;
        }

        public string BinaryTrailFontFamily
        {
            get => binaryTrailFontFamily;
            set
            {
                binaryTrailFontFamily = DefaultBinaryTrailFontFamily;
                foreach (var font in BinaryTrailFontFamilies)
                    if (string.Equals(value?.Trim(), font, StringComparison.OrdinalIgnoreCase))
                    {
                        binaryTrailFontFamily = font;
                        break;
                    }
            }
        }

        public double CloudSizeScale
        {
            get => cloudSizeScale;
            set => cloudSizeScale = Normalize(value, 0.5, 3, DefaultCloudSizeScale);
        }

        public string CloudPosition
        {
            get => cloudPosition;
            set => cloudPosition = value is "TopLeft" or "BottomLeft" or "TopRight" or "BottomRight" ? value : DefaultCloudPosition;
        }

        public double CloudHorizontalOffset
        {
            get => cloudHorizontalOffset;
            set => cloudHorizontalOffset = Normalize(value, 0, 300, DefaultCloudHorizontalOffset);
        }

        public double CloudVerticalOffset
        {
            get => cloudVerticalOffset;
            set => cloudVerticalOffset = Normalize(value, 0, 300, DefaultCloudVerticalOffset);
        }

        public double PixelBlocksSizeScale
        {
            get => pixelBlocksSizeScale;
            set => pixelBlocksSizeScale = Normalize(value, 0.5, 3, DefaultPixelBlocksSizeScale);
        }

        public double CottonCandyOpacity
        {
            get => cottonCandyOpacity;
            set => cottonCandyOpacity = Normalize(value, 0, 1);
        }

        public double CottonCandySizeScale
        {
            get => cottonCandySizeScale;
            set => cottonCandySizeScale = Normalize(value, 0.5, 3, DefaultCottonCandySizeScale);
        }

        public double NeonRibbonOpacity
        {
            get => neonRibbonOpacity;
            set => neonRibbonOpacity = Normalize(value, 0, 1);
        }

        public double WaterRippleOpacity
        {
            get => waterRippleOpacity;
            set => waterRippleOpacity = Normalize(value, 0, 1);
        }

        public double WaterRippleSizeScale
        {
            get => waterRippleSizeScale;
            set => waterRippleSizeScale = Normalize(value, 0.5, 3, DefaultWaterRippleSizeScale);
        }

        public double WaterRippleFrequency
        {
            get => waterRippleFrequency;
            set => waterRippleFrequency = Normalize(value, 1, 20, DefaultWaterRippleFrequency);
        }

        public string WaterRippleColor
        {
            get => waterRippleColor;
            set => waterRippleColor = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultWaterRippleColor;
        }

        public double StardustOpacity
        {
            get => stardustOpacity;
            set => stardustOpacity = Normalize(value, 0, 1);
        }

        public double FallingPetalsOpacity
        {
            get => fallingPetalsOpacity;
            set => fallingPetalsOpacity = Normalize(value, 0, 1);
        }

        public double FallingPetalsSizeScale
        {
            get => fallingPetalsSizeScale;
            set => fallingPetalsSizeScale = Normalize(value, 0.5, 3, DefaultFallingPetalsSizeScale);
        }

        public string FallingPetalsColor
        {
            get => fallingPetalsColor;
            set
            {
                fallingPetalsColor = DefaultFallingPetalsColor;
                foreach (var preset in FallingPetalsPresetColors)
                {
                    if (string.Equals(value?.Trim(), preset, StringComparison.OrdinalIgnoreCase))
                    {
                        fallingPetalsColor = preset;
                        break;
                    }
                }
            }
        }

        public double StardustSizeScale
        {
            get => stardustSizeScale;
            set => stardustSizeScale = Normalize(value, 0.5, 3, DefaultStardustSizeScale);
        }

        public string NeonRibbonColor1
        {
            get => neonRibbonColor1;
            set => neonRibbonColor1 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultNeonRibbonColor1;
        }

        public string NeonRibbonColor2
        {
            get => neonRibbonColor2;
            set => neonRibbonColor2 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultNeonRibbonColor2;
        }

        public string NeonRibbonColor3
        {
            get => neonRibbonColor3;
            set => neonRibbonColor3 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultNeonRibbonColor3;
        }

        public string InkFluidColor1
        {
            get => inkFluidColor1;
            set => inkFluidColor1 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultInkFluidColor1;
        }

        public string InkFluidColor2
        {
            get => inkFluidColor2;
            set => inkFluidColor2 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultInkFluidColor2;
        }

        public string InkFluidColor3
        {
            get => inkFluidColor3;
            set => inkFluidColor3 = IsValidColor(value) ? value.Trim().ToUpperInvariant() : DefaultInkFluidColor3;
        }

        public static bool IsValidFollowingColor(string value)
        {
            return IsValidColor(value);
        }

        public static bool IsValidColor(string value)
        {
            value = value?.Trim();
            if (value == null || value.Length != 7 || value[0] != '#')
                return false;

            for (var i = 1; i < value.Length; i++)
            {
                if (!Uri.IsHexDigit(value[i]))
                    return false;
            }
            return true;
        }

        private static double Normalize(double value, double minimum, double maximum, double defaultValue = 1)
        {
            return double.IsFinite(value) ? Math.Clamp(value, minimum, maximum) : defaultValue;
        }
    }
}
