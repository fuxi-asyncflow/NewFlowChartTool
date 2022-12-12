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
using System.ComponentModel;
using Prism.Services.Dialogs;
using ProjectFactory;

namespace NewFlowChartTool.ViewModels
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class MenuItemAttribute : System.Attribute
    {
        public string Name;

        public MenuItemAttribute(string name)
        {
            Name = name;
        }
    }
    public class MenuItemViewModel<T> : BindableBase where T : BindableBase
    {
        public MenuItemViewModel(string text, DelegateCommand<T> command)
        {
            Text = text;
            Command = command;
        }
        public string Text { get; set; }
        public DelegateCommand<T> Command { get; set; }
    }
    public class ProjectTreeItemViewModel : BindableBase
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
            item.DescriptionChangeEvent += delegate { RaisePropertyChanged(nameof(Description)); };
        }
        protected readonly TreeItem _item;
        public TreeItem Item => _item;
        public string Name => _item.Name;
        public string? Description => _item.Description;
        public ProjectTreeFolderViewModel? Folder { get; set; }
        public ProjectTreeItemViewModel? Parent => Folder;
        public virtual bool IsFolder => false;

        public virtual void Open()
        {
            if (_item is Graph graph)
            {
                MainWindowViewModel.Inst?.OnOpenGraph(graph);
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
            get => (this is ProjectTreeFolderViewModel) && _isExpanded;
            set
            {
                if (this is ProjectTreeFolderViewModel)
                SetProperty(ref _isExpanded, value);
            }
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

        public virtual void Remove()
        {
            var graph = _item as Graph;
            if (graph == null)
                return;
            graph.Project.Remove(graph);
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
            Logger.LOG($"RENAME {item.GetHashCode()}");
            item.EnterRenameMode();
        }

        [MenuItem(NFCT.Common.Localization.ResourceKeys.Menu_ModifyDescriptionKey)]
        public static void MenuModifyDescription(ProjectTreeItemViewModel item)
        {
            var dlgService = ContainerLocator.Current.Resolve<IDialogService>();
            var parameters = new DialogParameters();
            parameters.Add("Description", $"set description for {item.Name}");
            parameters.Add("Value", item.Description);
            dlgService.Show(InputDialogViewModel.NAME, parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var v = result.Parameters.GetValue<string>("Value");
                    item.Item.Description = v;
                }
            });
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

        [MenuItem(NFCT.Common.Localization.ResourceKeys.Menu_RemoveGraphKey)]
        public static void MenuRemoveGraph(ProjectTreeItemViewModel item)
        {
            item.Remove();
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
                    MenuItems.Add(new MenuItemViewModel<ProjectTreeItemViewModel>(
                        Application.Current.FindResource(menuItemAttr.Name) as string ?? menuItemAttr.Name
                    , new DelegateCommand<ProjectTreeItemViewModel>(menuDelegate)));
                }
            }
        }

        public bool Search(string text)
        {
            if (Name.ToLower().Contains(text))
                return true;
            if (Description != null && Description.ToLower().Contains(text))
                return true;
            return false;
        }

        public void Expand()
        {
            var folder = Folder;
            while (folder != null)
            {
                folder.IsExpanded = true;
                folder = folder.Folder;
            }
        }
    }

    public class ProjectTreeFolderViewModel : ProjectTreeItemViewModel
    {
        public ProjectTreeFolderViewModel(TreeItem item) : base(item)
        {
            
            Children = new ObservableCollection<ProjectTreeItemViewModel>();
        }
        public ObservableCollection<ProjectTreeItemViewModel> Children { get; set; }
        public override bool IsFolder => true;

        public void AddChild(ProjectTreeItemViewModel? child)
        {
            if (child == null) return;
            child.Folder = this;
            Children.Add(child);
        }

        public void Remove(Graph graph)
        {
            foreach (var child in Children)
            {
                if (child.Item == graph)
                {
                    Children.Remove(child);
                    return;
                }
            }
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

        public override void Remove()
        {
            var folder = _item as Folder;
            if (folder == null) return;
            folder.Project.Remove(folder);
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
            graph.Type = folder.Type ?? folder.Children.First().Type;

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
            if(newFolder != null)
                AddChild(new ProjectTreeFolderViewModel(newFolder));
        }
    }

    public class ProjectPanelViewModel : BindableBase
    {
        public ProjectPanelViewModel()
        {
            Roots = new ObservableCollection<ProjectTreeItemViewModel>();
            SubscribeEvents();
            SearchResult = new ObservableCollection<ProjectTreeItemViewModel>();
            _searchText = null;

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Project = new Project(new MemoryProjectFactory());
                Project.Load();
                OnOpenProject(Project);
                return;
            }
#endif
        }


        public Project? Project;

        void SubscribeEvents()
        {
            EventHelper.Sub<ProjectOpenEvent, Project>(OnOpenProject, ThreadOption.UIThread);
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);
        }

        public void OnOpenProject(Project project)
        {
            Project = project;
            Project.AddGraphEvent += OnAddGraph;
            Project.RemoveGraphEvent += OnRemoveGraph;
            ProjectTreeItemViewModel? CreateTreeItemViewModel(TreeItem item)
            {                
                if (item is Graph) return new ProjectTreeItemViewModel(item);
                if (item is Folder folder)
                {
                    var vm = new ProjectTreeFolderViewModel(folder);
                    folder.Children.ForEach(child => vm.AddChild(CreateTreeItemViewModel(child)));
                    return vm;
                }

                return null;
            }
            ProjectTreeRoot = CreateTreeItemViewModel(project.Root);
            if(ProjectTreeRoot is ProjectTreeFolderViewModel model)
            {
                Roots = model.Children;
                RaisePropertyChanged(nameof(Roots));
            }            
        }

        public void OnCloseProject(Project p)
        {
            Roots.Clear();
        }

        public void OnAddGraph(Graph graph)
        {
            if (GetTreeItemViewModel(graph.Parent) is ProjectTreeFolderViewModel folderVm)
            {
                var treeItem = new ProjectTreeItemViewModel(graph);
                folderVm.AddChild(treeItem);
                AddGraphEvent?.Invoke(treeItem);
            }
        }

        public void OnRemoveGraph(Graph graph)
        {
            if (GetTreeItemViewModel(graph.Parent) is ProjectTreeFolderViewModel folderVm)
            {
                folderVm.Remove(graph);
            }
        }

        private ProjectTreeItemViewModel? GetTreeItemViewModel(TreeItem? item)
        {
            if (Project == null)
                return null;
            var list = new List<TreeItem>();
            TreeItem? cur = item;
            while (cur != Project.Root)
            {
                if (cur == null)
                    break;
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

        public event ProjectTreeItemDelegate? AddGraphEvent;

        #region Search

        private string? _searchText;
        public string? SearchText { get => _searchText;
            set { SetProperty(ref _searchText, value); Search(_searchText);}
        }
        private bool _showPopup;
        public bool ShowPopup { get => _showPopup; set => SetProperty(ref _showPopup, value); }
        public ObservableCollection<ProjectTreeItemViewModel> SearchResult { get; set; }
        private ProjectTreeItemViewModel? _selectedSearchItem;
        public ProjectTreeItemViewModel? SelectedSearchItem { get => _selectedSearchItem; set => SetProperty(ref _selectedSearchItem, value); }

        private string? _lastSearchText;
        void Search(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                ShowPopup = false;
                return;
            }

            text = text.ToLower();
            if (text.Length < 3 && Encoding.UTF8.GetByteCount(text) < 6) // start search until 3 letters or 2 Chinese character
            {
                ShowPopup = false;
                return;
            }

            if (!string.IsNullOrEmpty(_lastSearchText) && text.Contains(_lastSearchText))
            {
                var result = SearchResult;
                for (int i = result.Count - 1; i >= 0; i--)
                {
                    if (!result[i].Search(text))
                    {
                        result.RemoveAt(i);
                    }
                }

                _lastSearchText = text;
                return;
            }
            else
            {
                // full search
                SearchResult.Clear();
                foreach (var root in Roots)
                {
                    _searchTreeItem(text, root);
                }
            }

            ShowPopup = true;
        }

        void _searchTreeItem(string text, ProjectTreeItemViewModel treeItem)
        {
            if(treeItem.Search(text))
                SearchResult.Add(treeItem);
            else if (treeItem is ProjectTreeFolderViewModel folder)
            {
                foreach (var item in folder.Children)
                {
                    _searchTreeItem(text, item);
                }
            }
        }

        public void ExitSearch()
        {
            ShowPopup = false;
            _lastSearchText = null;
            SearchResult.Clear();
        }

        public void OpenSelectedSearchResult()
        {
            if (SelectedSearchItem == null && SearchResult.Count > 0)
                SelectedSearchItem = SearchResult.First();
            if(SelectedSearchItem != null)
                SelectedSearchItem.Open();
        }

        #endregion
    }
}
