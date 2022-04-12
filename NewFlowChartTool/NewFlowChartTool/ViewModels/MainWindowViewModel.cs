using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using FlowChart.Core;

namespace NewFlowChartTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(IEventAggregator ea)
        {
            _testText = "Hello world";
            _ea = ea;

            OpenProjectCommand = new DelegateCommand(OpenProject, () => true);
            OpenProject();
        }



        readonly IEventAggregator _ea;

        public string _testText;
        public string TestText { get => _testText; set { SetProperty<string>(ref _testText, value); } }

        #region COMMAND
        public DelegateCommand OpenProjectCommand { get; private set; }
        #endregion

        public void OpenProject()
        {
            var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            p.Path = @"D:\git\asyncflow_new\test\flowchart";
            p.Load();
            _ea.GetEvent<Event.ProjectOpenEvent>().Publish(p);
        }
    }
}
