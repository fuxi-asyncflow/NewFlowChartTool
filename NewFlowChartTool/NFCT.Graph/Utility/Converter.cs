using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using FlowChart.Core;
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Utility
{
    class GraphNodesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextNodeTemplate { get; set; }
        public DataTemplate? StartNodeTemplate { get; set; }
        public DataTemplate? ControlNodeTemplate { get; set; }
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is TextNodeViewModel)
                return TextNodeTemplate;
            if (item is StartNodeViewModel)
                return StartNodeTemplate;
            if(item is ControlNodeViewModel)
                return ControlNodeTemplate;
            return base.SelectTemplate(item, container);
        }
    }

    #region NODE COVERTERS
    [ValueConversion(typeof(ViewModels.BaseNodeViewModel.NodeBgType), typeof(Brush))]

    class NodeBgColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bgType = (int)value;
            return CanvasNodeResource.BackgroundBrushes[bgType];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(ViewModels.BaseNodeViewModel.NodeBgType), typeof(Brush))]
    class NodeBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CanvasNodeResource.SelectedNodeBorderBrush;
            //return CanvasNodeResource.BorderBrushes[bgType];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    class NodeBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? CanvasNodeResource.SelectedBorderWidth : CanvasNodeResource.DefaultBorderWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Connector.ConnectorType), typeof(Brush))]
    class ConnectorColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CanvasNodeResource.LineBrushes[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    class ConnectorStrokeWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 6 : 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region PROMPT CONVERTERS

    [ValueConversion(typeof(PromptItemViewModel.PromptType), typeof(Brush))]
    class PromptColorConverter : IValueConverter
    {
        private static Brush[] _brushes;

        static PromptColorConverter()
        {
            _brushes = new Brush[6];
            _brushes[0] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush;
            _brushes[1] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush; //variable
            _brushes[2] = Application.Current.FindResource("NodeWaitBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush; //event
            _brushes[3] = Application.Current.FindResource("NodeActionBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush; //action
            _brushes[4] = Application.Current.FindResource("NodeConditionBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush; // method
            _brushes[5] = Application.Current.FindResource("NodeWaitBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush; // property
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (PromptItemViewModel.PromptType)value;
            int idx = (int)v;
            return _brushes[idx];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(PromptItemViewModel.PromptType), typeof(string))]
    class PromptIconConverter : IValueConverter
    {
        private static string[] _strs;

        static PromptIconConverter()
        {
            _strs = new string[] {"P", "V", "E", "A", "M", "P"};
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (PromptItemViewModel.PromptType)value;
            int idx = (int)v;
            return _strs[idx];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    class CutOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCut = (bool)value;
            return isCut ? 0.5 : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;
            int i = (int)(d * 100);
            return $"{i}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = (string)value;
            s = s.Trim('%').Trim();
            if (Int32.TryParse(s, out var i))
            {
                if (parameter is double)
                {
                    var minValue = (double)value;
                    return Math.Max(i / 100.0, minValue);
                }
                return i / 100.0;
            }

            return Binding.DoNothing;
        }
    }

    #endregion

}
