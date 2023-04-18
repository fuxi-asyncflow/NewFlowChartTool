using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using FlowChart;
using FlowChart.Core;
using FlowChart.Plugin;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    internal class GraphRootConfigViewModel : BindableBase
    {
        static GraphRootConfigViewModel()
        {
            TypeNames = new ObservableCollection<string>();
        }

        public GraphRootConfigViewModel(GraphRootConfig config)
        {
            _graphRootConfig = config;
        }

        private GraphRootConfig _graphRootConfig;

        public string Name
        {
            get => _graphRootConfig.Name;
            set
            {
                _graphRootConfig.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Path
        {
            get => _graphRootConfig.Path;
            set
            {
                _graphRootConfig.Path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public string OutputPath
        {
            get => _graphRootConfig.OutputPath;
            set
            {
                _graphRootConfig.OutputPath = value;
                RaisePropertyChanged(nameof(OutputPath));
            }
        }

        public string DefaultType
        {
            get => _graphRootConfig.DefaultType;
            set
            {
                _graphRootConfig.DefaultType = value;
                RaisePropertyChanged(nameof(DefaultType));
            }
        }

        public string SaveRule
        {
            get => _graphRootConfig.SaveRuleString;
            set
            {
                _graphRootConfig.SaveRuleString = value;
                RaisePropertyChanged(nameof(SaveRule));
            }
        }

        public static ObservableCollection<string> TypeNames { get; set; }

    }

    internal class ProjectConfigDialogViewModel : BindableBase, IDialogAware
    {
        public const string NAME = nameof(ProjectConfigDialogViewModel);
        private ProjectConfig? _config;
        private Project _project;

        public ObservableCollection<GraphRootConfigViewModel> GraphRoots { get; set; }

        public ProjectConfigDialogViewModel()
        {
            GraphRoots = new ObservableCollection<GraphRootConfigViewModel>();
            CodeGeneratorNames = new ObservableCollection<string>();
        }

        #region IDialogAware
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var project = parameters.GetValue<Project>("CurrentProject");
            if (project == null)
                return;
            _project = project;
            _config = project.Config;
            PrepareListBox(_project);

            GraphRoots.Clear();
            _config.GraphRoots.ForEach(root => GraphRoots.Add(new GraphRootConfigViewModel(root)));
        }

        public string Title => "Config Project";
        public event Action<IDialogResult>? RequestClose;

        #endregion

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        private string _codeGenerator;
        public string CodeGenerator
        {
            get => _codeGenerator;
            set => SetProperty(ref _codeGenerator, value);
        }

        public ObservableCollection<string> CodeGeneratorNames { get; set; }
        public ObservableCollection<string> TypeNames => GraphRootConfigViewModel.TypeNames;

        private void PrepareListBox(Project project)
        {
            CodeGeneratorNames.Clear();
            PluginManager.Inst.GetAllCodeGenerators().ForEach(CodeGeneratorNames.Add);

            TypeNames.Clear();
            project.GetCustomTypeNames().ForEach(TypeNames.Add);
        }

    }
}
