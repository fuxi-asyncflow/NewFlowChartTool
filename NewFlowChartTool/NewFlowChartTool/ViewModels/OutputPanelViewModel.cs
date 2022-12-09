using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        }

        public ObservableCollection<OutputItemViewModel> Outputs { get; set; }

        public void Output(string msg, OutputMessageType msgType =
                OutputMessageType.Default
            , Node? node = null, Graph? graph = null)
        {
            Outputs.Add(new OutputItemViewModel()
            {
                Message = msg,
                MessageType = msgType,
                Node = node,
                Graph = graph
            });
        }

        public void Clear()
        {
            Outputs.Clear();
        }
    }
}
