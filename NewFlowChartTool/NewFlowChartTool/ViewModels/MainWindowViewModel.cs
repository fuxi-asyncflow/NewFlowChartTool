using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using FlowChart.Core;
using System.Windows;
using System.Collections.ObjectModel;
using FlowChart.AST;
using FlowChart.LuaCodeGen;
using FlowChart.Parser;
using FlowChartCommon;
using NewFlowChartTool.Event;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Graph.ViewModels;

using ProjectFactory.DefaultProjectFactory;

namespace NewFlowChartTool.ViewModels
{
    class CodeGenFactory : ICodeGenFactory
    {
        public ICodeGenerator CreateCodeGenerator(Project p, Graph g)
        {
            return new CodeGenerator() { G = g, P = p };
        }
    }

    public class MainWindowViewModel : BindableBase
    {
        
        public MainWindowViewModel(IEventAggregator ea)
        {
            _testText = "Hello world";
            _ea = ea;
            _openedGraphs = new ObservableCollection<GraphPaneViewModel>();
            CurrentProject = null;

            OpenProjectCommand = new DelegateCommand(OpenProject, () => CurrentProject == null);
            SaveProjectCommand = new DelegateCommand(SaveProject, () => CurrentProject != null);
            NewProjectCommand = new DelegateCommand(NewProject, () => CurrentProject == null);
            CloseProjectCommand = new DelegateCommand(CloseProject, () => CurrentProject != null);
            BuildAllCommand = new DelegateCommand(BuildAll, () => CurrentProject != null);
            SwitchThemeCommand = new DelegateCommand(SwitchTheme, () => true);

            UndoCommand = new DelegateCommand(Undo);
            RedoCommand = new DelegateCommand(Redo);

            _ea.GetEvent<GraphOpenEvent>().Subscribe(OnOpenGraph);
            EventHelper.Sub<GraphCloseEvent, Graph>(OnCloseGraph);
#if DEBUG
            //TestOpenProject();
#endif
        }

        private Project? _currentProject;
        public Project? CurrentProject
        {
            get => _currentProject;
            set
            {
                if (_currentProject == value)
                    return;
                _currentProject = value;
                OpenProjectCommand.RaiseCanExecuteChanged();
                SaveProjectCommand.RaiseCanExecuteChanged();
                CloseProjectCommand.RaiseCanExecuteChanged();
            }
        }

        readonly IEventAggregator _ea;

        public string _testText;
        public string TestText { get => _testText; set { SetProperty<string>(ref _testText, value); } }

        public Theme SelectedTheme;

        public GraphPaneViewModel? _activeGraph;
        public GraphPaneViewModel? ActiveGraph { get => _activeGraph; set => SetProperty(ref _activeGraph, value, nameof(ActiveGraph)); }

        private ObservableCollection<GraphPaneViewModel> _openedGraphs;    
        public ObservableCollection<GraphPaneViewModel> OpenedGraphs
        {
            get { return _openedGraphs; }
        }

        #region COMMAND
        public DelegateCommand OpenProjectCommand { get; private set; }
        public DelegateCommand SaveProjectCommand { get; private set; }
        public DelegateCommand NewProjectCommand { get; private set; }
        public DelegateCommand CloseProjectCommand { get; private set; }
        public DelegateCommand SwitchThemeCommand { get; private set; }
        public DelegateCommand BuildAllCommand { get; private set; }

        public DelegateCommand UndoCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }
        
        public void TestOpenProject()
        {
            //var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            //var p = new FlowChart.Core.Project(new ProjectFactory.LegacyProjectFactory());
            var p = new FlowChart.Core.Project(new ProjectFactory.MemoryProjectFactory());
            //var p = new FlowChart.Core.Project(new DefaultProjectFactory());
            p.Path = @"F:\asyncflow\asyncflow_new\test\flowchart";
            p.Load();
            p.Builder = new Builder(new FlowChart.Parser.Parser(), new CodeGenFactory());
            _ea.GetEvent<ProjectOpenEvent>().Publish(p);
            p.Save();
            CurrentProject = p;
        }
        #endregion

        public void OpenProject()
        {
            if (CurrentProject != null)
            {
                MessageBox.Show("please close project before open another project");
                return;
            }

            var folderPath = Dialogs.ChooseFolder();
            if (folderPath == null)
                return;

            var p = new Project(new DefaultProjectFactory());
            p.Path = folderPath;
#if DEBUG
            p.Load();
#else
            try
            {
                p.Load();
            }
            catch (Exception e)
            {

            }
#endif
            CurrentProject = p;
            p.Builder = new Builder(new FlowChart.Parser.Parser(), new CodeGenFactory());
            _ea.GetEvent<ProjectOpenEvent>().Publish(p);
        }

        public void NewProject()
        {
            var folderPath = Dialogs.ChooseFolder();
            if (folderPath == null)
                return;

            var p = DefaultProjectFactory.CreateNewProject(folderPath);
            CurrentProject = p;
            p.Builder = new Builder(new FlowChart.Parser.Parser(), new CodeGenFactory());
            _ea.GetEvent<ProjectOpenEvent>().Publish(p);
            p.Save();
        }

        public void SwitchTheme()
        {            
            if(SelectedTheme == Theme.Dark)
            {
                SelectedTheme = Theme.Light;
                Application.Current.Resources.MergedDictionaries[0].Source 
                    = new Uri("pack://application:,,,/AvalonDock.Themes.VS2013;component/LightTheme.xaml");
                Application.Current.Resources.MergedDictionaries[1].Source
                    = new Uri("pack://application:,,,/NFCT.Themes;component/LightTheme.xaml");
                //Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/VS2013Test;component/Themes/DarkBrushsExtended.xaml");

            }
            else if(SelectedTheme == Theme.Light)
            {
                SelectedTheme = Theme.Dark;
                Application.Current.Resources.MergedDictionaries[0].Source
                    = new Uri("pack://application:,,,/AvalonDock.Themes.VS2013;component/DarkTheme.xaml");
                Application.Current.Resources.MergedDictionaries[1].Source
                    = new Uri("pack://application:,,,/NFCT.Themes;component/DarkTheme.xaml");
            }

            EventHelper.Pub<ThemeSwitchEvent, Theme>(SelectedTheme);
        }

        public void SaveProject()
        {
            if (CurrentProject == null)
                return;
            CurrentProject.Save();

        }

        public void CloseProject()
        {
            if (CurrentProject == null) return;
            ActiveGraph = null;
            OpenedGraphs.Clear();
            EventHelper.Pub<ProjectCloseEvent, Project>(CurrentProject);
            CurrentProject = null;
            CloseProjectCommand.RaiseCanExecuteChanged();
        }

        public void BuildAll()
        {
            if(CurrentProject == null) return;
            CurrentProject.Build();
            CurrentProject.Save();
        }

        public void OnOpenGraph(Graph graph)
        {
            foreach (var gvm in OpenedGraphs)
            {
                if (gvm.Graph == graph)
                {
                    ActiveGraph = gvm;
                    return;
                }
            }

            OpenedGraphs.Add(new GraphPaneViewModel(graph));
            ActiveGraph = OpenedGraphs.Last();
        }

        public void OnCloseGraph(Graph graph)
        {
            GraphPaneViewModel? vm = null;
            foreach (var gvm in OpenedGraphs)
            {
                if (gvm.Graph == graph)
                {
                    vm = gvm;
                }
            }

            if (vm != null)
            {
                OpenedGraphs.Remove(vm);
            }

            //Logger.DBG($"{ActiveGraph.Name}");
            ActiveGraph = null;
        }

        public bool CloseWindow()
        {
            return true;
        }

        public void Redo()
        {
            if(ActiveGraph == null) return;
            ActiveGraph.UndoRedoManager.Redo();
            ActiveGraph.Graph.Build();
        }

        public void Undo()
        {
            if (ActiveGraph == null) return;
            ActiveGraph.UndoRedoManager.Undo();
            ActiveGraph.Graph.Build();
        }
    }
}
