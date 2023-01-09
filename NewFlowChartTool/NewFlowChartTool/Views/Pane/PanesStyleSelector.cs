using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NewFlowChartTool.Views;
using NewFlowChartTool.ViewModels;
using NFCT.Graph.ViewModels;
using NFCT.Diff.ViewModels;

namespace NewFlowChartTool.Views.Pane
{
    class PanesStyleSelector : StyleSelector
    {
        public Style? ProjectPanelStyle { get; set; }
        public Style? OutputPanelStyle { get; set; }
        public Style? GraphPaneStyle { get; set; }
        public Style? TypePanelStyle { get; set; }

        public override System.Windows.Style? SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is ProjectPanelViewModel)
                return ProjectPanelStyle;

            if (item is OutputPanelViewModel)
                return OutputPanelStyle;

            if (item is GraphPaneViewModel || item is VersionControlPanelViewModel)
                return GraphPaneStyle;
            if (item is TypePanelViewModel)
                return TypePanelStyle;

            return base.SelectStyle(item, container);
        }
    }

    class PanesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? GraphViewTemplate { get; set; }
        public DataTemplate? VersionControlPanelTemplate { get; set; }
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is GraphPaneViewModel)
                return GraphViewTemplate;
            if (item is VersionControlPanelViewModel)
                return VersionControlPanelTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
