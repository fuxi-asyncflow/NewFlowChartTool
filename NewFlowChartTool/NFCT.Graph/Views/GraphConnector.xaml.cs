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

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphConnector.xaml
    /// </summary>
    public partial class GraphConnector : UserControl
    {
        public GraphConnector()
        {
            InitializeComponent();
        }

        private void ConnectorPath_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
