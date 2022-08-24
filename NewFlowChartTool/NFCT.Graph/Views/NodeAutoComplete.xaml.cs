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
using System.Windows.Threading;
using FlowChartCommon;
using NFCT.Common;
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for NodeAutoComplete.xaml
    /// </summary>
    public partial class NodeAutoComplete : UserControl
    {
        public NodeAutoComplete()
        {
            InitializeComponent();
        }

        public Panel? ParentPanel { get; set; }

        public void AddToPanel(Panel panel)
        {
            RemoveFromPanel();
            ParentPanel = panel;
            ParentPanel.Children.Add(this);
        }

        public void RemoveFromPanel()
        {
            if (ParentPanel == null)
                return;
            ParentPanel.Children.Remove(this);
            ParentPanel = null;
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                EditBox.Focus();
                Keyboard.Focus(EditBox);
            }), DispatcherPriority.Render);
        }

        public void SelectText(string s)
        {
            //EditBox.SelectionStart = 0;
            //EditBox.SelectionLength = s.Length;
            //EditBox.SelectAll();
        }

        // -1 - end
        public void SetCursor(int pos)
        {
            if (pos < 0)
                EditBox.CaretIndex = EditBox.Text.Length;
        }

        public Action<NodeAutoComplete, bool> OnExit = (complete, b) =>
        {
            var vm = complete.DataContext as NodeAutoCompleteViewModel;
            if (vm == null) return;

            vm.Node.ExitEditingMode(vm, b);
            
            // set focus back
            vm.Node.IsFocused = false;
            vm.Node.IsFocused = true;
        };

        private void EditBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            OnExit?.Invoke(this, true);
        }

        private void EditBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            Logger.WARN($"node editbox key down: {e.Key}");
            if (e.Key == Key.Enter)
            {
                OnExit?.Invoke(this, true);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                Logger.WARN($"prompts: {PromptsListBox.SelectedIndex}");
                PromptsListBox.SelectedIndex++;
                PromptsListBox.ScrollIntoView(PromptsListBox.SelectedItem);
            }
            else if (e.Key == Key.Up)
            {
                if (PromptsListBox.SelectedIndex < 1) return;
                PromptsListBox.SelectedIndex--;
                PromptsListBox.ScrollIntoView(PromptsListBox.SelectedItem);
            }
            else if (e.Key == Key.Escape)
                OnExit?.Invoke(this, false);

        }

        private void EditBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var newVisibility = (bool)e.NewValue;
            if (newVisibility)
            {
                PromptsListBox.Focus();
                Keyboard.Focus(PromptsListBox);
                return;
            }
        }
    }
}
