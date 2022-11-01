using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Common.Services;
using Prism.Events;
using Prism.Mvvm;

namespace NewFlowChartTool.ViewModels
{
    internal class MainWindowStatusBarViewModel : BindableBase, IStatusBarService
    {
        public MainWindowStatusBarViewModel()
        {
            EventHelper.Sub<StatusBarProgressEnableEvent, Tuple<int, string>>(tuple =>
            {
                if (string.IsNullOrEmpty(tuple.Item2))
                    EnableProgressBar = false;
                else
                    EnableProgressBar = true;
            }, ThreadOption.UIThread);

            EventHelper.Sub<StatusBarProgressUpdateEvent, int>(v =>
            {
                ProgressBarValue = v;
            });
        }
        private int _progressBarMax;
        public int ProgressBarMax { get => _progressBarMax; set => SetProperty(ref _progressBarMax, value); }

        private int _progressBarValue;
        public int ProgressBarValue { get => _progressBarValue; set => SetProperty(ref _progressBarValue, value); }
        private bool _enableProgressBar;
        public bool EnableProgressBar { get => _enableProgressBar; set => SetProperty(ref _enableProgressBar, value); }


        public void BeginProgress(int max, string text)
        {
            //_dispatcher.Invoke(delegate { EnableProgressBar = true; });
            EventHelper.Pub<StatusBarProgressEnableEvent, Tuple<int, string>>(new Tuple<int, string>(max, text));
        }

        public void UpdateProgress(int v)
        {
            //_dispatcher.Invoke(delegate { ProgressBarValue = v; });
            EventHelper.Pub<StatusBarProgressUpdateEvent, int>(v);
        }

        public void EndProgress()
        {
            //_dispatcher.Invoke(delegate { EnableProgressBar = false; });
            EventHelper.Pub<StatusBarProgressEnableEvent, Tuple<int, string>>(new Tuple<int, string>(0, null));
        }
    }
}
