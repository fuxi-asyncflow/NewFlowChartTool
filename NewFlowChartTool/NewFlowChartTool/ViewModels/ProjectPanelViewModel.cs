using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using Prism.Events;
using FlowChart.Core;
using NewFlowChartTool.Event;
using Prism.Commands;

namespace NewFlowChartTool.ViewModels
{
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
            MenuItems.Add(new MenuItemViewModel<ProjectTreeItemViewModel>()
                {
                    Text = "rename",
                    Command = new DelegateCommand<ProjectTreeItemViewModel>(MenuRename)
                }
            );
        }
        public ProjectTreeItemViewModel(Item item)
        {
            _item = item;           
        }
        protected readonly Item _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }
        public ProjectTreeFolderViewModel? Folder { get; set; }

        public virtual void Open()
        {
            if (_item is Graph graph)
            {
                EventHelper.Pub<GraphOpenEvent, Graph>(graph);
                graph.Project.BuildGraph(graph);
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
            graph.Name = newName;
            graph.Path = GetPath();
            RaisePropertyChanged(nameof(Name));
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

        public virtual void SetPath(string path)
        {
            var graph = _item as Graph;
            if (graph == null)
                return;
            graph.Path = path;
        }

        public static ObservableCollection<MenuItemViewModel<ProjectTreeItemViewModel>> MenuItems { get; set; }

        public static void MenuRename(ProjectTreeItemViewModel item)
        {
            // entering editing name mode
            Console.WriteLine($"RENAME {item.GetHashCode()}");
            item.EnterRenameMode();
        }
    }

    internal class ProjectTreeFolderViewModel : ProjectTreeItemViewModel
    {
        public ProjectTreeFolderViewModel(Item item) : base(item)
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
            folder.Name = newName;
            SetPath(GetPath());
        }

        public override void SetPath(string path)
        {
            var folder = _item as Folder;
            if (folder == null) return;
            foreach (var projectTreeItemViewModel in Children)
            {
                projectTreeItemViewModel.SetPath($"{path}.{Name}");
            }
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

        void SubscribeEvents()
        {
            
            _ea.GetEvent<Event.ProjectOpenEvent>().Subscribe(OnOpenProject, ThreadOption.UIThread);
        }

        private IEventAggregator _ea;

        public void OnOpenProject(Project project)
        {
            ProjectTreeItemViewModel? CreateTreeItemViewModel(Item item)
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

        public ProjectTreeItemViewModel? ProjectTreeRoot { get; set; }
        public ObservableCollection<ProjectTreeItemViewModel> Roots { get; set; }
    }
}
