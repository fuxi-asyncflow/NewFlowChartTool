using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Description { get; set; }
        public string? DefaultValue { get; set; }
    }
    class TypeMemberViewModel : BindableBase
    {
        public TypeMemberViewModel(Member member)
        {
            Model = member;
        }
        public Member Model { get; set; }
        public string Name => Model.Name;
        public string Type => Model.Type.Name;
        public string? Description => Model.Description;
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
    }

    class TypeViewModel : BindableBase
    {
        public TypeViewModel(FlowChart.Type.Type type)
        {
            Model = type;
            Members = new ObservableCollection<TypeViewModel>();
        }

        public FlowChart.Type.Type Model { get; set; }

        public ObservableCollection<TypeViewModel> Members { get; set; }
        public string Name => Model.Name;
        public string? Description => Model.Description;

        public void OnAddMember(Member member)
        {
            if (member is Method method)
            {

            }
            else if (member is Property property)
            {

            }
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
            _outputService = outputService;
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);

            SelectedTypeChangeCommand = new DelegateCommand(OnSelectedTypeChange);
            SelectedMemberChangeCommand = new DelegateCommand(OnSelectedMemberChange);
            AddNewTypeCommand = new DelegateCommand(AddNewType);
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
        public TypeMemberViewModel? SelectedMember { get; set; }
        public ObservableCollection<TypeMemberViewModel> Members { get; set; }
        public DelegateCommand SelectedTypeChangeCommand { get; set; }
        public DelegateCommand SelectedMemberChangeCommand { get; set; }
        public DelegateCommand AddNewTypeCommand { get; set; }
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
            
        }

        #region Member Editor

        private string _memberName;
        public string MemberName { get => _memberName; set => SetProperty(ref _memberName, value); }
        private string? _memberDescription;
        public string? MemberDescription { get => _memberDescription; set => SetProperty(ref _memberDescription, value); }
        private string _memberType;
        public string MemberType { get => _memberType; set => SetProperty(ref _memberType, value); }

        public ObservableCollection<ParameterViewModel> Parameters { get; set; }

        #endregion

        void OnSelectedMemberChange()
        {
            if (SelectedMember == null)
            {
                MemberName = string.Empty;
                MemberDescription = null;
                MemberType = string.Empty;
                Parameters.Clear();
                return;
            }

            var m = SelectedMember;
            MemberName = m.Name;
            MemberDescription = m.Description;
            MemberType = m.Type;
            Parameters.Clear();
            if (m.Model is Method method)
            {
                method.Parameters.ForEach(p =>
                {
                    var pvm = new ParameterViewModel
                    {
                        Name = m.Name,
                        Description = m.Description,
                        DefaultValue = p.Default
                    };
                    Parameters.Add(pvm);
                });
            }
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

        void Save()
        {
            if (CurrentProject == null)
                return;
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
