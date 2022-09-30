using Master_Zhao.Config.Model;
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
            this.Stroke = Brushes.Black;
            this.StrokeThickness = 1;
            dropShadowEffect.Color = Colors.Green;
            this.Effect = dropShadowEffect;
            dropShadowEffect.Color = Colors.Transparent;
            this.Stretch = Stretch.Fill;
        }

        public void StartMouseEnterAnimation()
        {
            this.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetAnimation(Colors.Purple, TimeSpan.FromMilliseconds(200)));
            this.dropShadowEffect.BeginAnimation(DropShadowEffect.ColorProperty, GetAnimation(Colors.Red, TimeSpan.FromMilliseconds(200)));
        }

        public void StartMouseLeaveAnimation()
        {
            this.Fill.BeginAnimation(SolidColorBrush.ColorProperty, GetAnimation(Colors.White, TimeSpan.FromMilliseconds(200)));
            this.dropShadowEffect.BeginAnimation(DropShadowEffect.ColorProperty, GetAnimation(Colors.Transparent, TimeSpan.FromMilliseconds(200)));
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
