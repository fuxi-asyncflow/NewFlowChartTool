using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    public class InputDialogViewModel : BindableBase, IDialogAware
    {
        public InputDialogViewModel()
        {
            Description = "This is an Description: ";
            Text = "XXXX";
        }
        public static string NAME = "InputDialog";
        string _text;
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        private string _description;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool CanCloseDialog()
        {
            //throw new NotImplementedException();
            return true;
        }

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //throw new NotImplementedException();
        }

        public string Title { get; }
        public event Action<IDialogResult>? RequestClose;
    }
}
