using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    public class DialogButton
    {
        public DialogButton(string text, ButtonResult result)
        {
            Text = text;
            Result = result;
        }

        public string Text { get; set; }
        public ButtonResult Result { get; set; }
    }

    public class DialogButtonViewModel : BindableBase
    {
        public DialogButtonViewModel(DialogButton button)
        {
            _button = button;
        }

        public string Text => _button.Text;
        public DelegateCommand? Command { get; set; }
        private DialogButton _button;
    }

    internal class CustomMessageDialogViewModel : BindableBase, IDialogAware
    {
        public static string NAME = nameof(CustomMessageDialogViewModel);
        #region IDialogAware

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var buttons = parameters.GetValue<List<DialogButton>>("buttons");
            if (buttons != null)
            {
                if (Buttons == null)
                    Buttons = new List<DialogButtonViewModel>();
                Buttons.Clear();
                buttons.ForEach(button =>
                {
                    Buttons.Add(new DialogButtonViewModel(button)
                    {
                        Command = new DelegateCommand(delegate { RequestClose?.Invoke(new DialogResult(button.Result)); })
                    });
                });
            }

            var message = parameters.GetValue<string>("message");
            if (!string.IsNullOrEmpty(message))
                Message = message;

            var title = parameters.GetValue<string>("title");
            if (!string.IsNullOrEmpty(title))
                _title = title;
        }

        public string Title => _title ?? "Dialog";
        public event Action<IDialogResult>? RequestClose;

        #endregion

        public List<DialogButtonViewModel>? Buttons { get; set; }
        public string? Message { get; set; }

        private string? _title;

    }
}
