using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Windows;

namespace NewFlowChartTool.Utility
{
    static class WPFHelper
    {
        public static T? GetDataContext<T>(object obj) where T : BindableBase
        {            
            return (obj is FrameworkElement) ? ((FrameworkElement)obj).DataContext as T : null;
        }
    }
}
