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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using FlowChart.AST;
using FlowChart.Debug;
using FlowChart.LuaCodeGen;
using FlowChart.Misc;
using FlowChart.Parser;
using FlowChart.Common;
using FlowChart.Common.Report;
using Microsoft.Win32;
using NewFlowChartTool.Event;
using NewFlowChartTool.Utility;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Common.Services;
using NFCT.Diff.ViewModels;
using NFCT.Graph.ViewModels;
using Prism.Ioc;
using Prism.Services.Dialogs;
using ProjectFactory.DefaultProjectFactory;

namespace NewFlowChartTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        
        public MainWindowViewModel(IDialogService dialogService, IStatusBarService statusBarService)
        {
            if (Inst == null)
                Inst = this;
            _testText = "Hello world";
            
            _dialogService = dialogService;
            _statusBarService = statusBarService;
            _openedGraphs = new ObservableCollection<BindableBase>();
            CurrentProject = null;

            ActualLeftDockWidth = 245;
            ActualBottomHeight = 245;
            IsLeftDockVisible = true;
            IsBottomDockVisible = true;
            

            OpenProjectCommand = new DelegateCommand(ChooseProjectPath, () => CurrentProject == null);
            SaveProjectCommand = new DelegateCommand(SaveProject, () => CurrentProject != null);
            NewProjectCommand = new DelegateCommand(NewProject, () => CurrentProject == null);
            CloseProjectCommand = new DelegateCommand(CloseProject, () => CurrentProject != null);
            ExplorerToProjectCommand = new DelegateCommand(ExplorerToProject, () => CurrentProject != null);
            ConfigProjectCommand = new DelegateCommand(ConfigProject, () => CurrentProject != null);
            TypeDialogCommand = new DelegateCommand(ShowTypeDialog, () => CurrentProject != null);
            BuildAllCommand = new DelegateCommand(BuildAll, () => CurrentProject != null);
            SwitchThemeCommand = new DelegateCommand(SwitchTheme, () => true);
            SwitchLangCommand = new DelegateCommand(SwitchLang, () => true);

            QuickDebugCommand = new DelegateCommand(QuickDebug);
            ShowDebugDialogCommand = new DelegateCommand(ShowDebugDialog);
            StopDebugCommand = new DelegateCommand(StopDebug);
            HotfixCommand = new DelegateCommand(Hotfix);
            PatchGraphCommand = new DelegateCommand(PatchGraph);
            PatchNodesCommand = new DelegateCommand(PatchNodes);

            ScreenShotCommand = new DelegateCommand(ScreenShot);
            SearchNodeCommand = new DelegateCommand(SearchNode);
            DiffCommand = new DelegateCommand(Diff);
            ShowDocumentationDialogCommand = new DelegateCommand(ShowDocumentationDialog);
            TestCommand = new DelegateCommand(Test);

            SelectedLang = Lang.Chinese;
            SelectedTheme = AppConfig.Inst.LightTheme ? Theme.Light : Theme.Dark;

            UndoCommand = new DelegateCommand(Undo);
            RedoCommand = new DelegateCommand(Redo);

            SwitchLeftDockVisibilityCommand = new DelegateCommand(SwitchLeftDockVisibility);
            SwitchBottomDockVisibilityCommand = new DelegateCommand(SwitchBottomDockVisibility);

            EventHelper.Sub<GraphCloseEvent, Graph>(OnCloseGraph);
            EventHelper.Sub<NewDebugAgentEvent, DebugAgent>(OnNewDebugAgentEvent, ThreadOption.UIThread);
            EventHelper.Sub<StartDebugGraphEvent, GraphInfo?>(OnStartDebugGraphEvent, ThreadOption.UIThread);

#if DEBUG
            //TestOpenProject();
#endif
        }

        public static MainWindowViewModel? Inst;

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
                NewProjectCommand.RaiseCanExecuteChanged();
                CloseProjectCommand.RaiseCanExecuteChanged();
                ExplorerToProjectCommand.RaiseCanExecuteChanged();
                ConfigProjectCommand.RaiseCanExecuteChanged();
                BuildAllCommand.RaiseCanExecuteChanged();
                TypeDialogCommand.RaiseCanExecuteChanged();
            }
        }

        
        readonly IDialogService _dialogService;
        readonly IStatusBarService _statusBarService;

        public string _testText;
        public string TestText { get => _testText; set { SetProperty<string>(ref _testText, value); } }

        public Theme SelectedTheme;
        public Lang SelectedLang;

        public BindableBase? _activeDoc;
        public GraphPaneViewModel? ActiveGraph { get => _activeDoc as GraphPaneViewModel; set => ActiveDoc = value; }
        public BindableBase? ActiveDoc
        {
            get => _activeDoc;
            set => SetProperty(ref _activeDoc, value, nameof(ActiveDoc));
        }

        private ObservableCollection<BindableBase> _openedGraphs;    
        public ObservableCollection<BindableBase> OpenedGraphs
        {
            get { return _openedGraphs; }
        }

        public void ForEachOpenedGraphs(Action<GraphPaneViewModel> action)
        {
            foreach (var vm in _openedGraphs)
            {
                if(vm is GraphPaneViewModel gvm)
                    action.Invoke(gvm);
            }
        }

        #region Layout
        public double ActualLeftDockWidth { set { if (value > 1.0) _leftDockWidth = value; } }

        private double _leftDockWidth;
        public double LeftDockWidth
        {
            get => IsLeftDockVisible ? _leftDockWidth : 0.0;
            set => ActualLeftDockWidth = value;
        }

        private bool _isLeftDockVisible;
        public bool IsLeftDockVisible
        {
            get => _isLeftDockVisible;
            set
            {
                SetProperty(ref _isLeftDockVisible, value);
                RaisePropertyChanged(nameof(LeftDockWidth));
            }
        }

        public double ActualBottomHeight { set { if (value > 1.0) _bottomDockHeight = value; } }

        private double _bottomDockHeight;
        public double BottomDockHeight
        {
            get => IsBottomDockVisible ? _bottomDockHeight : 0.0;
            set => ActualBottomHeight = value;
        }

        private bool _isBottomDockVisible;
        public bool IsBottomDockVisible
        {
            get => _isBottomDockVisible;
            set
            {
                SetProperty(ref _isBottomDockVisible, value);
                RaisePropertyChanged(nameof(BottomDockHeight));
            }
        }

        private void SwitchLeftDockVisibility()
        {
            IsLeftDockVisible = !IsLeftDockVisible;
        }

        public DelegateCommand SwitchLeftDockVisibilityCommand { get; set; }

        private void SwitchBottomDockVisibility()
        {
            IsBottomDockVisible = !IsBottomDockVisible;
        }

        public DelegateCommand SwitchBottomDockVisibilityCommand { get; set; }

        #endregion

        #region COMMAND
        public DelegateCommand OpenProjectCommand { get; private set; }
        public DelegateCommand SaveProjectCommand { get; private set; }
        public DelegateCommand NewProjectCommand { get; private set; }
        public DelegateCommand CloseProjectCommand { get; private set; }
        public DelegateCommand ExplorerToProjectCommand { get; private set; }
        public DelegateCommand ConfigProjectCommand { get; private set; }
        public DelegateCommand TypeDialogCommand { get; private set; }
        public DelegateCommand SwitchThemeCommand { get; private set; }
        public DelegateCommand SwitchLangCommand { get; private set; }
        public DelegateCommand BuildAllCommand { get; private set; }

        public DelegateCommand UndoCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }

        public DelegateCommand QuickDebugCommand { get; private set; }
        public DelegateCommand ShowDebugDialogCommand { get; private set; }
        public DelegateCommand StopDebugCommand { get; private set; }
        public DelegateCommand HotfixCommand { get; private set; }
        public DelegateCommand PatchGraphCommand { get; private set; }
        public DelegateCommand PatchNodesCommand { get; private set; }

        public DelegateCommand ScreenShotCommand { get; set; }
        public DelegateCommand SearchNodeCommand { get; set; }
        public DelegateCommand DiffCommand { get; set; }

        public DelegateCommand ShowDocumentationDialogCommand { get; set; }

        public DelegateCommand TestCommand { get; set; }
        public ObservableCollection<CustomMenuItemViewModel> CustomMenuItems => WPFPluginManager.Inst.Menus;

        public void TestOpenProject()
        {
            //var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            //var p = new FlowChart.Core.Project(new ProjectFactory.LegacyProjectFactory());
            var p = new FlowChart.Core.Project(new ProjectFactory.MemoryProjectFactory());
            //var p = new FlowChart.Core.Project(new DefaultProjectFactory());
            p.Path = @"F:\asyncflow\asyncflow_new\test\flowchart";
            p.Load();
            EventHelper.Pub<ProjectOpenEvent, Project>(p);
            p.Save();
            CurrentProject = p;
        }
        #endregion

        async void ChooseProjectPath()
        {
            if (CurrentProject != null)
            {
                MessageBox.Show("please close project before open another project");
                return;
            }

            var dlg = new OpenFileDialog();
            dlg.FileName = "project.json";
            dlg.DefaultExt = ".json";
            dlg.Filter = "AsyncFlow Project|project.json";
            var result = dlg.ShowDialog();
            if (result != true)
                return;
            var fi = new FileInfo(dlg.FileName);
            if (fi.Name != "project.json")
            {
                MessageBox.Show("please choose file: `project.json`");
                return;
            }

            var folderPath = new FileInfo(dlg.FileName).DirectoryName;
            if (folderPath == null)
            {
                MessageBox.Show($"open project failed: folder of {dlg.FileName} not exist");
                return;
            }
            //var folderPath = Dialogs.ChooseFolder();
            //if (folderPath == null)
            //    return;
            OpenProject(folderPath);
        }

        public async void OpenProject(string projectPath)
        {
            if (CurrentProject != null)
            {
                if (CurrentProject.Path == projectPath)
                    return;
                MessageBox.Show("another project is opened, please close it");
                return;
            }

            var p = new Project();
            p.Path = projectPath;
            p.IsAsyncLoad = true;
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
            EventHelper.Pub<ProjectOpenEvent, Project>(p);
            WPFPluginManager.Inst.Report(new OpenProjectEvent(p.Path));

            if (p.IsAsyncLoad)
            {
                int count = 0;
                _statusBarService.BeginProgress(p.GraphDict.Count, "loading graphs ...");
                var sw = Stopwatch.StartNew();
                foreach (var item in p.GraphDict.Values)
                {
                    if (item is Graph graph)
                    {
                        await Task.Run(graph.LazyLoad);
                    }
                    _statusBarService.UpdateProgress(++count);
                }
                Logger.LOG($"lazy load graph : {sw.ElapsedMilliseconds}");
                //var loadAllGraphTask = Task.WhenAll(p.GraphDict.Values.ToList().ConvertAll(graph => Task.Run(graph.LazyLoadFunc)));
                //await loadAllGraphTask;
                _statusBarService.EndProgress();
            }
        }

        public void NewProject()
        {
            var folderPath = Dialogs.ChooseFolder();
            if (folderPath == null)
                return;

            var p = DefaultProjectFactory.CreateNewProject(folderPath);
            CurrentProject = p;
            EventHelper.Pub<ProjectOpenEvent, Project>(p);
            p.Save();
        }

        public void SwitchTheme()
        {
            var app = Application.Current as App;
            if (app == null)
                return;
            if(SelectedTheme == Theme.Dark)
            {
                SelectedTheme = Theme.Light;
                app.SetTheme(true);
                //Application.Current.Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/VS2013Test;component/Themes/DarkBrushsExtended.xaml");

            }
            else if(SelectedTheme == Theme.Light)
            {
                SelectedTheme = Theme.Dark;
                app.SetTheme(false);
            }
            
            EventHelper.Pub<ThemeSwitchEvent, Theme>(SelectedTheme);

            var buttons = new List<DialogButton>();
            buttons.Add(new DialogButton("重启", ButtonResult.OK));
            buttons.Add(new DialogButton("稍后", ButtonResult.Cancel));

            var parameters = new DialogParameters();
            parameters.Add("buttons", buttons);
            parameters.Add("message", "部分图标与控件的颜色需要重启后才能生效，是否现在重启");

            _dialogService.ShowDialog(CustomMessageDialogViewModel.NAME, parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    //Process.Start(Application.ResourceAssembly.Location);
                    System.Windows.Forms.Application.Restart();
                    app.Shutdown();
                }
            });
        }

        public void SwitchLang()
        {
            if (SelectedLang == Lang.English)
            {
                SelectedLang = Lang.Chinese;
                Application.Current.Resources.MergedDictionaries[4].Source
                    = new Uri("pack://application:,,,/NFCT.Common;component/Localization/Chinese.xaml");
            }
            else if (SelectedLang == Lang.Chinese)
            {
                SelectedLang = Lang.English;
                Application.Current.Resources.MergedDictionaries[4].Source
                    = new Uri("pack://application:,,,/NFCT.Common;component/Localization/English.xaml");
            }

            EventHelper.Pub<LangSwitchEvent, Lang>(SelectedLang);
        }

        public void SaveProject()
        {
            if (CurrentProject == null)
                return;
            Logger.LOG($"save project");
            CurrentProject.Save();
            ForEachOpenedGraphs(graphVm =>
            {
                if (graphVm.IsDirty)
                    graphVm.IsDirty = false;
            });
        }

        public void CloseProject()
        {
            if (CurrentProject == null) return;
            ActiveGraph = null;
            //OpenedGraphs.Clear();
            while (OpenedGraphs.Count > 0)
            {
                OpenedGraphs.RemoveAt(0);
            }
            EventHelper.Pub<ProjectCloseEvent, Project>(CurrentProject);
            CurrentProject = null;
            CloseProjectCommand.RaiseCanExecuteChanged();
        }

        void ExplorerToProject()
        {
            if (CurrentProject == null) return;
            var folderPath = CurrentProject.Path;
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
        }

        void ConfigProject()
        {
            if (CurrentProject == null) return;
            var dialogParameters = new DialogParameters();
            dialogParameters.Add(nameof(CurrentProject), CurrentProject);
            _dialogService.Show(ProjectConfigDialogViewModel.NAME, dialogParameters, result => { });
        }

        public async void BuildAll()
        {
            if(CurrentProject == null) return;
            ContainerLocator.Current.Resolve<OutputPanelViewModel>().Clear();
            _statusBarService.BeginProgress(CurrentProject.GraphDict.Count, "build project");
            int count = 0;
            Action<Item> updateProgressBar = (item) => _statusBarService.UpdateProgress(++count);
            CurrentProject.OnGraphBuildEvent += updateProgressBar;
            await Task.Factory.StartNew(delegate { CurrentProject.Build(); });
            CurrentProject.OnGraphBuildEvent -= updateProgressBar;
            _statusBarService.EndProgress();
            CurrentProject.Save();
        }

        public void ShowTypeDialog()
        {
            if(CurrentProject == null) return;
            var dialogParameters = new DialogParameters();
            dialogParameters.Add(nameof(CurrentProject), CurrentProject);
            _dialogService.Show(TypeDialogViewModel.NAME, dialogParameters, result => { });
        }

        void QuickDebug()
        {
            if (ActiveGraph == null)
                return;
            ContainerLocator.Current.Resolve<IDebugService>().QuickDebug(ActiveGraph.Graph);
        }

        public void ShowDebugDialog()
        {
            var vm = DebugDialogViewModel.Inst;
            if(vm == null || !vm.IsOpened)
                _dialogService.Show(DebugDialogViewModel.NAME);
        }

        public void StopDebug()
        {
            DebugDialogViewModel.Inst?.StopDebug();
            ForEachOpenedGraphs(graphVm =>
            {
                graphVm.ExitDebugMode();
            });
        }

        void Hotfix()
        {
            if (ActiveGraph == null)
                return;
            var lines = new List<string>();
            var codes = new List<string>();
            var graphs = new List<Graph>() { ActiveGraph.Graph };
            var hotfixStr = CurrentProject?.Factory.HotFix(graphs);
            if (hotfixStr != null)
            {
                FileHelper.Save("./tmp/hotfix.txt", hotfixStr);
                ContainerLocator.Current.Resolve<IDebugService>().Hotfix(hotfixStr);
            }
        }

        void PatchGraph()
        {
            if (ActiveGraph == null)
                return;

            var codes = CurrentProject?.Factory.GraphPatch(ActiveGraph.Graph);
            FileHelper.Save("tmp/patch.txt", codes);
        }

        void PatchNodes()
        {
            if (ActiveGraph == null)
                return;
            ActiveGraph.Build();
            var nodes = ActiveGraph.SelectedNodes.ConvertAll(vm => vm.Node);
            var codes = CurrentProject?.Factory.NodesPatch(ActiveGraph.Graph, nodes);
            codes.ForEach(Console.WriteLine);
            FileHelper.Save("tmp/patch.txt", codes);
        }

        void ScreenShot()
        {
            if (ActiveGraph == null)
                return;
            ActiveGraph.ScreenShot();
        }

        void SearchNode()
        {
            if (CurrentProject == null)
                return;
            _dialogService.Show(InputDialogViewModel.NAME, result =>
            {
                if (result.Result != ButtonResult.OK)
                    return;
                var searchText = result.Parameters.GetValue<string>("Value");
                SearchNode(searchText);
            });
        }

        void SearchNode(string text)
        {
            if (CurrentProject == null)
                return;
            text = text.Trim().ToLower();
            var output = ContainerLocator.Current.Resolve<OutputPanelViewModel>();
            foreach (var item in CurrentProject.GraphDict.Values)
            {
                if (item is Graph graph)
                {
                    graph.Nodes.ForEach(node =>
                    {
                        if (node is TextNode textNode)
                        {
                            if (textNode.Text.ToLower().Contains(text))
                                output.Output(textNode.Text, OutputMessageType.Default, node);
                        }
                    });
                }
            }
        }

        void Diff()
        {
            var vcpVm = ContainerLocator.Current.Resolve<VersionControlPanelViewModel>();
            foreach (var vm in OpenedGraphs)
            {
                if (vm == vcpVm)
                {
                    ActiveDoc = vm;
                    return;
                }
            }

            if (CurrentProject != null)
            {
                OpenedGraphs.Add(vcpVm);
                vcpVm.OpenProject(CurrentProject);
                ActiveDoc = vcpVm;
            }
        }

        void ShowDocumentationDialog()
        {
            _dialogService.Show(DocumentationDialogViewModel.NAME);
        }

        void Test()
        {
            
        }

        public async void OpenGraph(string path)
        {
            if (CurrentProject == null)
                return;
            var graph = CurrentProject.GetGraph(path);
            if (graph == null)
                return;
            OnOpenGraph(graph);
        }

        public bool IsOpened(Graph graph)
        {
            foreach (var vm in OpenedGraphs)
            {
                if (vm is GraphPaneViewModel graphVm)
                {
                    if (graphVm.Graph == graph)
                        return true;
                }
            }
            return false;
        }

        public async void OnOpenGraph(Graph graph)
        {
            OnOpenGraph(graph, null);
        }

        public async void OnOpenGraph(Graph graph, Node? centerNode)
        {
            EventHelper.Pub<GraphOpenEvent, Graph>(graph);
            WPFPluginManager.Inst.Report(new OpenGraphEvent(graph.Path));
            bool isOpened = false;
            ForEachOpenedGraphs(gvm =>
            {
                if (gvm.Graph == graph)
                {
                    ActiveGraph = gvm;
                    ActiveGraph.MoveNodeToCenter(centerNode);
                    isOpened = true;
                }
            });
            if (isOpened)
                return;

            if (!graph.IsLoaded && graph.LazyLoadCompletionSource != null)
                await graph.LazyLoadCompletionSource.Task;

            OpenedGraphs.Add(new GraphPaneViewModel(graph));
            if (OpenedGraphs.Last() is GraphPaneViewModel graphVm)
            {
                ActiveGraph = graphVm;
                ActiveGraph.InitCenterNode = centerNode;
                ActiveGraph.Build();
            }
        }

        public void OnCloseGraph(Graph graph)
        {
            GraphPaneViewModel? vm = null;
            foreach (var v in OpenedGraphs)
            {
                if(v is GraphPaneViewModel gvm && gvm.Graph == graph)
                {
                    vm = gvm;
                }
            }

            if (vm != null)
            {
                vm.ExitDebugMode();
                OpenedGraphs.Remove(vm);
            }

            //Logger.DBG($"{ActiveGraph.Name}");
            ActiveGraph = null;
        }

        public bool CloseWindow()
        {
            if(CurrentProject != null)
                CloseProject();
            return true;
        }

        void Redo()
        {
            if(ActiveGraph == null) return;
            ActiveGraph.UndoRedoManager.Redo();
            ActiveGraph.Build();
        }

        void Undo()
        {
            if (ActiveGraph == null) return;
            ActiveGraph.UndoRedoManager.Undo();
            ActiveGraph.Build();
        }

        void OnNewDebugAgentEvent(DebugAgent agent)
        {
            ForEachOpenedGraphs(graphVm =>
            {
                if (graphVm.IsDebugMode && graphVm.FullPath == agent.GraphName)
                    graphVm.UpdateAgents(DebugDialogViewModel.Inst.GetAgents(agent.GraphName));
            });
        }

        void OnStartDebugGraphEvent(GraphInfo? graphInfo)
        {
            if (CurrentProject == null) return;
            if (graphInfo == null && ActiveGraph != null) // quick debug
            {
                ActiveGraph.EnterDebugMode(graphInfo);
                return;
            }
            if (CurrentProject.GraphDict.TryGetValue(graphInfo.GraphName, out var item))
            {
                if(item is Graph graph)
                    OnOpenGraph(graph);
            }

            ForEachOpenedGraphs(graphVm =>
            {
                if (graphVm.Graph == item)
                {
                    graphVm.EnterDebugMode(graphInfo);
                    graphVm.UpdateAgents(DebugDialogViewModel.Inst.GetAgents(graphInfo.GraphName));
                }
            });
        }
    }
}
