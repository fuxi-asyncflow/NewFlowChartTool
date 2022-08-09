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

namespace NFCT.Common.Views
{
    /// <summary>
    /// Interaction logic for SimpleEditBox.xaml
    /// </summary>
    public partial class SimpleEditBox : UserControl
    {
        public SimpleEditBox()
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
                this.Focus();
                Keyboard.Focus(this);
            }), DispatcherPriority.Render);
        }

        public void SelectText(string s)
        {
            //EditBox.SelectionStart = 0;
            //EditBox.SelectionLength = s.Length;
            EditBox.SelectAll();
        }

        public Action<SimpleEditBox, bool> OnExit;

        private void EditBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            OnExit(this, true);
        }

        private void EditBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                OnExit(this, true);
            else if (e.Key == Key.Escape)
                OnExit(this, false);
        }
    }
}
