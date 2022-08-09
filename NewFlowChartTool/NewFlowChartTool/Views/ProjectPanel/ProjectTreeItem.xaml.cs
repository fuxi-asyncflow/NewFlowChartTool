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
using NewFlowChartTool.ViewModels;
using NFCT.Common;
using NFCT.Common.ViewModels;
using NFCT.Common.Views;
using Prism.Ioc;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for ProjectTreeItem.xaml
    /// </summary>
    public partial class ProjectTreeItem : UserControl
    {
        public ProjectTreeItem()
        {
            InitializeComponent();
        }

        private void ProjectTreeItem_OnNameVisibleChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Console.WriteLine($"STACK: {sender.GetHashCode()}  {e}");

            var vm = WPFHelper.GetDataContext<ProjectTreeItemViewModel>(sender);
            if (vm == null)
                return;
            var seb = ContainerLocator.Current.Resolve<SimpleEditBox>();
            if (vm.IsEditingName == false)
            {
                seb.RemoveFromPanel();
                return;
            }

            var stack = WPFHelper.GetVisualParent<StackPanel>(sender);
            if (stack == null) return;

            bool visible = (bool)e.NewValue;
            
            if (visible)
            {
                stack.Children.Remove(seb);
            }
            else
            {
                seb.AddToPanel(stack);

                if (seb.DataContext is SimpleEditBoxViewModel sebVm)
                {
                    sebVm.Text = vm.Name;
                    seb.SelectText(vm.Name);
                }

                seb.SetFocus();
                seb.OnExit = (box, save) =>
                {
                    var sebVm = box.DataContext as SimpleEditBoxViewModel;
                    if (sebVm == null)
                        return;
                    var text = sebVm.Text;
                    vm.ExitingRenameMode(text, save);
                };
            }
        }
    }
}
