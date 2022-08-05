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
using System.Security.AccessControl;
using FlowChart.AST;
using FlowChart.LuaCodeGen;
using FlowChart.Parser;
using NFCT.Common;
using NFCT.Graph.ViewModels;
using Prism.Services.Dialogs;
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
        public enum Theme
        {
            Dark = 0,
            Light = 1,
        }
        public MainWindowViewModel(IEventAggregator ea)
        {
            _testText = "Hello world";
            _ea = ea;
            _openedGraphs = new ObservableCollection<GraphPaneViewModel>();
            CurrentProject = null;


            OpenProjectCommand = new DelegateCommand(OpenProject, () => true);
            SwitchThemeCommand = new DelegateCommand(SwitchTheme, () => true);

            _ea.GetEvent<Event.GraphOpenEvent>().Subscribe(OnOpenGraph);
#if DEBUG
            //OpenProject();
#endif
        }

        public Project? CurrentProject { get; set; }

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
        public DelegateCommand SwitchThemeCommand { get; private set; }
        #endregion

        public void OpenProject()
        {
            //var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            //var p = new FlowChart.Core.Project(new ProjectFactory.LegacyProjectFactory());
            //var p = new FlowChart.Core.Project(new ProjectFactory.MemoryProjectFactory());
            //var p = new FlowChart.Core.Project(new DefaultProjectFactory());
            //p.Path = @"F:\asyncflow\asyncflow_new\test\flowchart";
            //p.Load();
            //p.Builder = new Builder(new FlowChart.Parser.Parser(), new CodeGenFactory());
            //_ea.GetEvent<Event.ProjectOpenEvent>().Publish(p);

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
            _ea.GetEvent<Event.ProjectOpenEvent>().Publish(p);
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
    }
}
