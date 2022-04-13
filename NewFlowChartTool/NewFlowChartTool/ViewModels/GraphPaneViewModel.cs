using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Core;

namespace NewFlowChartTool.ViewModels
{
    public class GraphPaneViewModel : BindableBase
    {
        private Graph _graph;
        public GraphPaneViewModel(Graph graph)
        {
            _graph = graph;
        }

    }
}
