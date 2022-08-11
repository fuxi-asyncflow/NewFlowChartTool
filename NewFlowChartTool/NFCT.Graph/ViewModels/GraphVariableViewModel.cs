using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Parser.NodeParser;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class GraphVariableViewModel : BindableBase
    {
        public GraphVariableViewModel(FlowChart.Core.Variable v)
        {
            _variable = v;
        }
        private FlowChart.Core.Variable _variable;
        public int Id { get; set; }
        public string Name => _variable.Name;

        public string Type => _variable.Type == null ? "" : _variable.Type.Name;
        public string Value => _variable.DefaultValue;
        public string? Description => _variable.Description;
    }
}
