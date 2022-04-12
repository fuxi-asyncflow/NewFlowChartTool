using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Events;
using FlowChart.Core;

namespace NewFlowChartTool.ViewModels
{
    internal class ProjectTreeItemViewModel : BindableBase
    {
        public ProjectTreeItemViewModel(Item item)
        {
            _item = item;           
        }
        readonly Item _item;
        public string Name { get => _item.Name; }
        public string Description { get => _item.Description; }
        
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
