using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using FlowChart.Core;
using FlowChart.Common;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Events;
using Prism.Mvvm;

namespace NewFlowChartTool.ViewModels
{
    public class ProjectHistory : BindableBase
    {
        public class GraphInfo : BindableBase
        {
            public GraphInfo()
            {
                FullPath = string.Empty;
            }

            private string _fullPath;
            public string FullPath
            {
                get => _fullPath;
                set => SetProperty(ref _fullPath, value);
            }

            private string? _description;
            public string? Description
            {
                get => _description;
                set => SetProperty(ref _description, value);
            }
        }

        public ProjectHistory() : this(string.Empty)
        {
        }

        public ProjectHistory(string projectPath)
        {
            ProjectPath = projectPath;
            RecentOpenedGraphs = new ObservableCollection<GraphInfo>();
            OpenedGraphs = new List<string>();
        }

        private const int MAX_RECENT_GRAPH_COUNT = 20;

        public string ProjectPath { get; set; }
        public ObservableCollection<GraphInfo> RecentOpenedGraphs { get; set; }
        public List<string> OpenedGraphs { get; set; } // Opened Charts when closing
        

        public void Open(Graph graph)
        {
            var graphInfo = RecentOpenedGraphs.ToList().Find(g => g.FullPath == graph.Path);
            if (graphInfo != null)
            {
                RecentOpenedGraphs.Move(RecentOpenedGraphs.IndexOf(graphInfo), 0);
            }
            else
            {
                graphInfo = new GraphInfo() { FullPath = graph.Path, Description = graph.Description };
                RecentOpenedGraphs.Insert(0, graphInfo);
            }
        }
    }

    public class HistoryData
    {
        public HistoryData()
        {
            RecentProjects = new ObservableCollection<ProjectHistory>();
        }
        public ObservableCollection<ProjectHistory> RecentProjects { get; set; }
    }

    internal class StartPageViewModel : BindableBase
    {
        public static string TmpFolderPath = "./tmp";
        public static JsonSerializerOptions JsonSerializerOption { get; set; }

        static StartPageViewModel()
        {
            JsonSerializerOption = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
        }
        public StartPageViewModel()
        {
            EventHelper.Sub<ProjectOpenEvent, Project>(OnProjectOpen);
            EventHelper.Sub<GraphOpenEvent, Graph>(OnGraphOpen);
            EventHelper.Sub<ProjectCloseEvent, Project>(OnProjectClose);
            HistoryData = new HistoryData();
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var ph = new ProjectHistory("A:\\test.project");
                ph.RecentOpenedGraphs.Add(new ProjectHistory.GraphInfo(){FullPath = "AI.test"});
                ph.RecentOpenedGraphs.Add(new ProjectHistory.GraphInfo() { FullPath = "Gameplay.test" });
                HistoryData.RecentProjects.Add(ph);
                return;
            }
#endif
            try
            {
                ReadHistoryFile();
            }
            catch (Exception e)
            {
                Logger.ERR($"read history data failed: {e.Message}");
            }

        }

        void CreateTmpFolder(string path)
        {
            var di = new DirectoryInfo(path);
            if(!di.Exists)
                di.Create();
        }

        void ReadHistoryFile()
        {
            CreateTmpFolder(TmpFolderPath);
            var historyFile = $"{TmpFolderPath}/history.json";
            if (File.Exists(historyFile))
            {
                var jsonStr = File.ReadAllText(historyFile);
                var data = JsonSerializer.Deserialize<HistoryData>(jsonStr);
                if (data != null)
                    HistoryData = data;
            }
        }

        void OnProjectOpen(Project project)
        {
            var projects = HistoryData.RecentProjects;
            var projectHistory = projects.ToList().Find(p => p.ProjectPath == project.Path);
            if (projectHistory != null)
            {
                projects.Move(projects.IndexOf(projectHistory), 0);
            }
            else
            {
                projectHistory = new ProjectHistory(project.Path);
                projects.Insert(0, projectHistory);
            }
        }

        void OnProjectClose(Project project)
        {
            var historyFile = $"{TmpFolderPath}/history.json";
            var data = JsonSerializer.Serialize<HistoryData>(HistoryData, JsonSerializerOption);
            FileHelper.Save(historyFile, data);
        }

        void OnGraphOpen(Graph graph)
        {
            if (graph.Project.Path != HistoryData.RecentProjects.First().ProjectPath)
            {
                OnProjectOpen(graph.Project);
            }
            var projectHistory = HistoryData.RecentProjects.First();
            projectHistory.Open(graph);
            
        }
        
        public HistoryData HistoryData { get; set; }
    }
}
