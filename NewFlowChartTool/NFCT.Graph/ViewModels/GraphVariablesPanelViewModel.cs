using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.DataStructures;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class GraphVariablesPanelViewModel : BindableBase
    {
        public GraphVariablesPanelViewModel(GraphPaneViewModel graphVm)
        {
            _graphVm = graphVm;
            IsEditing = false;
            Variables = new ObservableCollection<GraphVariableViewModel>();
            _graphVm.Graph.Variables.ForEach(v =>
            {
                Variables.Add(new GraphVariableViewModel(v));
            });
        }

        public GraphPaneViewModel _graphVm;
        public bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
        public ObservableCollection<GraphVariableViewModel> Variables { get; set; }
    }
}
