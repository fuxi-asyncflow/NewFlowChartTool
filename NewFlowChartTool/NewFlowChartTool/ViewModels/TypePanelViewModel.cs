using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
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
            IsVisibility = true;
        }
        readonly Item _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }
        public Item TreeItem { get => _item; }
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

        private bool _isVisible;

        public bool IsVisibility
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        public TypeMemberTreeItemViewModel ParentNode { get; set; }
    }

    internal class TypeMemberTreeFolderViewModel : TypeMemberTreeItemViewModel
    {
        public TypeMemberTreeFolderViewModel(Item item) : base(item)
        {
            Children = new ObservableCollection<TypeMemberTreeItemViewModel>();
            if (item is Type type && type.HasBaseType)
            {
                
                BaseType = $"({type.GetBaseTypesString()})";
            }
            
        }
        public ObservableCollection<TypeMemberTreeItemViewModel> Children { get; set; }

        public void AddChild(TypeMemberTreeItemViewModel? child) 
        { 
            if (child != null)
            {
                Children.Add(child);
                ChildsVisibleCount++;
                child.ParentNode = this;
            }               
        }

        public string? BaseType { get; set; }

        
        public int ChildsVisibleCount
        {
            get; set;
        }
    }

    public class TypePanelViewModel : BindableBase
    {
        public TypePanelViewModel()
        {
            Roots = new ObservableCollection<TypeMemberTreeItemViewModel>();
            CurSearchChilds = new List<TypeMemberTreeItemViewModel>();
            m_lastSeacherText = "";
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
            EventHelper.Sub<UpdateTypePanelDataEvent, Project>(UpdateRoot, ThreadOption.UIThread);
        }

        public void OnOpenProject(Project project)
        {
            // lazy load graph may run at same time and change type dict
            // so first get types to a list and user later
            var types = project.TypeDict.Values.ToList();
            foreach (var tp in types)
            {
                if (!tp.IsBuiltinType)
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

        void UpdateRoot(Project project)
        {
            Roots.Clear();
            OnOpenProject(project);
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
        void Search(string? text)
        {
            text = text.ToLower();
            if (text.Length < 3)
            {
                if(!string.IsNullOrEmpty(m_lastSeacherText))
                {
                    ResetRootVisible();
                }               
                m_lastSeacherText = "";
            }
            else
            {
                if (text.Length < 3 && Encoding.UTF8.GetByteCount(text) < 6)
                    return;
                if (!string.IsNullOrEmpty(m_lastSeacherText) && text.Length >= m_lastSeacherText.Length && text.StartsWith(m_lastSeacherText))
                {
                    SearchLastData(text);
                }
                else
                {
                    CurSearchChilds.Clear();
                    ResetRootVisible();
                    SearchRootData(text);
                }
                RaisePropertyChanged("Roots");
                m_lastSeacherText = text;

            }
        }
        private void SearchRootData(string text)
        {
            foreach (var node in Roots)
            {
                var treeFolderNode = (node as TypeMemberTreeFolderViewModel);
                if (treeFolderNode.ChildsVisibleCount == 0)
                    node.IsVisibility = false;
                else
                {
                    foreach (var child in treeFolderNode.Children)
                    {
                        if (child.Name.ToLower().Contains(text))
                        {
                            CurSearchChilds.Add(child);
                        }
                        else
                        {
                            SetParentVisibility(treeFolderNode);
                            child.IsVisibility = false;
                        }
                    }
                    node.IsExpanded = true;
                }               
            }
        }
        private void SetParentVisibility(TypeMemberTreeFolderViewModel node)
        {
            node.ChildsVisibleCount--;
            if (node.ChildsVisibleCount == 0)
                node.IsVisibility = false;
        }
        private void ResetRootVisible()
        {
            foreach (var node in Roots)
            {
                var treeFolderNode = (node as TypeMemberTreeFolderViewModel);
                node.IsVisibility = true;
                var children = treeFolderNode.Children;
                foreach (var child in children)
                {
                    child.IsVisibility = true;
                }
                treeFolderNode.ChildsVisibleCount = children.Count();
            }
        }
        private void SearchLastData(string text)
        {
            foreach (var node in CurSearchChilds)
            {
                if (!node.Name.ToLower().Contains(text))
                {
                    node.IsVisibility = false;                    
                    SetParentVisibility(node.ParentNode as TypeMemberTreeFolderViewModel);
                }
            }
            CurSearchChilds.RemoveAll(n=>{ return n.IsVisibility == false; });
        }
        public TypeMemberTreeItemViewModel? TypeTreeRoot { get; set; }
        public ObservableCollection<TypeMemberTreeItemViewModel> Roots { get; set; }
        private List<TypeMemberTreeItemViewModel> CurSearchChilds;
        private string? m_lastSeacherText;
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set 
            { 
                SetProperty(ref _searchText, value); 
                Search(_searchText); 
            }
        }
    }
}
