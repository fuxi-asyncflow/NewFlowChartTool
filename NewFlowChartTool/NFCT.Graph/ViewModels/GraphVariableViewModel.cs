using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Parser.NodeParser;
using Prism.Mvvm;

namespace NFCT.Graph.ViewModels
{
    public class GraphVariableViewModel : BindableBase
    {
        public GraphVariableViewModel(FlowChart.Core.Variable v)
        {
            _variable = v;
            _value = _variable.DefaultValue;
            v.VariableChangeEvent += OnVariableChange;
        }
        private FlowChart.Core.Variable _variable;
        public FlowChart.Core.Variable Variable => _variable;
        public int Id { get; set; }
        public string Name => _variable.Name;

        public string Type => _variable.Type == null ? "" : _variable.Type.Name;
        private string? _value;

        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public void ResetValue()
        {
            Value = _variable.DefaultValue;
        }
        public string? Description => _variable.Description;
        public bool IsParameter => _variable.IsParameter;

        public void OnVariableChange(Variable v)
        {
            Debug.Assert(v == _variable);
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Type));
            _value = _variable.DefaultValue;
            RaisePropertyChanged(nameof(Description));
        }
    }
}
