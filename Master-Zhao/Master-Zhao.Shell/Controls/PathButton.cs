using Master_Zhao.Config.Model;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls
{
    public class PathButton : Shape
    {
        public static readonly DependencyProperty GeometryDataProperty = DependencyProperty.Register("GeometryData", typeof(Geometry), typeof(Shape));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(Shape),new PropertyMetadata());
        private DropShadowEffect dropShadowEffect = new DropShadowEffect();

        public FastRunItem FastRunItem { get; set; }

        public Geometry GeometryData
        {
            get => GetValue(GeometryDataProperty) as Geometry;
            set => SetValue(GeometryDataProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pathButton = d as PathButton;

            if(pathButton != null)
            {
                if((bool)e.NewValue == true)
                {
                    pathButton.StartMouseEnterAnimation();
                }
                else
                {
                    pathButton.StartMouseLeaveAnimation();
                }
            }
        }

        protected override Geometry DefiningGeometry => GeometryData;

        public PathButton()
        {
            this.MouseEnter += PathButton_MouseEnter;
            this.MouseLeave += PathButton_MouseLeave;
            this.Fill = new SolidColorBrush(Colors.White);
            this.Stroke = new SolidColorBrush(Colors.Transparent);
            this.StrokeThickness = 1;
            dropShadowEffect.Color = Colors.Transparent;
            this.Effect = dropShadowEffect;
            this.Stretch = Stretch.Fill;
        }

        public void StartMouseEnterAnimation()
        {
            this.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetAnimation(GlobalData.Instance.AccentColorBrushTran.Color, TimeSpan.FromMilliseconds(300)));
            this.dropShadowEffect.BeginAnimation(DropShadowEffect.ColorProperty, GetAnimation(GlobalData.Instance.AccentColorBrushShadow.Color, TimeSpan.FromMilliseconds(300)));
        }

        public void StartMouseLeaveAnimation()
        {
            this.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetAnimation(GlobalData.Instance.WhiteColorBrush.Color, TimeSpan.FromMilliseconds(300)));
            this.dropShadowEffect.BeginAnimation(DropShadowEffect.ColorProperty, GetAnimation(Colors.Transparent, TimeSpan.FromMilliseconds(300)));
        }

        private void PathButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StartMouseEnterAnimation();
        }

        private void PathButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StartMouseLeaveAnimation();
        }

        private ColorAnimation GetAnimation(Color to,TimeSpan duration)
        {
            ColorAnimation animation = new ColorAnimation();
            animation.Duration = duration;
            animation.To = to;
            animation.AutoReverse = false;
            return animation;      
        }
    }
}
