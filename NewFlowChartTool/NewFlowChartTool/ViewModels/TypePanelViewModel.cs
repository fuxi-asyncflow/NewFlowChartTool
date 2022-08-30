using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Type;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Events;
using Prism.Mvvm;

namespace NewFlowChartTool.ViewModels
{
    internal class TypeMemberTreeItemViewModel : BindableBase
    {
        public enum TypeMemberType
        {
            Class = 0,
            Method = 1,
            Property = 2
        }
        public TypeMemberTreeItemViewModel(Item item)
        {
            _item = item;
        }
        readonly Item _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }
        public TypeMemberType MemberType
        {
            get
            {
                if (_item is Method)
                    return TypeMemberType.Method;
                if(_item is Property)
                    return TypeMemberType.Property;
                return TypeMemberType.Class;
            }
        }
    }

    internal class TypeMemberTreeFolderViewModel : TypeMemberTreeItemViewModel
    {
        public TypeMemberTreeFolderViewModel(Item item) : base(item)
        {

            Children = new ObservableCollection<TypeMemberTreeItemViewModel>();
        }
        public ObservableCollection<TypeMemberTreeItemViewModel> Children { get; set; }

        public void AddChild(TypeMemberTreeItemViewModel? child) { if (child != null) Children.Add(child); }
    }

    internal class TypePanelViewModel : BindableBase
    {
        internal TypePanelViewModel()
        {
            Roots = new ObservableCollection<TypeMemberTreeItemViewModel>();
            SubscribeEvents();
        }

        void SubscribeEvents()
        {
            EventHelper.Sub<ProjectOpenEvent, Project>(OnOpenProject, ThreadOption.UIThread);
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);
        }

        public void OnOpenProject(Project project)
        {
            foreach (var kv in project.TypeDict)
            {
                var tp = kv.Value;
                if (!kv.Value.IsBuiltinType)
                {
                    AddType(tp);
                }
            }
            AddEvents(project);
            //TypeTreeRoot = CreateTreeItemViewModel(project.Root);
            //if (TypeTreeRoot is ProjectTreeFolderViewModel)
            //{
            //    Roots = ((ProjectTreeFolderViewModel)TypeTreeRoot).Children;
            //    RaisePropertyChanged("Roots");
            //}
        }

        public void OnCloseProject(Project project)
        {
            Roots.Clear();
        }

        public void AddType(FlowChart.Type.Type tp)
        {
            var typeRoot = new TypeMemberTreeFolderViewModel(tp);
            foreach (var kv in tp.MemberDict)
            {
                var treeItem = new TypeMemberTreeItemViewModel(kv.Value);
                typeRoot.AddChild(treeItem);
            }
            Roots.Add(typeRoot);
            RaisePropertyChanged("Roots");
        }

        public void AddEvents(Project project)
        {
            var typeRoot = new TypeMemberTreeFolderViewModel(new Item("Events"));
            foreach (var kv in project.EventDict)
            {
                var treeItem = new TypeMemberTreeItemViewModel(kv.Value);
                typeRoot.AddChild(treeItem);
            }
            Roots.Add(typeRoot);
            RaisePropertyChanged("Roots");
        }

        public TypeMemberTreeItemViewModel? TypeTreeRoot { get; set; }
        public ObservableCollection<TypeMemberTreeItemViewModel> Roots { get; set; }
    }
}
