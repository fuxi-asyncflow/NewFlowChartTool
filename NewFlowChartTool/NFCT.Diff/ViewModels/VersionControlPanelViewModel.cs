using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Diff;
using NFCT.Diff.Utils;
using Prism.Commands;
using Prism.Mvvm;

namespace NFCT.Diff.ViewModels
{
    public class VersionItemViewModel : BindableBase
    {
        public VersionItemViewModel(VersionItem item)
        {
            _item = item;
        }

        private VersionItem _item;

        public string Version => _item.Version;
        public string Author => _item.Author;
        public DateTime Time => _item.Time;
        public string Message => _item.Message;
    }

    public class FileItemViewModel : BindableBase
    {
        public FileItemViewModel(string name)
        {
            Name = name;
            Graphs = new ObservableCollection<GraphItemViewModel>();
            Graphs.Add(GraphItemViewModel.DummyGraph);
        }
        public string Name { get; set; }
        public ObservableCollection<GraphItemViewModel> Graphs { get; set; }
    }

    public class GraphItemViewModel : BindableBase
    {
        static GraphItemViewModel()
        {
            DummyGraph = new GraphItemViewModel("__dummy");
        }

        public GraphItemViewModel(string name)
        {
            Name = name;
        }

        public GraphItemViewModel(DiffGraph graph)
        {
            Graph = graph;
            if (graph.OldGraph != null)
            {
                Name = graph.OldGraph.Name;
            }
            else if (graph.NewGraph != null)
            {
                Name = graph.NewGraph.Name;
            }
        }

        public static GraphItemViewModel DummyGraph { get; set; }
        public string Name { get; set; }
        public DiffGraph? Graph { get; set; }
    }

    public class VersionControlPanelViewModel: BindableBase
    {
        public VersionControlPanelViewModel()
        {
            Versions = new ObservableCollection<VersionItemViewModel>();
            Files = new ObservableCollection<FileItemViewModel>();
            OnCloseCommand = new DelegateCommand(delegate { });
            GraphPath = ".";
        }

        public string TabHeaderName => "diff";
        public DelegateCommand OnCloseCommand { get; set; }
        public ObservableCollection<VersionItemViewModel> Versions { get; set; }
        public ObservableCollection<FileItemViewModel> Files { get; set; }
        private IVersionControlTool _versionControl;
        private string GraphPath { get; set; }
        private Project? Project { get; set; }
        private string? _version { get; set; }

        //public bool OpenProject(string projectPath)
        //{
        //    var di = new System.IO.DirectoryInfo(projectPath);
        //    _versionControl = new SvnTool(projectPath);
        //}

        public bool OpenProject(Project project)
        {
            Project = project;
            var roots = project.Config.GraphRoots;
            if (roots.Count == 0)
                return true;
            _versionControl = new SvnTool("");
            _versionControl.WorkingDir = project.Path;
            
            GraphPath = project.Config.GraphRoots.First().Path;
            var versionItems = _versionControl.GetHistory(GraphPath);
            Versions.Clear();
            versionItems.ForEach(item => Versions.Add(new VersionItemViewModel(item)));
            return true;
        }

        public void ShowVersion(string version)
        {
            var files = _versionControl.GetChangedFiles(GraphPath, version);
            _version = version;
            Files.Clear();
            files.ForEach(f => Files.Add(new FileItemViewModel(f)));
        }

        public void GetChangedGraphInFile(FileItemViewModel fvm)
        {
            if (Project == null || _version == null)
                return;
            var fileName = fvm.Name;
            var files = _versionControl.ExportFiles(fileName, _version);
            var diffGraphs = CompareProject.Diff(Project, files.Item1, files.Item2);
            fvm.Graphs.Clear();
            diffGraphs.ForEach(graph => fvm.Graphs.Add(new GraphItemViewModel(graph)));
        }
        
    }
}
