using System;
using System.Collections.Generic;
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
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is TextNodeViewModel)
                return TextNodeTemplate;
            else if (item is StartNodeViewModel)
                return StartNodeTemplate;
            return base.SelectTemplate(item, container);
        }
    }

}
