using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Mvvm;
using Prism.Events;

namespace NewFlowChartTool.ViewModels
{
    internal class OutputPanelViewModel : BindableBase
    {
        public OutputPanelViewModel()
        {
            Outputs = new ObservableCollection<string>();
            Outputs.Add("Test ...");
            
            Console.WriteLine("OutputPanel init ....");
            EventHelper.Sub<ProjectOpenEvent, Project>(project =>
            {
                Console.WriteLine("=================== project Open");
                Outputs.Add("project opened");
            }, ThreadOption.UIThread);
        }
        public ObservableCollection<string> Outputs { get; set; }
    }
}
