using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Events;

namespace NewFlowChartTool.ViewModels
{
    internal class OutputPanelViewModel : BindableBase
    {
        public OutputPanelViewModel(IEventAggregator ea)
        {
            Outputs = new ObservableCollection<string>();
            Outputs.Add("Test ...");
            _ea = ea;
            Console.WriteLine("OutputPanel init ....");
            ea.GetEvent<Event.ProjectOpenEvent>().Subscribe(project => {
                Console.WriteLine("=================== project Open");
                Outputs.Add("project opend"); }
            , ThreadOption.UIThread);
        }
        public ObservableCollection<string> Outputs { get; set; }

        IEventAggregator _ea;
    }
}
