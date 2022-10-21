using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    internal class TypeDialogViewModel : BindableBase, IDialogAware
    {
        public static string NAME = "TypeDialog";
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

        public string Title { get; }
        public event Action<IDialogResult>? RequestClose;
    }
}
