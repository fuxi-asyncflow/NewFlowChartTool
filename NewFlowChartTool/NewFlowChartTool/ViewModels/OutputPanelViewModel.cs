using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using FlowChart.Common;
using FlowChart.Core;
using FlowChart.Misc;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Mvvm;
using Prism.Events;

namespace NewFlowChartTool.ViewModels
{
    public class OutputItemViewModel : BindableBase
    {
        public OutputItemViewModel()
        {
            TimeStr = DateTime.Now.ToString("HH:mm:ss.fff");
        }

        public OutputMessageType MessageType { get; set; }
        public string Message { get; set; }

        public Node? Node { get; set; }
        public Graph? Graph { get; set; }
        public string TimeStr { get; set; }

        public string? NodeStr
        {
            get
            {
                if (Node != null)
                    return $"{Node.OwnerGraph.Name}[{Node.Id}]";
                if(Graph != null)
                    return Graph.Name;
                return null;
            }
        }
    }


    public class OutputPanelViewModel : BindableBase, IOutputMessage
    {
        public OutputPanelViewModel()
        {
            Outputs = new ObservableCollection<OutputItemViewModel>();
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Output("Test ...");
                Output("Test Error ...", OutputMessageType.Error);
                Output("Test Warning ...", OutputMessageType.Warning);
            }
#endif

            EventHelper.Sub<ProjectOpenEvent, Project>(project =>
            {
                Output("project opened");
            }, ThreadOption.UIThread);

            OutputMessage.Inst = this;
            _inst = this;
            ScrollToEnd = true;
            Logger.OnWarnEvent += msg => Output(msg, OutputMessageType.Warning);
            Logger.OnErrEvent += msg => Output(msg, OutputMessageType.Error);
        }

        private static OutputPanelViewModel? _inst;

        private bool _scrollToEnd;
        public bool ScrollToEnd
        {
            get => _scrollToEnd;
            set => SetProperty(ref _scrollToEnd, value);
        }

        public ObservableCollection<OutputItemViewModel> Outputs { get; set; }

        public static void OutputMsg(string msg, OutputMessageType msgType =
                OutputMessageType.Default
            , Node? node = null, Graph? graph = null)
        {
            _inst?.Output(msg, msgType, node, graph);
        }

        public void Output(string msg, OutputMessageType msgType =
                OutputMessageType.Default
            , Node? node = null, Graph? graph = null)
        {
            var message = new OutputItemViewModel()
            {
                Message = msg,
                MessageType = msgType,
                Node = node,
                Graph = graph
            };
            InvokeIfNecessary(delegate
            {
                Outputs.Add(message);
                if(ScrollToEnd)
                    EventHelper.Pub<ScrollOutputToEndEvent>();
            });
        }

        public void Clear()
        {
            InvokeIfNecessary(delegate { Outputs.Clear(); });
        }

        public static void InvokeIfNecessary(Action action)
        {
            if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
                action();
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }
    }
}
