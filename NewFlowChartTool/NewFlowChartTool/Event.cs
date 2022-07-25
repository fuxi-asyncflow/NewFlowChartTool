using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using FlowChart.Core;
using NLog.LayoutRenderers.Wrappers;
using Prism.Ioc;

namespace NewFlowChartTool.Event
{
    static class EventHelper
    {
        static EventHelper()
        {
            ea = ContainerLocator.Current.Resolve<IEventAggregator>();
        }

        public static void Pub<TEvent, TArg>(TArg eventArg) where TEvent : PubSubEvent<TArg>, new()
        {
            ea.GetEvent<TEvent>().Publish(eventArg);
        }

        private static IEventAggregator ea;

    }

    class ProjectOpenEvent : PubSubEvent<Project> { }

    class GraphOpenEvent : PubSubEvent<Graph> { }

    class GraphOpenedEvent : PubSubEvent<Graph> { }
    
}
