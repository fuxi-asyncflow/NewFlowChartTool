using System;
using System.Windows;
using System.Windows.Media;
using Prism.Mvvm;

namespace NFCT.Common
{
    public static class WPFHelper
    {
        public static T? GetDataContext<T>(object obj) where T : BindableBase
        {
            return (obj is FrameworkElement) ? ((FrameworkElement)obj).DataContext as T : null;
        }

        public static T? GetVisualParent<T>(object obj)
            where T : DependencyObject
        {
            var depo = obj as DependencyObject;
            while (!(depo == null || depo is T))
            {
                depo = VisualTreeHelper.GetParent(depo);
            }
            return (depo as T);
        }
    }
}
