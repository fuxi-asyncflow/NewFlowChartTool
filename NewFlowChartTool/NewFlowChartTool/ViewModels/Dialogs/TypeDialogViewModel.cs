using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Misc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using FlowChart.Type;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Commands;
using Prism.Events;
using Type = FlowChart.Type.Type;

namespace NewFlowChartTool.ViewModels
{
    class ParameterViewModel : BindableBase
    {
        public ParameterViewModel(Parameter param)
        {
            Model = param;
            Name = param.Name;
            Type = param.Type.Name;
            DefaultValue = param.Default;
        }
        public Parameter Model { get; set; }
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _type;
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        private string? _description;
        public string? Description { get => _description; set => SetProperty(ref _description, value); }
        private string _defaultValue;
        public string DefaultValue { get => _defaultValue; set => SetProperty(ref _defaultValue, value); }
        public void UpdateToModel(Project p)
        {
            if(Model.Name != Name)
                Model.Name = Name;
            if(Model.Description != Description)
                Model.Description = Description;
            if (Model.Default != DefaultValue)
                Model.Default = DefaultValue;
            if (Model.Type.Name != Type)
            {
                var tp = p.GetType(Type);
                if (tp == null)
                {

                }
                else
                {
                    Model.Type = tp;
                }
            }
        }
    }
    class TypeMemberViewModel : BindableBase
    {
        public TypeMemberViewModel(Member member)
        {
            Model = member;
            _name = member.Name;
            _type = member.Type.Name;
            Description = member.Description;
        }
        public Member Model { get; set; }
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _type;
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        private string? _description;
        public string? Description { get => _description; set => SetProperty(ref _description, value); }
        public bool IsVariadic;

        public string? Parameters
        {
            get
            {
                if (Model is Method method)
                    return string.Join(',', method.Parameters.ConvertAll(p => $"{p.Name} : {p.Type.Name}"));
                return null;
            }
        }

        public void OnParameterUpdate() { RaisePropertyChanged(nameof(Parameters));}

        public void UpdateToModel(Project p, Type type)
        {
            if (Model.Name != Name)
            {
                type.RenameMember(Model.Name, Name);
            }

            if (Model.Description != Description)
                Model.Description = Description;
            if (Model.Type.Name != Type)
            {
                var tp = p.GetType(Type);
                if (tp == null)
                {

                }
                else
                {
                    Model.Type = tp;
                }
            }
        }
    }

    class TypeViewModel : BindableBase
    {
        public TypeViewModel(FlowChart.Type.Type type)
        {
            Model = type;
        }
        public FlowChart.Type.Type Model { get; set; }

        public string Name => Model.Name;
        public string? Description => Model.Description;

        public void OnNameChange()
        {
            RaisePropertyChanged(nameof(Name));
        }


    }
    internal class TypeDialogViewModel : BindableBase, IDialogAware
    {
        public static string NAME = "TypeDialog";
        public Project? CurrentProject;

        public TypeDialogViewModel(IEventAggregator _ev, IOutputMessage outputService)
        {
            Types = new ObservableCollection<TypeViewModel>();
            Members = new ObservableCollection<TypeMemberViewModel>();
            Parameters = new ObservableCollection<ParameterViewModel>();

            _memberName = string.Empty;
            _memberType = string.Empty;

            _outputService = outputService;
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);

            SelectedTypeChangeCommand = new DelegateCommand(OnSelectedTypeChange);
            SelectedMemberChangeCommand = new DelegateCommand(OnSelectedMemberChange);
            SelectedParamChangeCommand = new DelegateCommand(OnSelectedParamChange);
            AddNewTypeCommand = new DelegateCommand(AddNewType);
            AddNewPropertyCommand = new DelegateCommand(AddNewProperty);
            RemoveMemberCommand = new DelegateCommand(RemoveMember);
            AddNewMethodCommand = new DelegateCommand(AddNewMethod);

            AddNewParamCommand = new DelegateCommand(AddParameter);
            RemoveParamCommand = new DelegateCommand(RemoveParameter);
            ParamUpCommand = new DelegateCommand(UpParameter);
            ParamDownCommand = new DelegateCommand(DownParameter);

            SaveCommand = new DelegateCommand(Save);
        }

        private readonly IOutputMessage _outputService;
        void Output(string msg) { _outputService.Output(msg); }

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
            var project = parameters.GetValue<Project>("CurrentProject");
            if (project != CurrentProject)
            {
                Types.Clear();
                CurrentProject = project;
            }

            var typeDict = new Dictionary<Type, TypeViewModel>();
            foreach (var typeViewModel in Types)
            {
                typeDict.Add(typeViewModel.Model, typeViewModel);
            }

            foreach (var kv in CurrentProject.TypeDict)
            {
                var typeName = kv.Key;
                var type = kv.Value;

                if(!type.IsBuiltinType && !typeDict.ContainsKey(type))
                    Types.Add(new TypeViewModel(type));
            }
        }

        public string Title => "Type Manager";
        public event Action<IDialogResult>? RequestClose;
        #endregion

        private TypeViewModel? _selectedType;
        public TypeViewModel? SelectedType { get => _selectedType; set => SetProperty(ref _selectedType, value); }
        public ObservableCollection<TypeViewModel> Types { get; set; }
        private TypeMemberViewModel? _selectedMember;
        public TypeMemberViewModel? SelectedMember { get => _selectedMember; set => SetProperty(ref _selectedMember, value); }
        private ParameterViewModel? _selectedParameter;
        public ParameterViewModel? SelectedParameter { get => _selectedParameter; set => SetProperty(ref _selectedParameter, value); }
        public ObservableCollection<TypeMemberViewModel> Members { get; set; }
        public DelegateCommand SelectedTypeChangeCommand { get; set; }
        public DelegateCommand SelectedMemberChangeCommand { get; set; }
        public DelegateCommand SelectedParamChangeCommand { get; set; }
        public DelegateCommand AddNewTypeCommand { get; set; }
        public DelegateCommand AddNewPropertyCommand { get; set; }
        public DelegateCommand AddNewMethodCommand { get; set; }
        public DelegateCommand RemoveMemberCommand { get; set; }
        public DelegateCommand AddNewParamCommand { get; set; }
        public DelegateCommand RemoveParamCommand { get; set; }
        public DelegateCommand ParamUpCommand { get; set; }
        public DelegateCommand ParamDownCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }

        void OnCloseProject(Project project)
        {
            CurrentProject = null;
            Types.Clear();
        }

        void OnSelectedTypeChange()
        {
            if (SelectedType == null)
                return;
            Output($"type change to {SelectedType.Name}");
            Members.Clear();
            SelectedMember = null;
            foreach (var kv in SelectedType.Model.MemberDict)
            {
                Members.Add(new TypeMemberViewModel(kv.Value));
            }

            TypeName = SelectedType.Name;

        }

        #region Type Editor

        private string? _typeName;
        public string? TypeName { get => _typeName; set => SetProperty(ref _typeName, value); }


        #endregion

        #region Member Editor

        private string _memberName;
        public string MemberName { get => _memberName; set => SetProperty(ref _memberName, value); }
        private string? _memberDescription;
        public string? MemberDescription { get => _memberDescription; set => SetProperty(ref _memberDescription, value); }
        private string _memberType;
        public string MemberType { get => _memberType; set => SetProperty(ref _memberType, value); }
        private int _memberKind;
        public int MemberKind { get => _memberKind; set => SetProperty(ref _memberKind, value); }

        public ObservableCollection<ParameterViewModel> Parameters { get; set; }
        private string _paraName;
        public string ParaName { get => _paraName; set => SetProperty(ref _paraName, value); }
        private string? _paraDescription;
        public string? ParaDescription { get => _paraDescription; set => SetProperty(ref _paraDescription, value); }
        private string _paraType;
        public string ParaType { get => _paraType; set => SetProperty(ref _paraType, value); }
        private string? _paraValue;
        public string? ParaValue { get => _paraValue; set => SetProperty(ref _paraValue, value); }

        #endregion

        void OnSelectedMemberChange()
        {
            if (SelectedMember == null)
            {
                MemberName = string.Empty;
                MemberDescription = null;
                MemberType = string.Empty;
                Parameters.Clear();
                MemberKind = -1;
                return;
            }

            var m = SelectedMember;
            MemberName = m.Name;
            MemberDescription = m.Description;
            MemberType = m.Type;
            Parameters.Clear();
            if (m.Model is Method method)
            {
                MemberKind = 2;
                method.Parameters.ForEach(p =>
                {
                    var pvm = new ParameterViewModel(p)
                    {
                        Description = p.Description,
                        DefaultValue = p.Default
                    };
                    Parameters.Add(pvm);
                });
            }
            else
            {
                MemberKind = 1;
            }
        }

        void OnSelectedParamChange()
        {
            if (SelectedParameter == null)
            {
                ParaName = string.Empty;
                ParaDescription = null;
                ParaValue = null;
                ParaType = string.Empty;
                return;
            }

            ParaName = SelectedParameter.Name;
            ParaDescription = SelectedParameter.Description;
            ParaType = SelectedParameter.Type;
            ParaValue = SelectedParameter.DefaultValue;
        }

        string? GetNewTypeName()
        {
            if (CurrentProject == null)
                return null;
            const string NEW_TYPE_NAME = "NewType";
            var name = NEW_TYPE_NAME;
            if (CurrentProject.GetType(name) == null)
                return name;
            for (int i = 0; i < 100; i++)
            {
                name = $"{NEW_TYPE_NAME}_{i}";
                if (CurrentProject.GetType(name) == null)
                    return name;
            }
            return null;
        }

        void AddNewType()
        {
            var newTypeName = GetNewTypeName();
            if (newTypeName == null)
                return;
            var newType = new Type(newTypeName);
            var newTypeVm = new TypeViewModel(newType);
            Types.Add(newTypeVm);
            SelectedType = newTypeVm;
            OnSelectedTypeChange();
        }

        string? GetNewMemberName(string defaultNewName)
        {
            if (SelectedType == null)
                return null;
            
            var name = defaultNewName;
            if (Members.All(m => m.Name != name))
                return name;
            for (int i = 0; i < 100; i++)
            {
                name = $"{defaultNewName}_{i}";
                if (Members.All(m => m.Name != name))
                    return name;
            }
            return null;
        }

        void AddNewProperty()
        {
            if (SelectedType == null)
                return;
            var name = GetNewMemberName("NewProperty");
            if (name == null)
                return;
            var prop = new Property(name);
            var memberVm = new TypeMemberViewModel(prop);
            Debug.Assert(SelectedType.Model.AddMember(prop));
            Members.Add(memberVm);
            SelectedMember = memberVm;
        }

        void AddNewMethod()
        {
            if (SelectedType == null)
                return;
            var name = GetNewMemberName("NewMethod");
            if (name == null)
                return;
            var method = new Method(name);
            var memberVm = new TypeMemberViewModel(method);
            Debug.Assert(SelectedType.Model.AddMember(method));
            Members.Add(memberVm);
            SelectedMember = memberVm;
        }

        void RemoveMember()
        {
            if (SelectedType == null)
                return;
            if (SelectedMember == null)
                return;
            if (SelectedType.Model.RemoveMember(SelectedMember.Name))
            {
                Members.Remove(SelectedMember);
                SelectedMember = null;
            }
        }

        void AddParameter()
        {
            if (SelectedMember is not { Model: Method method })
                return;
            var p = new Parameter("NewArg") { Type = BuiltinTypes.AnyType };
            method.Parameters.Add(p);
            var paraVm = new ParameterViewModel(p);
            Parameters.Add(paraVm);
            SelectedParameter = paraVm;
        }

        void RemoveParameter()
        {

        }

        void UpParameter()
        {

        }

        void DownParameter()
        {

        }

        void Save()
        {
            if (CurrentProject == null)
                return;

            // save param
            if (SelectedParameter != null)
            {
                SelectedParameter.Name = ParaName;
                SelectedParameter.Description = ParaDescription;
                SelectedParameter.DefaultValue = ParaValue;
                SelectedParameter.Type = ParaType;
                SelectedParameter.UpdateToModel(CurrentProject);
                SelectedMember?.OnParameterUpdate();
            }

            // save member
            if (SelectedMember != null)
            {
                SelectedMember.Name = MemberName;
                SelectedMember.Description = MemberDescription;
                SelectedMember.Type = MemberType;
                if(SelectedType != null)
                    SelectedMember.UpdateToModel(CurrentProject, SelectedType.Model);
            }

            // type rename
            if (SelectedType != null && !string.IsNullOrEmpty(TypeName))
            {
                if (SelectedType.Name != TypeName)
                {
                    CurrentProject.RenameType(SelectedType.Name, TypeName);
                    SelectedType.OnNameChange();
                }
            }

            // save type
            foreach (var typeVm in Types)
            {
                if(CurrentProject.GetType(typeVm.Name) != null)
                    continue;
                CurrentProject.AddType(typeVm.Model);
                Output($"type {typeVm.Name} successfully added");
            }
            CurrentProject.Save();
        }
    }
}
