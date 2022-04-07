using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;

namespace NewFlowChartTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {
            TestText = "Hello World";
        }

        public string _testText;
        public string TestText { get => _testText; set { SetProperty<string>(ref _testText, value); } }
    }
}
