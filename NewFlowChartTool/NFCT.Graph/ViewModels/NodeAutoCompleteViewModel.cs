using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class NodeAutoCompleteViewModel : BindableBase
    {
        public TextNodeViewModel Node { get; set; }
        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value, nameof(Text));
        }
    }
}
