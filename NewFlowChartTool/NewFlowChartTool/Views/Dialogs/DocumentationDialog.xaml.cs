using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlowChart.Common;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace NewFlowChartTool.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for DocumentionDialog.xaml
    /// </summary>
    public partial class DocumentationDialog : UserControl
    {
        private WebView2 _wv2;
        private static CoreWebView2Environment? _wv2Env;
        public DocumentationDialog()
        {
            Logger.LOG("create DocumentationDialog");
            InitializeComponent();
            Loaded += delegate
            {
                var window = Window.GetWindow(this);
                if (window == null)
                    return;
                window.Height = 1366;
                window.Width = 768;
            };
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            var webView = new WebView2();
            if (_wv2Env == null)
            {
                _wv2Env = await CoreWebView2Environment.CreateAsync(default, "tmp").ConfigureAwait(true);
            }

            webView.Loaded += async (sender, e) =>
            {
                await webView.EnsureCoreWebView2Async(_wv2Env).ConfigureAwait(true);
                webView.Source = new System.Uri("http://10.240.161.169/nfc_doc/index.html");
            };
            _wv2 = webView;
            DlgDockPanel.Children.Add(webView);
        }

        private void txtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _wv2.NavigateToString(txtUrl.Text);
        }

        private void wbSample_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            txtUrl.Text = e.Uri.OriginalString;
        }

        private void BrowseBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_wv2 != null) && (_wv2.CanGoBack));
        }

        private void BrowseBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _wv2.GoBack();
        }

        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_wv2 != null) && (_wv2.CanGoForward));
        }

        private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _wv2.GoForward();
        }

        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GoToPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _wv2.NavigateToString(txtUrl.Text);
        }
	}
}
