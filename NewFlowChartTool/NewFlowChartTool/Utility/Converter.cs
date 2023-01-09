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
using System.Windows.Media;
using FlowChart.Misc;
using FlowChart.Type;
using NewFlowChartTool.ViewModels;
using NFCT.Common;
using NFCT.Graph.ViewModels;
using Prism.Mvvm;
using Type = System.Type;

namespace NewFlowChartTool.Utility.Converter
{
    internal class ActiveGraphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindableBase)
                return value;
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindableBase)
                return value;
            return Binding.DoNothing;
        }
    }

    internal class TypeMemberIconConverter : IValueConverter
    {
        private Viewbox? ClassIcon => Application.Current.FindResource("Icon_Class") as Viewbox;
        private Viewbox? MethodIcon => Application.Current.FindResource("Icon_Method") as Viewbox;
        private Viewbox? PropertyIcon => Application.Current.FindResource("Icon_Field") as Viewbox;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public class OutputMessageColorConverter : IValueConverter
    {
        public static SolidColorBrush ErrorMessageBrush;
        public static SolidColorBrush WarningMessageBrush;
        public static SolidColorBrush DefaultMessageBrush;

        static OutputMessageColorConverter()
        {
            ErrorMessageBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            DefaultMessageBrush = (SolidColorBrush)Application.Current.FindResource("ForeGroundBrush");
            //WarningMessageBrush = (SolidColorBrush)Application.Current.FindResource("light-yellow");
            WarningMessageBrush = Brushes.DarkGoldenrod;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (OutputMessageType)value;
            if (type == OutputMessageType.Error) return ErrorMessageBrush;
            else if (type == OutputMessageType.Warning) return WarningMessageBrush;
            return DefaultMessageBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
