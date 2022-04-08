using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NewFlowChartTool.Views;
using NewFlowChartTool.ViewModels;

namespace NewFlowChartTool.Views.Pane
{
    class PanesStyleSelector : StyleSelector
    {
        public Style ProjectPanelStyle { get; set; }
        public Style OutputPanelStyle { get; set; }
        public Style RecentFilesStyle { get; set; }

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is ProjectPanelViewModel)
                return ProjectPanelStyle;

            if (item is OutputPanelViewModel)
                return OutputPanelStyle;

            //if (item is FileViewModel)
            //    return FileStyle;

            return base.SelectStyle(item, container);
        }
    }
}
