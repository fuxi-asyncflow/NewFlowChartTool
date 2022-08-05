using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using Prism.Events;
using Prism.Ioc;
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
                Text = "test",
                Command = new DelegateCommand<ProjectTreeItemViewModel>((item) =>
            {
                MessageBox.Show($"{item.Name}");
            })
            });
        }
        public ProjectTreeItemViewModel(Item item)
        {
            _item = item;           
        }
        readonly Item _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }

        public virtual void Open()
        {
            if (_item is Graph graph)
            {
                EventHelper.Pub<GraphOpenEvent, Graph>(graph);
                graph.Project.BuildGraph(graph);
            }
        }

        public static ObservableCollection<MenuItemViewModel<ProjectTreeItemViewModel>> MenuItems { get; set; }
    }

    internal class ProjectTreeFolderViewModel : ProjectTreeItemViewModel
    {
        public ProjectTreeFolderViewModel(Item item) : base(item)
        {
            
            Children = new ObservableCollection<ProjectTreeItemViewModel>();
        }
        public ObservableCollection<ProjectTreeItemViewModel> Children { get; set; }

        public void AddChild(ProjectTreeItemViewModel? child) { if (child != null) Children.Add(child); }
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
