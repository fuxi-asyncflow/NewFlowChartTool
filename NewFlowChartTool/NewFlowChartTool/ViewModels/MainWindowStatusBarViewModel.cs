using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
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
            ProgressBarMax = 100;
            ProgressBarValue = 50;
            _progressBarText = "-/-";
            _progressBarDescription = string.Empty;
        }
        private int _progressBarMax;
        public int ProgressBarMax { get => _progressBarMax; set => SetProperty(ref _progressBarMax, value); }

        private int _progressBarValue;
        public int ProgressBarValue { get => _progressBarValue; set => SetProperty(ref _progressBarValue, value); }

        private bool _enableProgressBar;
        public bool EnableProgressBar { get => _enableProgressBar; set => SetProperty(ref _enableProgressBar, value); }

        private string _progressBarText;
        public string ProgressBarText { get => _progressBarText; set => SetProperty(ref _progressBarText, value); }

        private string _progressBarDescription;
        public string ProgressBarDescription { get => _progressBarDescription; set => SetProperty(ref _progressBarDescription, value); }


        public void BeginProgress(int max, string text)
        {
            InvokeIfNecessary(() =>
            {
                EnableProgressBar = true;
                ProgressBarMax = max;
                ProgressBarDescription = text;
            });
        }

        public void UpdateProgress(int v)
        {
            InvokeIfNecessary(() =>
            {
                ProgressBarText = $"{v}/{ProgressBarMax}";
                ProgressBarValue = v;
            });
        }

        public void EndProgress()
        {
            InvokeIfNecessary(() =>
            {
                EnableProgressBar = false;
            });
        }

        public static void InvokeIfNecessary(Action action)
        {
            if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
                action();
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }
    }
}
