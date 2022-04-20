using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using FlowChart.Core;

namespace NewFlowChartTool.Event
{
    class ProjectOpenEvent : PubSubEvent<Project> { }

    class GraphOpenEvent : PubSubEvent<Graph> { }

    class GraphOpenedEvent : PubSubEvent<Graph> { }
    
}
