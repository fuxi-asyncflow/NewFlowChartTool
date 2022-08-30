using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using FlowChart.AST;
using Prism.Mvvm;
using Prism.Events;
using FlowChart.Core;
using FlowChartCommon;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Commands;
using Prism.Ioc;

namespace NewFlowChartTool.ViewModels
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class MenuItemAttribute : System.Attribute
    {
        public object Name;

        public MenuItemAttribute(object name)
        {
            Name = name;
        }
    }
    public class MenuItemViewModel<T> : BindableBase where T : BindableBase
    {
        public string Text { get; set; }
        public DelegateCommand<T> Command { get; set; }
    }
    internal class ProjectTreeItemViewModel : BindableBase
    {
        static ProjectTreeItemViewModel()
        {
            MenuItems = new ObservableCollection<MenuItemViewModel<ProjectTreeItemViewModel>>();
            OnLangSwitch(Lang.Chinese);
            EventHelper.Sub<LangSwitchEvent, Lang>(OnLangSwitch);
        }

        public ProjectTreeItemViewModel(TreeItem item)
        {
            _item = item;
            item.NameChangeEvent += OnRename;
        }
        protected readonly TreeItem _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }
        public ProjectTreeFolderViewModel? Folder { get; set; }
        public ProjectTreeItemViewModel? Parent => Folder;

        public virtual void Open()
        {
            if (_item is Graph graph)
            {
                EventHelper.Pub<GraphOpenEvent, Graph>(graph);
                graph.Project.BuildGraph(graph, new ParserConfig(){GetTokens = true});
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
            set { SetProperty(ref _isExpanded, value); RaisePropertyChanged("ItemIcon"); }
        }

        private bool _isEditingName;
        public bool IsEditingName
        {
            get => _isEditingName;
            set => SetProperty(ref _isEditingName, value);
        }

        public void EnterRenameMode()
        {
            IsEditingName = true;
        }

        public void ExitingRenameMode(string newName, bool save = true)
        {
            IsEditingName = false;
            if (!save)
                return;
            if (newName == Name)
                return;
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("name is empty");
                return;
            }

            if (Folder != null && Folder.GetChild(newName) != null)
            {
                MessageBox.Show("item with same name exist");
                return;
            }

            if (newName.Contains('.'))
            {
                MessageBox.Show("graph name cannot contains `.`");
                return;
            }

            // rename
            Rename(newName);
        }

        public virtual void Rename(string newName)
        {
            var graph = _item as Graph;
            if (graph == null)
                return;
            graph.Rename(newName);
        }

        public void OnRename(TreeItem item, string newName)
        {
            Debug.Assert(item == _item);
            Debug.Assert(newName == Name);
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Path));
        }

        public string GetPath()
        {
            var paths = new Stack<string>();
            var cur = this;
            while (cur != null)
            {
                paths.Push(cur.Name);
                cur = cur.Folder;
            }

            var pathList = new List<string>();
            while (paths.Count > 0)
            {
                var s = paths.Pop();
                if(!string.IsNullOrEmpty(s))
                    pathList.Add(s);
            }

            return string.Join('.', pathList);
        }

        public static ObservableCollection<MenuItemViewModel<ProjectTreeItemViewModel>> MenuItems { get; set; }

        [MenuItem(NFCT.Common.Localization.ResourceKeys.Menu_RenameKey)]
        public static void MenuRename(ProjectTreeItemViewModel item)
        {
            // entering editing name mode
            Console.WriteLine($"RENAME {item.GetHashCode()}");
            item.EnterRenameMode();
        }

        [MenuItem(NFCT.Common.Localization.ResourceKeys.Menu_NewGraphKey)]
        public static void MenuNewGraph(ProjectTreeItemViewModel item)
        {
            if (item is ProjectTreeFolderViewModel folder)
            {
                folder.AddNewGraph();
                return;
            }

            if (item.Folder != null)
            {
                item.Folder.AddNewGraph();
            }
        }

        [MenuItem(NFCT.Common.Localization.ResourceKeys.Menu_NewFolderKey)]
        public static void MenuNewFolder(ProjectTreeItemViewModel item)
        {
            if (item is ProjectTreeFolderViewModel folder)
            {
                folder.AddNewFolder();
                return;
            }

            if (item.Folder != null)
            {
                item.Folder.AddNewFolder();
            }
        }

        public static void OnLangSwitch(Lang lang)
        {
            MenuItems.Clear();
            var tp = typeof(ProjectTreeItemViewModel);
            foreach (var methodInfo in tp.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = methodInfo.GetCustomAttribute(typeof(MenuItemAttribute));
                if (attr is MenuItemAttribute menuItemAttr)
                {
                    var menuDelegate = (Action<ProjectTreeItemViewModel>)
                        Delegate.CreateDelegate(typeof(Action<ProjectTreeItemViewModel>), methodInfo);
                    MenuItems.Add(new MenuItemViewModel<ProjectTreeItemViewModel>()
                    {
                        Text = Application.Current.FindResource(menuItemAttr.Name) as string,
                        Command = new DelegateCommand<ProjectTreeItemViewModel>(menuDelegate)
                    });
                }
            }
        }
    }

    internal class ProjectTreeFolderViewModel : ProjectTreeItemViewModel
    {
        public ProjectTreeFolderViewModel(TreeItem item) : base(item)
        {
            
            Children = new ObservableCollection<ProjectTreeItemViewModel>();
        }
        public ObservableCollection<ProjectTreeItemViewModel> Children { get; set; }

        public void AddChild(ProjectTreeItemViewModel? child)
        {
            if (child == null) return;
            child.Folder = this;
            Children.Add(child);
        }

        public ProjectTreeItemViewModel? GetChild(string name)
        {
            foreach (var item in Children)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        public override void Rename(string newName)
        {
            var folder = _item as Folder;
            if(folder == null) return;
            folder.Rename(newName);
        }

        public void AddNewGraph()
        {
            var folder = _item as Folder;
            if(folder == null) return;
            var newGraphName = "new_graph";
            if (folder.ContainsChild(newGraphName))
            {
                int i = 1;
                while (folder.ContainsChild(newGraphName))
                {
                    newGraphName = $"new_graph_{i}";
                    i++;
                }
            }
            var graph = Graph.EmptyGraph.Clone();
            graph.Name = newGraphName;
            graph.Path = $"{GetPath()}.{newGraphName}";
            graph.Project = folder.Project;
            graph.Type = folder.Type;

            folder.Project.AddGraph(graph);
            
        }

        public void AddNewFolder()
        {
            var folder = _item as Folder;
            if (folder == null) return;
            var newGraphName = "new_folder";
            if (folder.ContainsChild(newGraphName))
            {
                int i = 1;
                while (folder.ContainsChild(newGraphName))
                {
                    newGraphName = $"new_folder_{i}";
                    i++;
                }
            }

            var newFolder = folder.GetOrCreateSubFolder(newGraphName);
            AddChild(new ProjectTreeFolderViewModel(newFolder));
        }
    }

    internal class ProjectPanelViewModel : BindableBase
    {
        internal ProjectPanelViewModel(IEventAggregator ea)
        {
            _ea = ea;
            Roots = new ObservableCollection<ProjectTreeItemViewModel>();
            SubscribeEvents();
        }


        public Project? Project;

        void SubscribeEvents()
        {
            
            _ea.GetEvent<ProjectOpenEvent>().Subscribe(OnOpenProject, ThreadOption.UIThread);
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);
        }

        private IEventAggregator _ea;

        public void OnOpenProject(Project project)
        {
            Project = project;
            Project.AddGraphEvent += OnAddGraph;
            ProjectTreeItemViewModel? CreateTreeItemViewModel(TreeItem item)
            {                
                if (item is Graph) return new ProjectTreeItemViewModel(item);
                if (item is Folder)
                {
                    var vm = new ProjectTreeFolderViewModel(item);
                    ((Folder)item).Children.ForEach(child => vm.AddChild(CreateTreeItemViewModel(child)));
                    return vm;
                }

                return null;
            }
            ProjectTreeRoot = CreateTreeItemViewModel(project.Root);
            if(ProjectTreeRoot is ProjectTreeFolderViewModel)
            {
                Roots = ((ProjectTreeFolderViewModel)ProjectTreeRoot).Children;
                RaisePropertyChanged("Roots");
            }            
        }

        public void OnCloseProject(Project p)
        {
            Roots.Clear();
        }

        public void OnAddGraph(Graph graph)
        {
            var folderVm = GetTreeItemViewModel(graph.Parent) as ProjectTreeFolderViewModel;
            if (folderVm != null)
            {
                var treeItem = new ProjectTreeItemViewModel(graph);
                folderVm.AddChild(treeItem);
                AddGraphEvent?.Invoke(treeItem);
            }
        }

        private ProjectTreeItemViewModel? GetTreeItemViewModel(TreeItem? item)
        {
            var list = new List<TreeItem>();
            TreeItem? cur = item;
            while (cur != Project.Root)
            {
                list.Add(cur);
                cur = cur.Parent;
            }
            list.Reverse();

            var root = ProjectTreeRoot;
            foreach (var treeItem in list)
            {
                var vm = root as ProjectTreeFolderViewModel;
                if (vm == null)
                    return null;
                root = vm.GetChild(treeItem.Name);
            }
            return root;
        }

        public ProjectTreeItemViewModel? ProjectTreeRoot { get; set; }
        public ObservableCollection<ProjectTreeItemViewModel> Roots { get; set; }

        public delegate void ProjectTreeItemDelegate(ProjectTreeItemViewModel item);

        public event ProjectTreeItemDelegate AddGraphEvent;
    }
}
