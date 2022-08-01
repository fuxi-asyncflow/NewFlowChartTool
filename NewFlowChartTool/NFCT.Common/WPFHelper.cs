using System;
using System.Windows;
using Prism.Mvvm;

namespace NFCT.Common
{
    public static class WPFHelper
    {
        public static T? GetDataContext<T>(object obj) where T : BindableBase
        {
            return (obj is FrameworkElement) ? ((FrameworkElement)obj).DataContext as T : null;
        }
    }
}
