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

namespace NewFlowChartTool.ViewModels
{
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

            OpenProjectCommand = new DelegateCommand(OpenProject, () => true);
            SwitchThemeCommand = new DelegateCommand(SwitchTheme, () => true);
#if DEBUG
            OpenProject();
#endif
        }



        readonly IEventAggregator _ea;

        public string _testText;
        public string TestText { get => _testText; set { SetProperty<string>(ref _testText, value); } }

        public Theme SelectedTheme;
        

        #region COMMAND
        public DelegateCommand OpenProjectCommand { get; private set; }
        public DelegateCommand SwitchThemeCommand { get; private set; }
        #endregion

        public void OpenProject()
        {
            var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            p.Path = @"D:\git\asyncflow_new\test\flowchart";
            p.Load();
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
    }
}
