using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FlowChart.Common;
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphVariablesPanel.xaml
    /// </summary>
    public partial class GraphVariablesPanel : UserControl
    {
        public GraphVariablesPanel()
        {
            InitializeComponent();
        }


        private bool _isResizing;
        private Point _startPoint;
        private double _originWidth;
        private double _originHeight;
        private void ResizeCorner_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing && e.LeftButton == MouseButtonState.Pressed)
            {
                var curPos = e.GetPosition(VariablesPaneGrid);
                var deltaPos = curPos - _startPoint;
                VariablesPaneGrid.Width = _originWidth + deltaPos.X;
                VariablesPaneGrid.Height = _originHeight + deltaPos.Y;
            }
        }

        private void ResizeCorner_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isResizing = true;
            Logger.DBG("begin resize variables pane");
            _startPoint = e.GetPosition(VariablesPaneGrid);
            _originWidth = VariablesPaneGrid.Width;
            _originHeight = VariablesPaneGrid.Height;
        }

        private void ResizeCorner_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isResizing = false;
            Logger.DBG("exit resize variables pane");

        }

        private void ResizeCorner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _isResizing = false;
            Logger.DBG("exit resize variables pane");
        }
    }
}
