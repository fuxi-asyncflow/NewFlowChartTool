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
using Prism.Events;
using Prism.Ioc;
using NewFlowChartTool.ViewModels;
using NFCT.Common;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for ProjectPanel.xaml
    /// </summary>
    public partial class ProjectPanel : UserControl
    {        
        public ProjectPanel()
        {            
            InitializeComponent();            
        }

        private void ProjectTreeItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TreeViewItem)) return;

            if (!((TreeViewItem)sender).IsSelected) return;

            var dc = WPFHelper.GetDataContext<ProjectTreeItemViewModel>(sender);
            if (dc == null) return;

            dc.Open();
            
            //ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<EventType.GraphOpenEvent>().Publish(dc.)

            e.Handled = true;
        }
    }
}
