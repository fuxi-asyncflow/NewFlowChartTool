using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using FlowChart.Type;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Events;
using Type = FlowChart.Type.Type;

namespace NewFlowChartTool.ViewModels
{
    class TypeMemberViewModel : BindableBase
    {
        public FlowChart.Type.Member Model { get; set; }
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

        public TypeDialogViewModel(IEventAggregator _ev)
        {
            Types = new ObservableCollection<TypeViewModel>();
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);
        }

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

        public string Title { get; }
        public event Action<IDialogResult>? RequestClose;
        #endregion

        public ObservableCollection<TypeViewModel> Types { get; set; }

        void OnCloseProject(Project project)
        {
            CurrentProject = null;
            Types.Clear();
        }

    }
}
