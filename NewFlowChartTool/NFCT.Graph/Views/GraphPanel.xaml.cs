﻿using System;
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
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphPanel.xaml
    /// </summary>
    public partial class GraphPanel : UserControl
    {
        public GraphPanel()
        {
            InitializeComponent();
        }

        private void Graph_LayoutUpdated(object sender, EventArgs e)
        {
            GraphPaneViewModel? vm = DataContext as GraphPaneViewModel;
            if (vm == null) return;

            if (vm.NeedLayout)
            {
                vm.NeedLayout = false;
                vm.Relayout();
            }
        }
    }
}
