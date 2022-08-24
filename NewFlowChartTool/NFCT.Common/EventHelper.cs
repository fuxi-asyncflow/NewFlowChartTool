﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using Prism.Events;
using Prism.Ioc;

namespace NFCT.Common
{
    public static class EventHelper
    {
        static EventHelper()
        {
            ea = ContainerLocator.Current.Resolve<IEventAggregator>();
        }

        public static void Pub<TEvent, TArg>(TArg eventArg) where TEvent : PubSubEvent<TArg>, new()
        {
            ea.GetEvent<TEvent>().Publish(eventArg);
        }

        public static void Sub<TEvent, TArg>(Action<TArg> cb) where TEvent : PubSubEvent<TArg>, new()
        {
            ea.GetEvent<TEvent>().Subscribe(cb);
        }

        public static void Sub<TEvent, TArg>(Action<TArg> cb, ThreadOption to) where TEvent : PubSubEvent<TArg>, new()
        {
            ea.GetEvent<TEvent>().Subscribe(cb, to);
        }

        private static IEventAggregator ea;
    }
}

namespace NFCT.Common.Events
{
    public class ProjectOpenEvent : PubSubEvent<Project> { }

    public class ProjectCloseEvent : PubSubEvent<Project> { }

    public class GraphOpenEvent : PubSubEvent<Graph> { }

    public class GraphOpenedEvent : PubSubEvent<Graph> { }

    public class GraphCloseEvent : PubSubEvent<Graph> { }

    public class ThemeSwitchEvent : PubSubEvent<NFCT.Common.Theme> { }
}