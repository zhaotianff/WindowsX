using Master_Zhao.Config.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls
{
    public class PathButton : Button
    {
        public static readonly DependencyProperty FastRunIconProperty = DependencyProperty.Register("FastRunIcon", typeof(string), typeof(Button));
        public static readonly DependencyProperty FastRunNameProperty = DependencyProperty.Register("FastRunName", typeof(string), typeof(Button));
        public static readonly DependencyProperty FastRunDataProperty = DependencyProperty.Register("FastRunData", typeof(FastRunItem), typeof(Button));

        public string FastRunIcon
        {
            get => GetValue(FastRunIconProperty).ToString();
            set => SetValue(FastRunIconProperty, value);
        }

        public string FastRunName
        {
            get => GetValue(FastRunNameProperty).ToString();
            set => SetValue(FastRunNameProperty, value);
        }

        public FastRunItem FastRunData
        {
            get => GetValue(FastRunDataProperty) as FastRunItem;
            set => SetValue(FastRunDataProperty, value);
        }

        static PathButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathButton), new FrameworkPropertyMetadata(typeof(PathButton)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.MouseEnter += PathButton_MouseEnter;
            this.MouseLeave += PathButton_MouseLeave;
            this.MouseDown += PathButton_MouseDown;
        }

        private void PathButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }


        private void PathButton_MouseLeave(object sender, MouseEventArgs e)
        {
            //this.Opacity = 1;
            this.Background = Brushes.White;
            this.Effect = null;
        }

        private void PathButton_MouseEnter(object sender, MouseEventArgs e)
        {
            //this.Opacity = 0.5;
            this.Background = Brushes.MediumPurple;
            this.Effect = new DropShadowEffect() { Color = Colors.Purple };
        }
    }
}
