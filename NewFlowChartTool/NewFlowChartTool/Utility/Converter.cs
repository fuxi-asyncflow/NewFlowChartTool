using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using NewFlowChartTool.ViewModels;
using NFCT.Graph.ViewModels;

namespace NewFlowChartTool.Utility.Converter
{
    internal class ActiveGraphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GraphPaneViewModel)
                return value;
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GraphPaneViewModel)
                return value;
            return Binding.DoNothing;
        }
    }
}
