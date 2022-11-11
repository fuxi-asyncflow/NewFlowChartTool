using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using FlowChart.Layout;
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphPanelHeaderBar.xaml
    /// </summary>
    public partial class GraphPanelHeaderBar : UserControl
    {
        public GraphPanelHeaderBar()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            foreach (var kv in LayoutManager.Instance.LayoutDict)
            {
                LayoutComboBox.Items.Add(kv.Key);
            }
            LayoutComboBox.SelectedIndex = 0;
        }

        private void LayoutComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as GraphPaneViewModel;
            if (vm == null)
                return;
            var layoutName = LayoutComboBox.SelectedItem as string;
            if (layoutName == null)
                return;
            if (LayoutManager.Instance.LayoutDict.TryGetValue(layoutName, out var layoutFactory))
            {
                var layout = layoutFactory.Invoke();
                vm.ChangeLayout(layout);
            }
        }
    }
}
