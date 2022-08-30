using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FlowChart.Type;
using NewFlowChartTool.ViewModels;
using NFCT.Graph.ViewModels;
using Type = System.Type;

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

    internal class TypeMemberIconConverter : IValueConverter
    {
        private Viewbox ClassIcon => Application.Current.FindResource("Icon_Class") as Viewbox;
        private Viewbox MethodIcon => Application.Current.FindResource("Icon_Method") as Viewbox;
        private Viewbox PropertyIcon => Application.Current.FindResource("Icon_Field") as Viewbox;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TypeMemberTreeItemViewModel vm)
            {
                var tp = vm.MemberType;
                if (tp == TypeMemberTreeItemViewModel.TypeMemberType.Method)
                    return MethodIcon;
                else if(tp == TypeMemberTreeItemViewModel.TypeMemberType.Property)
                    return PropertyIcon;
            }
            return ClassIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
