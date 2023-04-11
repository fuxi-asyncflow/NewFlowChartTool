using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    internal class CustomDialogViewModel : BindableBase, IDialogAware
    {
        public CustomDialogViewModel()
        {
            Title = NAME;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("name"))
            {
                var name = parameters.GetValue<string>("name");
                ControlChangeEvent?.Invoke(name);
            }
        }

        public static string NAME = "CustomDialog";
        public string Title { get; set; }
        public event Action<IDialogResult>? RequestClose;
        public event Action<string>? ControlChangeEvent;
    }
}
