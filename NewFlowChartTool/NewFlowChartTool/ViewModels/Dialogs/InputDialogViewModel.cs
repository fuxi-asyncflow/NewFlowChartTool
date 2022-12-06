using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    public class InputDialogViewModel : BindableBase, IDialogAware
    {
        public InputDialogViewModel()
        {
            Description = "Please input: ";
            Text = "XXXX";
            OKCommand = new DelegateCommand(OK);
            CancelCommand = new DelegateCommand(Cancel);
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
            var des = parameters.GetValue<string?>(nameof(Description));
            if (des != null)
                Description = des;
            var v = parameters.GetValue<string?>("Value");
            if (v != null)
                Text = v;
        }

        public string Title { get; }
        public event Action<IDialogResult>? RequestClose;

        public DelegateCommand OKCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        void OK()
        {
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add("Value", Text);
            RequestClose?.Invoke(result);
        }

        void Cancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }
    }
}
