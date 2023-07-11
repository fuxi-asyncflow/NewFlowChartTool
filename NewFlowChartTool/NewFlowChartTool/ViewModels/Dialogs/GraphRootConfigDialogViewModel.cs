using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart;
using FlowChart.Core;
using FlowChart.Type;
using Microsoft.Msagl.Core.DataStructures;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    internal class GraphRootConfigDialogViewModel: BindableBase, IDialogAware
    {
        public GraphRootConfigDialogViewModel()
        {
            TypeNames = new ObservableCollection<string>();
            SaveCommand = new DelegateCommand(Save);
        }
        public static string NAME = nameof(GraphRootConfigDialogViewModel);
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            return;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            config = parameters.GetValue<GraphRootConfig>("graph_root");
            project = parameters.GetValue<Project>("project");
            Name = config.Name;
            Path = config.Path;
            TypeName = config.DefaultType;
            OutputPath = config.OutputPath;

            TypeNames.Clear();
            TypeNames.AddRange(project.GetCustomTypeNames());
        }

        public string Title => "Graph Root Config";
        public event Action<IDialogResult>? RequestClose;

        ///////////////////////////////////////////////////////////////////////////////////////

        GraphRootConfig config;
        Project project;
        public ObservableCollection<string> TypeNames { get; set; }

        public DelegateCommand? SaveCommand { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private string _outputPath;
        public string OutputPath
        {
            get => _outputPath;
            set => SetProperty(ref _outputPath, value);
        }

        private string _typeName;
        public string TypeName
        {
            get => _typeName;
            set => SetProperty(ref _typeName, value);
        }

        private void Save()
        {
            var newName = Name.Trim();
            project.RenameRoot(config.Name, newName);
            config.Path = Path.Trim();
            config.OutputPath = OutputPath.Trim();
            if(TypeName != null)
                config.DefaultType = TypeName.Trim();
            project.Save();
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }
    }
}
