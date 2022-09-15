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
            else if (item is StartNodeViewModel)
                return StartNodeTemplate;
            else if(item is ControlNodeViewModel)
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
            var bgType = (int)value;
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
            _brushes[0] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush;
            _brushes[1] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush; //variable
            _brushes[2] = Application.Current.FindResource("NodeWaitBackGround") as SolidColorBrush; //event
            _brushes[3] = Application.Current.FindResource("NodeActionBackGround") as SolidColorBrush; //action
            _brushes[4] = Application.Current.FindResource("NodeConditionBackGround") as SolidColorBrush; // method
            _brushes[5] = Application.Current.FindResource("NodeWaitBackGround") as SolidColorBrush; // property
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


    #endregion

}
