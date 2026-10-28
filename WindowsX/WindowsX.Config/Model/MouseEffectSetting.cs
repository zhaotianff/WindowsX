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
        public const double DefaultSnowflakesSizeScale = 1;
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
        private double inkFluidOpacity = 1;
        private double neonRibbonOpacity = 1;
        private double waterRippleOpacity = 1;
        private double stardustOpacity = 1;
        private double fallingPetalsOpacity = 1;
        private double colorfulMistOpacity = 1;
        private double fireworksTrailOpacity = 1;
        private double fallingLeavesOpacity = 1;
        private double iconRainOpacity = 1;
        private double haloRingOpacity = 1;
        private double haloRingSizeScale = DefaultHaloRingSizeScale;
        private double haloRingBreathingFrequency = DefaultHaloRingBreathingFrequency;
        private string haloRingColor1 = DefaultHaloRingColor1;
        private string haloRingColor2 = DefaultHaloRingColor2;
        private string haloRingColor3 = DefaultHaloRingColor3;
        private double followingAnimalOpacity = 1;
        private double cottonCandyOpacity = 1;
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

        public double IconRainOpacity
        {
            get => iconRainOpacity;
            set => iconRainOpacity = Normalize(value, 0, 1);
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

        public double CottonCandyOpacity
        {
            get => cottonCandyOpacity;
            set => cottonCandyOpacity = Normalize(value, 0, 1);
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
