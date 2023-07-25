using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class NodePropPanelViewModel : BindableBase
    {
        public NodePropPanelViewModel(GraphPaneViewModel graphVm)
        {
            _graphVm = graphVm;
        }

        public string Uid => _nodeVm?.Node.Uid.ToString() ?? string.Empty;
        public bool IsShow => _nodeVm != null;
        private string? _description;
        public string? Description
        {
            get => _description; 
            set
            {
                SetProperty(ref _description, value);
                if (_nodeVm != null)
                    _nodeVm.Node.Description = value;
            }
        }
        public void SetNode(BaseNodeViewModel? nodeVm)
        {
            _nodeVm = nodeVm;
            _description = nodeVm?.Description;
            RaisePropertyChanged(nameof(Uid));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(IsShow));
        }

        private BaseNodeViewModel? _nodeVm;
        private GraphPaneViewModel _graphVm;
    }
}
