using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlowChart.Core;
using FlowChart.Type;
using Microsoft.Msagl.Core.DataStructures;
using Prism.Commands;
using Prism.Mvvm;
using Type = FlowChart.Type.Type;

namespace NFCT.Graph.ViewModels
{
    public class GraphVariablesPanelViewModel : BindableBase
    {
        static GraphVariablesPanelViewModel()
        {
            TypeNames = new ObservableCollection<Type>();
        }

        public GraphVariablesPanelViewModel(GraphPaneViewModel graphVm)
        {
            _graphVm = graphVm;
            IsEditing = false;
            Variables = new ObservableCollection<GraphVariableViewModel>();
            _graphVm.Graph.Variables.ForEach(v =>
            {
                Variables.Add(new GraphVariableViewModel(v));
            });
            _graphVm.Graph.GraphAddVariableEvent += OnAddVariable;

            AddVariableCommand = new DelegateCommand(AddVariable);
            DelVariableCommand = new DelegateCommand(DelVariable);
            ModifyVariableCommand = new DelegateCommand(ModifyVariable);
            ConfirmCommand = new DelegateCommand(Confirm);
            CancelCommand = new DelegateCommand(Cancel);

            _varDict = new Dictionary<string, GraphVariableViewModel>();

            PrepareTypeNames();
        }

        public GraphPaneViewModel _graphVm;

        public GraphVariableViewModel? SelectedItem { get; set; }
        public static ObservableCollection<Type> TypeNames { get; set; }

        #region Modifying

        private string _tmpName;
        public string TmpName
        {
            get => _tmpName;
            set => SetProperty(ref _tmpName, value);
        }

        private string _tmpTypeName;
        public string TmpTypeName
        {
            get => _tmpTypeName;
            set => SetProperty(ref _tmpTypeName, value);
        }

        private string _tmpDefaultVal;
        public string TmpDefaultVal
        {
            get => _tmpDefaultVal;
            set => SetProperty(ref _tmpDefaultVal, value);
        }

        //private bool _tmpVariadic;

        //public bool TmpIsVariadic
        //{
        //    get => _tmpVariadic;
        //    set => SetProperty(ref _tmpVariadic, value);
        //}

        private bool _tmpIsParameter;
        public bool TmpIsParameter
        {
            get => _tmpIsParameter;
            set => SetProperty(ref _tmpIsParameter, value);
        }

        public bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsAdding { get; set; }

        #endregion

        
        public ObservableCollection<GraphVariableViewModel> Variables { get; set; }

        public DelegateCommand AddVariableCommand { get; set; }

        public void AddVariable()
        {
            IsAdding = true;
            IsEditing = true;
            ResetPara();
        }

        public void OnAddVariable(FlowChart.Core.Graph graph, Variable variable)
        {
            Debug.Assert(_graphVm.Graph == graph);
            Variables.Add(new GraphVariableViewModel(variable));
            SortVariables();
        }

        void SortVariables()
        {
            var variables = Variables.ToList();
            variables.Sort((a, b) =>
            {
                if(a.IsParameter == b.IsParameter)
                    return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
                return a.IsParameter.CompareTo(b.IsParameter);
            });
            var tempSelectItem = SelectedItem;
            Variables.Clear();
            SelectedItem = tempSelectItem;
            variables.ForEach(Variables.Add);
        }

        public DelegateCommand DelVariableCommand { get; set; }

        public void DelVariable()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("please select a variable");
                return;
            }
            _graphVm.Graph.DelVariable(SelectedItem.Name);
            Variables.Remove(SelectedItem);
            SelectedItem = null;
            IsAdding = false;
            IsEditing = false;
        }

        public DelegateCommand ModifyVariableCommand { get; set; }

        public void ModifyVariable()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("please select a variable");
                return;
            }

            IsAdding = false;
            IsEditing = true;
            TmpName = SelectedItem.Name;
            TmpTypeName = SelectedItem.Type;
            TmpDefaultVal = SelectedItem.Variable.DefaultValue;
        }

        public DelegateCommand ConfirmCommand { get; set; }

        public void Confirm()
        {
            if (IsAdding)
            {
                // check name
                if (_graphVm.Graph.Variables.Find(v => v.Name == TmpName) != null)
                {
                    MessageBox.Show("variable with same name exit, please rename the name");
                    return;
                }
                
                //TODO checktype
                var type = _graphVm.Graph.Project.GetType(TmpTypeName);
                if (type == null)
                {
                    MessageBox.Show($"invalid type name {TmpTypeName}");
                    return;
                }

                var variable = _graphVm.Graph.GetOrAddVariable(TmpName);
                variable.DefaultValue = TmpDefaultVal;
                variable.Type = type;

                IsAdding = false;
                IsEditing = false;
            }
            else
            {
                if (SelectedItem == null)
                    return;
                if (SelectedItem.Name != TmpName)
                {
                    SelectedItem.Variable.Name = TmpName;
                    SortVariables();
                    _graphVm.Build();
                }

                if (SelectedItem.Type != TmpTypeName)
                {
                    var tp = _graphVm.Graph.Project.GetType(TmpTypeName);
                    if (tp == null)
                    {
                        MessageBox.Show($"invalid type name {TmpTypeName}");
                        return;
                    }

                    SelectedItem.Variable.Type = tp;
                    _graphVm.Build();
                }

                if (SelectedItem.Value != TmpDefaultVal)
                {
                    SelectedItem.Variable.DefaultValue = TmpDefaultVal;
                    _graphVm.Build();
                }

                if (SelectedItem.IsParameter != TmpIsParameter)
                {
                    SelectedItem.Variable.IsParameter = TmpIsParameter;
                    SortVariables();
                }

                IsEditing = false;
            }
        }
        public DelegateCommand CancelCommand { get; set; }

        public void Cancel()
        {
            IsAdding = false;
            IsEditing = false;
        }

        void ResetPara()
        {
            TmpName = "";
            TmpTypeName = "";
            TmpDefaultVal = "";
            TmpIsParameter = false;
        }

        #region DEBUG

        private Dictionary<string, GraphVariableViewModel> _varDict;

        public void OnEnterDebugMode()
        {
            _varDict.Clear();
            foreach (var varVm in Variables)
            {
                varVm.Value = "-";
                _varDict.Add(varVm.Name, varVm);
            }
        }

        public void OnExitDebugMode()
        {
            foreach (var varVm in Variables)
            {
                varVm.ResetValue();
            }
        }

        public void UpdateDebugValue(string varName, string varValue)
        {
            if (_varDict.TryGetValue(varName, out var vm))
            {
                vm.Value = varValue;
            }
        }



        #endregion

        void PrepareTypeNames()
        {
            var project = _graphVm.Graph.Project;
            var names = new List<Type>();
            foreach (var kv in project.TypeDict)
            {
                names.Add(kv.Value);
            }

            TypeNames.Clear();
            names.Sort((a, b) =>
            {
                var aIsInst = a is InstanceType;
                var bIsInst = b is InstanceType;
                if (aIsInst == bIsInst)
                {
                    if (a.IsBuiltinType == b.IsBuiltinType)
                    {
                        return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
                    }
                    return b.IsBuiltinType.CompareTo(a.IsBuiltinType);
                }

                return aIsInst.CompareTo(bIsInst);
            });
            names.ForEach(TypeNames.Add);
        }
    }
}
