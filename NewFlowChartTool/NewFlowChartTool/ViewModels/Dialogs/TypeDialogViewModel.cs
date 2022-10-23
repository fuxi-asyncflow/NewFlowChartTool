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
            _outputService = outputService;
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);

            SelectedTypeChangeCommand = new DelegateCommand(OnSelectedTypeChange);
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

        public TypeViewModel? SelectedType { get; set; }
        public ObservableCollection<TypeViewModel> Types { get; set; }
        public TypeMemberViewModel SelectedMember { get; set; }
        public ObservableCollection<TypeMemberViewModel> Members { get; set; }
        public DelegateCommand SelectedTypeChangeCommand { get; set; }


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
            foreach (var kv in SelectedType.Model.MemberDict)
            {
                Members.Add(new TypeMemberViewModel(kv.Value));
            }
            
        }

    }
}
