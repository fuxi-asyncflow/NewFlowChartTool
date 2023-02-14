using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlowChart.Core;
using FlowChart.Type;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Events;
using Prism.Mvvm;
using ProjectFactory;
using Type = FlowChart.Type.Type;

namespace NewFlowChartTool.ViewModels
{
    public class TypeMemberTreeItemViewModel : BindableBase
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

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
    }

    internal class TypeMemberTreeFolderViewModel : TypeMemberTreeItemViewModel
    {
        public TypeMemberTreeFolderViewModel(Item item) : base(item)
        {
            Children = new ObservableCollection<TypeMemberTreeItemViewModel>();
            if (item is Type type && type.BaseTypes.Count > 0)
            {
                BaseType = string.Join(',', type.BaseTypes.ConvertAll(tp => tp.Name));
                BaseType = $"({BaseType})";
            }
            
        }
        public ObservableCollection<TypeMemberTreeItemViewModel> Children { get; set; }

        public void AddChild(TypeMemberTreeItemViewModel? child) { if (child != null) Children.Add(child); }

        public string? BaseType { get; set; }
    }

    public class TypePanelViewModel : BindableBase
    {
        public TypePanelViewModel()
        {
            Roots = new ObservableCollection<TypeMemberTreeItemViewModel>();
            SubscribeEvents();

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var project = new Project(new MemoryProjectFactory());
                project.Load();
                OnOpenProject(project);
                return;
            }
#endif
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
