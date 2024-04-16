using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WindowsX.Shell.Util
{
    public static class TreeHelper
    {
        public static T FindParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            return obj.GetAncestors().OfType<T>().FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element)
        {
            do
            {
                yield return element;
                element = VisualTreeHelper.GetParent(element);
            } while (element != null);
        }
    }
}
