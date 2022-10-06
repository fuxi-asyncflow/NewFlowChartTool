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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlowChartCommon;
using NFCT.Graph.ViewModels;
using NFCT.Common;
using Prism.Ioc;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for TextNode.xaml
    /// </summary>
    public partial class TextNode : UserControl
    {
        public TextNode()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

           
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var nodeVm = DataContext as TextNodeViewModel;
            if (nodeVm == null) return;
            nodeVm.DebugStatusChangeEvent += OnNodeDebugStatusChange;
        }

        private void NodeGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            // 防止在节点上右键拖动
            //if (e.RightButton == MouseButtonState.Pressed)
            //{
            //    e.Handled = true;
            //}
            //else if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    Logger.DBG($"nodegrid mouse move {e.GetPosition((UIElement)sender)}");
            //}
        }

        private void NodeTexts_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var nodeVm = WPFHelper.GetDataContext<TextNodeViewModel>(this);
            if (nodeVm == null) return;

            bool visible = (bool)e.NewValue;
            var ac = ContainerLocator.Current.Resolve<NodeAutoComplete>();
            if (visible)
            {
                ac.RemoveFromPanel();
            }
            else
            {
                ac.AddToPanel(NodeStack);
                // show autocomplete
                if (ac.DataContext is NodeAutoCompleteViewModel acVm)
                {
                    acVm.Node = nodeVm;
                    acVm.Text = nodeVm.Text;
                }

                ac.SetFocus();
                ac.SetCursor(-1);
            }
        }

        private void OnNodeDebugStatusChange(DebugNodeStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                ColorAnimation anim;
                switch (status)
                {
                    case DebugNodeStatus.SUCCESS:
                        NodeBorder.Background = new SolidColorBrush(Colors.GreenYellow);
                        anim = new ColorAnimation(Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(200)));
                        NodeBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                        break;
                    case DebugNodeStatus.FAILURE:
                        NodeBorder.Background = new SolidColorBrush(Colors.Red);
                        anim = new ColorAnimation(Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(200)));
                        NodeBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                        break;
                    case DebugNodeStatus.RUNNING:
                        NodeBorder.Background = new SolidColorBrush(Colors.Blue);
                        break;
                }
            });
        }
    }
}

    
