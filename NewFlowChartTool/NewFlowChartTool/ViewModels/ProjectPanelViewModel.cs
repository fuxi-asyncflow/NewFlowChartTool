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
using FlowChart.Common;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Commands;
using Prism.Ioc;
using System.ComponentModel;
using FlowChart;
using FlowChart.Misc;
using Prism.Services.Dialogs;
using ProjectFactory;

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
        public MenuItemViewModel(string text, DelegateCommand<T> command)
        {
            Text = text;
            Command = command;
        }
        public string Text { get; set; }
        public DelegateCommand<T> Command { get; set; }
        public string? InputGestureText { get; set; }
    }
    public class ProjectTreeItemViewModel : BindableBase
    {
        public enum ItemIcon
        {
            Graph = 0,
            Folder = 1,
            SubGraphGlobal = 2,
            SubGraphLocal = 3
        }
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
            item.TypeChangeEvent += delegate { RaisePropertyChanged(nameof(Type)); };
            if (_item is Graph graph)
                graph.SubGraphChangeEvent += delegate { RaisePropertyChanged(nameof(IconType)); };

        }
        protected readonly TreeItem _item;
        public TreeItem Item => _item;
        public string Name => _item.Name;
        public string Path => _item.Path;
        [Obsolete("Should be refactored")]
        private static Dictionary<FlowChart.Type.Type, TypeViewModel> _types;
        public TypeViewModel Type //TODO use TypeViewModel saved in TypeManagerDialog
        {
            get
            {
                if (_types == null)
                    _types = new Dictionary<FlowChart.Type.Type, TypeViewModel>();
                if(_item.Type == null)
                    Logger.ERR("error type in null");
                if (_types.ContainsKey(_item.Type))
                    return _types[_item.Type];
                var vm = new TypeViewModel(_item.Type);
                _types.Add(_item.Type, vm);
                return vm;
            }
        }

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
            if (MainWindowViewModel.Inst.IsOpened(graph))
            {
                MessageBox.Show($"graph {graph.Path} is opened, close it before remove");
            }
            else
            {
                graph.Project.Remove(graph);
            }
        }

        public void OnRename(TreeItem item, string newName)
        {
            Debug.Assert(item == _item);
            Debug.Assert(newName == Name);
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Path));
            Folder?.Sort();
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

        public ItemIcon IconType
        {
            get
            {
                if (_item is Graph graph)
                {
                    switch (graph.SubGraphType)
                    {
                        case Graph.SubGraphTypeEnum.GLOBAL:
                            return ItemIcon.SubGraphGlobal;
                        case Graph.SubGraphTypeEnum.LOCAL:
                            return ItemIcon.SubGraphLocal;
                        default:
                            return ItemIcon.Graph;
                    }
                }
                return ItemIcon.Folder;
            }
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

        public static void MenuCutGraph(ProjectTreeItemViewModel item)
        {
            OutputPanelViewModel.OutputMsg($"cut graph {item.Path}");
            _clipBoard = item;
            _isCut = true;
        }

        public static void MenuCopyGraph(ProjectTreeItemViewModel item)
        {
            OutputPanelViewModel.OutputMsg($"copy graph {item.Path}");
            _clipBoard = item;
            _isCut = false;
        }

        public static void MenuPasteGraph(ProjectTreeItemViewModel item)
        {
            if (_clipBoard == null)
            {
                OutputPanelViewModel.OutputMsg($"paste graph failed, clipboard is empty", OutputMessageType.Warning);
                return;
            }

            var project = item.Item.Project;
            var source = _clipBoard.Item;
            var dstFolder = item as ProjectTreeFolderViewModel ?? item.Folder;
            if (dstFolder == null)
            {
                OutputPanelViewModel.OutputMsg($"paste graph failed, cannot get folder", OutputMessageType.Warning);
                return;
            }

            if (_isCut)
            {
                if (source.Parent == dstFolder.Item || source == dstFolder.Item)
                {
                    OutputPanelViewModel.OutputMsg($"paste graph failed, item is in same folder", OutputMessageType.Warning);
                    return;
                }
                else
                {
                    project.Remove(source);
                    _isCut = false;
                    if (dstFolder.Item is Folder folder)
                    {
                        var name = folder.GetUnusedName(source.Name);
                        source.Name = name;
                        source.Path = folder.Path + "." + source.Name;
                        folder.AddChild(source);
                        if(source is Graph g)
                            dstFolder.AddChild(new ProjectTreeItemViewModel(g));
                        else if (source is Folder f)
                        {
                            f.Rename(source.Name);
                            dstFolder.AddChild(new ProjectTreeFolderViewModel(f));
                        }
                    }
                }
            }
            else
            {
                if (source is Folder)
                {
                    OutputPanelViewModel.OutputMsg($"目前不支持文件夹的粘贴", OutputMessageType.Error);
                    return;
                }

                if (source is Graph graph)
                {
                    var clone = project.Factory.DeserializeGraph(project.Factory.SerializeGraph(graph));
                    clone?.ResetUid();
                    if (clone != null && dstFolder.Item is Folder folder)
                    {
                        var name = folder.GetUnusedName(clone.Name);
                        if (name != clone.Name)
                        {
                            clone.Name = name;
                        }

                        clone.Path = folder.Path + "." + clone.Name;
                        project.AddGraph(clone);
                    }
                }
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
                    MenuItems.Add(new MenuItemViewModel<ProjectTreeItemViewModel>(
                        Application.Current.FindResource(menuItemAttr.Name) as string ?? menuItemAttr.Name.ToString() ?? string.Empty
                    , new DelegateCommand<ProjectTreeItemViewModel>(menuDelegate)));
                }
            }

            var otherMenus = new List<Tuple<object, Action<ProjectTreeItemViewModel>, string?>>();
            otherMenus.Add(new Tuple<object, Action<ProjectTreeItemViewModel>, string?>
                (NFCT.Common.Localization.ResourceKeys.Common_Cut, MenuCutGraph, null));
            otherMenus.Add(new Tuple<object, Action<ProjectTreeItemViewModel>, string?>
                (NFCT.Common.Localization.ResourceKeys.Common_Copy, MenuCopyGraph, null));
            otherMenus.Add(new Tuple<object, Action<ProjectTreeItemViewModel>, string?>
                (NFCT.Common.Localization.ResourceKeys.Common_Paste, MenuPasteGraph, null));

            foreach (var item in otherMenus)
            {
                var key = item.Item1;
                var f = item.Item2;
                var menuItem = new MenuItemViewModel<ProjectTreeItemViewModel>(
                    Application.Current.FindResource(key) as string ?? key.ToString() ?? string.Empty
                    , new DelegateCommand<ProjectTreeItemViewModel>(f));
                menuItem.InputGestureText = item.Item3;
                MenuItems.Add(menuItem);
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

        #region Copy Paste

        private static ProjectTreeItemViewModel? _clipBoard;
        private static bool _isCut;

        #endregion
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
            Sort();
        }

        public void Remove(TreeItem item)
        {
            foreach (var child in Children)
            {
                if (child.Item == item)
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

            var path = $"{folder.Path}.{newGraphName}";
            var newFolder = folder.Project.GetFolder(path);
            newFolder.Type = folder.Type;
            AddChild(new ProjectTreeFolderViewModel(newFolder));
        }

        public void Sort()
        {
            var tempChildren = new List<ProjectTreeItemViewModel>(Children);
            //var caseInsensitiveComparer = new CaseSensitiveComparer();
            tempChildren.Sort((a, b) => a.IsFolder == b.IsFolder
                ? string.Compare(a.Name, b.Name, StringComparison.Ordinal)
                : b.IsFolder.CompareTo(a.IsFolder));
            Children.Clear();
            tempChildren.ForEach(Children.Add);
        }
    }

    public class ProjectTreeRootViewModel : ProjectTreeFolderViewModel
    {
        public ProjectTreeRootViewModel(TreeItem item) : base(item)
        {
            ConfigCommand = new DelegateCommand(ShowConfigDialog);
        }

        public DelegateCommand ConfigCommand { get; set; }

        void ShowConfigDialog()
        {
            var dialogService = ContainerLocator.Current.Resolve<IDialogService>();
            
            var config = Item.Project.Config.GraphRoots.Find(cfg => cfg.Name == Item.Name);
            if (config != null)
            {
                var parameters = new DialogParameters();
                parameters.Add("graph_root", config);
                parameters.Add("project", Item.Project);
                dialogService.ShowDialog(GraphRootConfigDialogViewModel.NAME, parameters, result => { });
            }
        }

        public override void Remove()
        {
            Logger.WARN("root folder cannot be deleted");
            return;
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
            NewGraphRootCommand = new DelegateCommand(NewGraphRoot);

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
                    ProjectTreeFolderViewModel vm = folder.Parent != project.Root ? new ProjectTreeFolderViewModel(folder) : new ProjectTreeRootViewModel(folder);
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

        public void OnRemoveGraph(TreeItem item)
        {
            if (GetTreeItemViewModel(item.Parent) is ProjectTreeFolderViewModel folderVm)
            {
                folderVm.Remove(item);
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

        void NewGraphRoot()
        {
            if (Project == null)
                return;
            var config = Project.Config;
            if (config == null)
                return;
            var dialogService = ContainerLocator.Current.Resolve<IDialogService>();


            GraphRootConfig rootConfig;
            rootConfig = config.GraphRoots.Count > 0 ? config.GraphRoots[0].Clone() : new GraphRootConfig();
            rootConfig.Name = "NewFile";

            var parameters = new DialogParameters();
            parameters.Add("graph_root", rootConfig);
            parameters.Add("project", Project);
            dialogService.ShowDialog(GraphRootConfigDialogViewModel.NAME, parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    
                    config.GraphRoots.Add(rootConfig);
                    config.GetGraphRoot(rootConfig.Name);
                    Project.Save();
                }
            });
        }

        public ProjectTreeItemViewModel? ProjectTreeRoot { get; set; }
        public ObservableCollection<ProjectTreeItemViewModel> Roots { get; set; }
        public DelegateCommand NewGraphRootCommand { get; private set; }

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
            if (treeItem is ProjectTreeFolderViewModel folder)
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
