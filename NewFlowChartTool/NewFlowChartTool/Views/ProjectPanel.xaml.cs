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
using NFCT.Common.ViewModels;
using NFCT.Common.Views;
using NFCT.Graph.ViewModels;

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

        //private void ProjectTreeItem_OnNameVisibleChange(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var vm = WPFHelper.GetDataContext<ProjectTreeItemViewModel>(sender);
        //    if (vm == null)
        //        return;
        //    var stack = WPFHelper.GetVisualParent<StackPanel>(sender);
        //    Console.WriteLine($"STACK: {sender.GetHashCode()} ${stack.GetHashCode()}");
        //    if (stack == null) return;

        //    bool visible = (bool)e.NewValue;
        //    var seb = ContainerLocator.Current.Resolve<SimpleEditBox>();
        //    if (visible)
        //    {
        //        seb.DataContext = null;
        //        stack.Children.Remove(seb);
        //    }
        //    else
        //    {
        //        seb.AddToPanel(stack);
                
        //        seb.DataContext ??= ContainerLocator.Current.Resolve<SimpleEditBoxViewModel>();
        //        if (seb.DataContext is SimpleEditBoxViewModel sebVm)
        //        {
        //            sebVm.Text = vm.Name;
        //            seb.SelectText(vm.Name);
        //        }

        //        seb.SetFocus();
        //        seb.OnExit = (box, save) =>
        //        {
        //            var sebVm = box.DataContext as SimpleEditBoxViewModel;
        //            if (sebVm == null)
        //                return;
        //            var text = sebVm.Text;
        //            vm.ExitingRenameMode(text, save);
        //        };
        //    }
        //}
    }
}
