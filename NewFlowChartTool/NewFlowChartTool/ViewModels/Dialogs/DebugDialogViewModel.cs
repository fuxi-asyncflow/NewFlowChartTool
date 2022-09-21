using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Debug;
using FlowChart.Debug.WebSocket;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    class DebugDialogViewModel : BindableBase, IDialogAware
    {
        public DebugDialogViewModel(IDialogService dialogService)
        {
            Test = "Hello world";
            _dialogService = dialogService;
            CloseCommand = new DelegateCommand(() => { RequestClose.Invoke(new DialogResult(ButtonResult.OK)); });
            GetGraphListCommand = new DelegateCommand(GetGraphList);

            _netManager = new FlowChart.Debug.WebSocket.Manager();
        }

        private IDialogService _dialogService;

        public const string NAME = "DebugDialog";
        public string Test { get; set; }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }

        public string Title => "Debug Dialog";
        public event Action<IDialogResult>? RequestClose;

        public DelegateCommand CloseCommand { get; set;}
        public DelegateCommand GetGraphListCommand { get; set; }
        private INetManager _netManager;

        public void GetGraphList()
        {
            var host = "127.0.0.1";
            int start = 9000;
            int end = 9003;

            _netManager.BroadCast(host, start, end, new GetChartListMessage(){ChartName = "", ObjectName = ""});
        }
    }
}
