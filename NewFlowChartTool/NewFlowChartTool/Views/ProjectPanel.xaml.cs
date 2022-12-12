using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using FlowChartCommon;
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
            var vm = DataContext as ProjectPanelViewModel;
            if (vm == null)
                return;
            vm.AddGraphEvent += OnAddTreeItem;
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

        private void OnAddTreeItem(ProjectTreeItemViewModel item)
        {
            item.IsSelected = true;
            item.IsEditingName = true;
            BringTreeItemIntoView(ProjectTree, item);
        }

        #region FUNCTION USED TO BRINGINTOVIEW
        // 对于virtualizing的treeview，实现BringTreeItemIntoView
        // http://stackoverflow.com/questions/183636/selecting-a-node-in-virtualized-treeview-with-wpf
        public static void BringTreeItemIntoView(TreeView treeView, object? item)
        {
            var selectedItem = item as ProjectTreeItemViewModel;
            if (selectedItem == null || selectedItem.Parent == null) return;
            // 获得从根节点到选中项的完整路径
            var itemList = new List<ProjectTreeItemViewModel>();
            var iteritem = selectedItem;
            while (iteritem != null)
            {
                itemList.Add(iteritem);
                iteritem = iteritem.Parent;
            }

            itemList.RemoveAt(itemList.Count - 1);
            itemList.Reverse();

            var currentTree = treeView as ItemsControl;
            foreach (var treeItem in itemList)
            {
                // 从treeview中，寻找treeItem对应的控件
                var nextTree = currentTree.ItemContainerGenerator.ContainerFromItem(treeItem) as TreeViewItem;
                // 如果找不到，那么说明由于虚拟化技术，该节点并没有存在,这时就比较麻烦了
                // 首先找到ItemsHost
                if (nextTree == null)
                {
                    currentTree.ApplyTemplate();
                    var itemsPresenter = currentTree.Template.FindName("ItemsHost", currentTree) as ItemsPresenter;
                    if (itemsPresenter != null)
                    {
                        itemsPresenter.ApplyTemplate();
                    }
                    else
                    {
                        currentTree.UpdateLayout();
                    }
                    // 然后找到VirtualizingPanel
                    var virtualizingPanel = GetItemsHost(currentTree) as VirtualizingPanel;
                    CallEnsureGenerator(virtualizingPanel);
                    // 在virtualizingPanel里面找到相应的Item
                    var index = currentTree.Items.IndexOf(treeItem);
                    if (index >= 0)
                    {
                        CallBringIndexIntoView(virtualizingPanel, index);
                        nextTree = currentTree.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                    }
                }

                if (nextTree == null)
                {
                    Logger.ERR("Tree view item cannot be found or created for node '" + treeItem.Name + "'");
                    break;
                }

                if (treeItem == selectedItem)
                {
                    nextTree.IsSelected = true;
                    nextTree.BringIntoView();
                    break;
                }

                nextTree.IsExpanded = true;
                currentTree = nextTree;
            }
        }

        #region Functions to get internal members using reflection

        // Some functionality we need is hidden in internal members, so we use reflection to get them

        #region ItemsControl.ItemsHost

        static readonly PropertyInfo ItemsHostPropertyInfo = typeof(ItemsControl).GetProperty("ItemsHost", BindingFlags.Instance | BindingFlags.NonPublic);

        private static Panel GetItemsHost(ItemsControl itemsControl)
        {
            Debug.Assert(itemsControl != null);
            return ItemsHostPropertyInfo.GetValue(itemsControl, null) as Panel;
        }

        #endregion ItemsControl.ItemsHost

        #region Panel.EnsureGenerator

        private static readonly MethodInfo EnsureGeneratorMethodInfo = typeof(Panel).GetMethod("EnsureGenerator", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void CallEnsureGenerator(Panel panel)
        {
            Debug.Assert(panel != null);
            EnsureGeneratorMethodInfo.Invoke(panel, null);
        }

        #endregion Panel.EnsureGenerator

        #region VirtualizingPanel.BringIndexIntoView

        private static readonly MethodInfo BringIndexIntoViewMethodInfo = typeof(VirtualizingPanel).GetMethod("BringIndexIntoView", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void CallBringIndexIntoView(VirtualizingPanel virtualizingPanel, int index)
        {
            Debug.Assert(virtualizingPanel != null);
            BringIndexIntoViewMethodInfo.Invoke(virtualizingPanel, new object[] { index });
        }

        #endregion VirtualizingPanel.BringIndexIntoView

        #endregion Functions to get internal members using reflection

        #endregion

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
        private void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as ProjectPanelViewModel;
            if (vm == null)
                return;
            //Logger.DBG($"node editbox key down: {e.Key}");
            if (e.Key == Key.Enter)
            {
                vm.OpenSelectedSearchResult();
                vm.ExitSearch();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                //Logger.DBG($"prompts: {PromptsListBox.SelectedIndex}");
                SearchResultListBox.SelectedIndex++;
                SearchResultListBox.ScrollIntoView(SearchResultListBox.SelectedItem);
            }
            else if (e.Key == Key.Up)
            {
                if (SearchResultListBox.SelectedIndex < 1) return;
                SearchResultListBox.SelectedIndex--;
                SearchResultListBox.ScrollIntoView(SearchResultListBox.SelectedItem);
            }
            else if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.ExitSearch();
            }
        }

        // cannot use MouseDown, because it will be handled by list box item
        private void SearchResultListBox_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as ProjectPanelViewModel;
            if (vm == null)
                return;

            vm.OpenSelectedSearchResult();
            BringTreeItemIntoView(ProjectTree, vm.SelectedSearchItem);
            vm.ExitSearch();
            e.Handled = true;

        }
    }
}
