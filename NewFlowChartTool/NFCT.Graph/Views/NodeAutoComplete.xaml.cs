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
using FlowChart.Type;
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
            if(FuncInfoToolTip.IsOpen)
                FuncInfoToolTip.IsOpen = false;
            if(AutoCompletePopup.IsOpen)
                AutoCompletePopup.IsOpen = false;
            OnExit?.Invoke(this, true);
        }

        private void EditBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            Logger.DBG($"node editbox key down: {e.Key}");
            if (e.Key == Key.Enter)
            {
                if (AutoCompletePopup.IsOpen && PromptsListBox.SelectedItem != null)
                {
                    ReplaceWithPrompt((PromptItemViewModel)(PromptsListBox.SelectedItem));
                    AutoCompletePopup.IsOpen = false;
                }
                else
                {
                    OnExit?.Invoke(this, true);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                Logger.DBG($"prompts: {PromptsListBox.SelectedIndex}");
                PromptsListBox.SelectedIndex++;
                PromptsListBox.ScrollIntoView(PromptsListBox.SelectedItem);
            }
            else if (e.Key == Key.Up)
            {
                if (PromptsListBox.SelectedIndex < 1) return;
                PromptsListBox.SelectedIndex--;
                PromptsListBox.ScrollIntoView(PromptsListBox.SelectedItem);
            }
            else if (e.Key == Key.Tab)
            {
                if (PromptsListBox.SelectedIndex == -1)
                    PromptsListBox.SelectedIndex = 0;
                var item = PromptsListBox.SelectedItem as PromptItemViewModel;
                if (item != null)
                {
                    ReplaceWithPrompt(item);
                }
                e.Handled = true;
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

        private int _oldPosition = -1;
        public void OnCursorPositionChange(int newPosition)
        {
            if (_oldPosition == newPosition) return;
            _oldPosition = newPosition;
            var vm = DataContext as NodeAutoCompleteViewModel;
            if(vm == null) return;

            //if (!vm.IsEditing)
            //{
            //    Logger.ERR(string.Format("node is not in editing when actb is shown: {0} {1}->{2}", vm.Command,
            //        _oldPosition, newPosition));
            //}
            Logger.DBG("cur pos " + newPosition);
            string currentText = vm.Text;
            if (_oldPosition < 0)
            {
                _oldPosition = 0;   // 保证下面的substring不会出错
            }

            if (_oldPosition > currentText.Length)
            {
                return;
            }
            string leftCmd = currentText.Substring(0, _oldPosition);
            string rightCmd = currentText.Substring(_oldPosition);
            vm.PreparePromptList(leftCmd, rightCmd);

            if (!AutoCompletePopup.IsOpen)
            {
                AutoCompletePopup.IsOpen = true;
            }

            PromptsListBox.SelectedIndex = -1;

            if (vm.OutsideFuncInfo != null)
            {
                SetActbTooltipContent(vm.OutsideFuncInfo, vm.ParameterPos);
                FuncInfoToolTip.PlacementTarget = EditBox;
                FuncInfoToolTip.IsOpen = true;
            }
            else
            {
                FuncInfoToolTip.IsOpen = false;
            }

        }

        private void EditBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            int curPos = -1;
            if (EditBox.SelectionLength == 0)
            {
                curPos = EditBox.SelectionStart;
            }
            OnCursorPositionChange(curPos);
        }

        private void ReplaceWithPrompt(PromptItemViewModel prompt)
        {
            string currentText = EditBox.Text;
            string leftCmd = currentText.Substring(0, _oldPosition);
            string rightCmd = currentText.Substring(_oldPosition);
            var vm = DataContext as NodeAutoCompleteViewModel;
            if (vm == null) return;
            int curPos = 0;
            EditBox.Text = vm.ApplyPrompt(prompt, ref curPos);

            AutoCompletePopup.IsOpen = false;
            if (PromptsListBox.Items.Count > 0)
            {
                PromptsListBox.ScrollIntoView(PromptsListBox.Items[0]);
            }
            // Set Cursor Position
            EditBox.SelectionStart = curPos;
            EditBox.SelectionLength = 0;
            OnCursorPositionChange(curPos);
            // 记录该接口已被使用
            PromptItemViewModel.UsedPrompts.Add(prompt.Text.ToLower());
            prompt.UseCount++;
        }

        private void SetActbTooltipContent(Method func, int pos)
        {
            var inlines = FuncInfoToolTipText.Inlines;
            inlines.Clear();
            inlines.Add(new Run(func.Name + "( \n"));
            int i = 0;
            foreach (var member in func.Parameters)
            {
                string paraStr = string.Format("{0} {1}", member.Type, member.Name);
                if (!string.IsNullOrEmpty(member.Default))
                {
                    paraStr = String.Format("[{0} = {1}]", paraStr, member.Default);
                }
                paraStr = paraStr + ", " + member.Description + "\n";
                inlines.Add(new Run(paraStr)
                {
                    Foreground = (i == pos) ? Brushes.Red : Brushes.Black,
                    FontSize = (i == pos) ? 16 : 12,
                });
                i++;
            }
            inlines.Add(new Run(")"));
        }
    }
}
